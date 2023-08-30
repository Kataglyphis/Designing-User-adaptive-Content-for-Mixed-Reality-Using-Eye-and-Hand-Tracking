// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using AIServer.Datasets;
using SharedResultsBetweenServerAndHoloLens;
using TorchSharp;
using Windows.AI.MachineLearning;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Shapes;
using static TorchSharp.torch;


namespace AIServer
{
    /**
    <summary>
        This is an approach similar to YOLACT Real Time Instance Segmentation: https://arxiv.org/abs/1904.02689
    </summary>
    <remarks>
        the output0 layout ia as follows; one detection result has #outputEntrySize[0] entries
        <list type= "table">
            <listheader>
                <term>index</term>
                <term>content</term>
            </listheader>
            <item>
                <term>0</term>
                <description> center point x-coordinate(unnormalized --> value range is [0, inputWidth])</description>
            </item>
            <item>
                <term>1</term>
                <description>center point y-coordinate(unnormalized --> value range is [0, inputHeight])</description>
            </item>
            <item>
                <term>2</term>
                <description>width(unnormalized --> value range is [0, inputWidth])</description>
            </item>
            <item>
                <term>3</term>
                <description>height(unnormalized --> value range is [0, inputHeight])</description>
            </item>
            <item>
                <term>4</term>
                <description>objectness score</description>
            </item>
            <item>
                <term>5 ... 5 + (dataset.c_classes - 1)</term>
                <description>class probabilities</description>
            </item>
            <item>
                <term>5 + (dataset.c_classes - 1) ... 5 + (dataset.c_classes - 1) + numPrototypes</term>
                <description>class probabilities</description>
            </item>
        </list>
        output1 layout is as follows; (has #outputEntrySize[1] entries)
        <list type="table">
            <listheader>
                <term>index</term>
                <term>content</term>
            </listheader>
            <item>
                <term> 0 ... numPrototypes</term>
                <description>160x160 output mask</description>
            </item>
        </list>
    </remarks>
    */

    class Yolov5Seg : YoloInstanceSegmentation
    {

        static int maxResults = 4;

        protected override List<uint> OutputEntrySizes
        {
            get
            {
                List<uint> neuralNetOutputEntrySizes = new List<uint>();
                neuralNetOutputEntrySizes.Add((uint)Dataset.Labels.Length + 5 + NumPrototypes);
                neuralNetOutputEntrySizes.Add(NumPrototypes);
                Debug.Assert(neuralNetOutputEntrySizes.Count == _outputs.Length);
                return neuralNetOutputEntrySizes;
            }

        }

        /**
        <value>
        have a look in the paper mentioned above for explainations what prototypes are
        </value>
        * */
        private const uint NumPrototypes = 32;

        /**
         <value>The width of our mask output</value>
         */
        private uint _maskOutputSizeX;

        /**
         <value>The height of our mask output</value>
         */
        private uint _maskOutputSizeY;

        public uint MaskOutputSizeX { get { return _maskOutputSizeX; } }
        public uint MaskOutputSizeY { get { return _maskOutputSizeY; } }

        async public static Task<Yolov5Seg> CreateAsync(string modelAssetFile, uint inputWidth, uint inputHeight, uint inputDepth,
                                                        string[] inputs, string[] outputs, Dataset dataset,
                                                        float confidence,
                                                        uint maskOutputSizeX, uint maskOutputSizeY)
        {
            Yolov5Seg newInstance = new Yolov5Seg(modelAssetFile, inputWidth, inputHeight, inputDepth, inputs,
                                                    outputs, dataset, confidence,
                                                    maskOutputSizeX, maskOutputSizeY);
            await newInstance.LoadModelAsync();
            return newInstance;
        }

        private Yolov5Seg(string modelAssetFile, uint inputWidth, uint inputHeight, uint inputDepth,
                            string[] inputs, string[] outputs, Dataset dataset,
                            float confidence,
                            uint maskOutputSizeX, uint maskOutputSizeY)
                            : base(modelAssetFile, inputWidth, inputHeight, inputDepth,
                                    inputs, outputs, dataset, confidence)
        {
            this._maskOutputSizeX = maskOutputSizeX;
            this._maskOutputSizeY = maskOutputSizeY;
        }

        public override List<InstanceSegmentationResult> EvaluateFrame(VideoFrame vf)
        {
            // all important stuff is listet here: https://learn.microsoft.com/de-de/windows/ai/windows-ml/bind-a-model
            // we have to normalize our frame input first; so you HAVE to first create a normalized tensor!
            var frameTensor = TensorUtils.ExtractPixelsAndNormalize(vf);

            binding.Clear();

            binding.Bind(_inputs[0], frameTensor);
            var results = session.Evaluate(binding, "");

            List<float[]> outputs = new List<float[]>();
            for (int i = 0; i < _outputs.Length; i++)
            {
                TensorFloat result = results.Outputs[_outputs[i]] as TensorFloat;
                outputs.Add(result.GetAsVectorView().ToArray());
            }

            List<InstanceSegmentationResult> detections = ParseResult(outputs);

            return detections;

        }

        protected List<InstanceSegmentationResult> NMS(IReadOnlyList<InstanceSegmentationResult> detections,
            float IOU_threshold = 0.25f)
        {
            List<InstanceSegmentationResult> final_detections = new List<InstanceSegmentationResult>();
            for (int i = 0; i < detections.Count; i++)
            {
                int j = 0;
                for (j = 0; j < final_detections.Count; j++)
                {
                    if (ComputeIOU(final_detections[j], detections[i]) > IOU_threshold)
                    {
                        break;
                    }
                }
                if (j == final_detections.Count)
                {
                    final_detections.Add(detections[i]);
                }
            }
            return final_detections;
        }

        public List<InstanceSegmentationResult> ParseResult(List<float[]> results)
        {

            // details in YOLOv3 paper about layout: https://pjreddie.com/media/files/papers/YOLOv3.pdf
            // good tutorial for yolov5:
            // https://learnopencv.com/object-detection-using-yolov5-and-opencv-dnn-in-c-and-python/
            var objectDetectionOutputSize = OutputEntrySizes[0];
            var segmentationOutputSize = OutputEntrySizes[1];
            uint c_anchor_boxes = (uint)results[0].Length / objectDetectionOutputSize;

            List<InstanceSegmentationResult> detections = new List<InstanceSegmentationResult>();

            for (uint i_box = 0; i_box < c_anchor_boxes; i_box++)
            {
                uint index = i_box * objectDetectionOutputSize;
                float objectness_score = results[0][index + 4];

                if (objectness_score > Confidence)
                {

                    int label_index = 0;
                    float max_prob = 0;

                    for (int i_classes = 0; i_classes < Dataset.Labels.Length; i_classes++)
                    {

                        if (results[0][index + 5 + i_classes] > max_prob)
                        {
                            max_prob = results[0][index + 5 + i_classes];
                            label_index = i_classes;
                        }

                    }

                    float resulting_prob = max_prob * objectness_score;

                    if (resulting_prob > Confidence)
                    {

                        List<float> bbox = new List<float>();

                        float center_x = results[0][index + 0];
                        float center_y = results[0][index + 1];
                        float width = results[0][index + 2];
                        float height = results[0][index + 3];

                        // be aware of our detection result layout
                        bbox.Add(center_y - height / 2f); // top
                        bbox.Add(center_x - width / 2f); // left
                        bbox.Add(center_y + height / 2f); // bottom
                        bbox.Add(center_x + width / 2f); // right

                        if (bbox[1] == bbox[3]) continue;

                        List<float> masks = new List<float>();
                        for (int j = 0; j < segmentationOutputSize; j++)
                        {
                            masks.Add(results[0][index + 5 + Dataset.Labels.Length + j]);
                        }

                        detections.Add(new InstanceSegmentationResult(
                            Dataset.Labels[label_index],
                            resulting_prob,
                            bbox,
                            masks,
                            new byte[MaskOutputSizeX * MaskOutputSizeY]
                        ));

                    }

                }
            }

            InstanceSegmentationComparer cp = new InstanceSegmentationComparer();
            detections.Sort(cp);
            List<InstanceSegmentationResult> final_detections = NMS(detections);

            if(final_detections.Count >= maxResults)
            {
                final_detections = final_detections.Take(maxResults).ToList();
            }

            try
            {

                // if there are no detections spare all the computations
                if (final_detections.Count == 0) return final_detections;
                torch.TensorStringStyle = torch.numpy;

                //for(int i = 0; i < 32; i++)
                //{
                //    var mask = new ArraySegment<float>(results[1], i * (int)MaskOutputSizeX * (int)MaskOutputSizeY, (i+1) * (int)MaskOutputSizeX * (int)MaskOutputSizeY); 

                //    // create a byte array and copy the floats into it...
                //    var byteArray = new byte[mask.Array.Length * 4];
                //    Buffer.BlockCopy(mask.Array, 0, byteArray, 0, byteArray.Length);

                //    SoftwareBitmap softwareBitmap = new SoftwareBitmap(BitmapPixelFormat.Gray8, (int)MaskOutputSizeX,
                //                                                (int)MaskOutputSizeY, BitmapAlphaMode.Ignore);
                //    softwareBitmap.CopyFromBuffer(byteArray.AsBuffer());
                //    BitmapUtils.SaveSoftwareBitmapToFile(softwareBitmap);

                //}

                using (var d = torch.NewDisposeScope())
                {
                    // tensor batch processing saves us time
                    var masks_in =
                        torch.stack(
                            (from m in Enumerable.Range(start: 0, count: final_detections.Count)
                             select torch.tensor(final_detections[m].masks.ToArray(),
                                                     device: torch.CUDA, dtype: torch.float32)
                                         .reshape(-1, NumPrototypes)).ToArray()
                        );

                    var protos = torch.tensor(results[1], device: torch.CUDA, dtype: torch.float32)
                                        .reshape(NumPrototypes, (int)MaskOutputSizeX * (int)MaskOutputSizeY);

                    var product = torch.matmul(masks_in, protos);

                    var mask_img = (product[TensorIndex.Slice(), 0, TensorIndex.Slice()]).sigmoid() * Byte.MaxValue;
                    var mask_img_byte_cpu = mask_img.to(ScalarType.Byte).cpu();

                    // this optimization is for avoiding copies of arrays which are very expensive
                    for (int m = 0; m < final_detections.Count; m++)
                    {

                        var auxTensor = torch.as_tensor(final_detections[m].mask_img,
                                                        device: torch.CPU,
                                                        dtype: ScalarType.Byte);

                        auxTensor[TensorIndex.Slice()] = mask_img_byte_cpu[m].flatten();
                        final_detections[m].mask_img = mask_img_byte_cpu[m].data<byte>().ToArray();
                    }
                }

            }
            catch (AggregateException err)
            {
                foreach (var errInner in err.InnerExceptions)
                {
                    // this will call ToString() on the inner execption and get you message,
                    // stacktrace and you could perhaps drill down further into the inner exception of it if necessary 
                    Debug.WriteLine(errInner);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }

            return final_detections;

            // For clarity I will let this implementation as an comment here
            // this version is unoptimized and works with matrix multiplications
            // NuGet package Math.NET is necessary for it
            // if you read this code it might be more clear what is happening in the
            // highly optimized code above which works with tensors

            //for (int m = 0; m < final_detections.Count; m++)
            //{
            //    // this is the unoptimized calculation without tensor calculation
            //    Matrix<float> mask_in = Matrix<float>.Build.DenseOfRowMajor(1, 32, final_detections[m].masks.ToArray());
            //    // you can read the 160 in the output1 of the .onnx file
            //    Matrix<float> protos =
            //        Matrix<float>.Build.DenseOfRowMajor(32, (int)(maskOutputSizeX * maskOutputSizeY), results[1]);

            //    var product = mask_in.Multiply(protos);

            //    final_detections[m].mask_img = new byte[maskOutputSizeX * maskOutputSizeY * 4];
            //    for (int i = 0; i < maskOutputSizeX * maskOutputSizeY; i++)
            //    {
            //        int index = i * 4;
            //        final_detections[m].mask_img[index] = 0; // blue
            //        final_detections[m].mask_img[index + 1] = 0; // green 
            //        final_detections[m].mask_img[index + 2] =
            //            (byte)(SpecialFunctions.Logistic(product[0, i]) * 255.0f); // red
            //        final_detections[m].mask_img[index + 3] = 255; // alpha

            //    }
            //}


        }

    }

}
