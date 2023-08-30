// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AIServer.Datasets;
using SharedResultsBetweenServerAndHoloLens;
using Windows.AI.MachineLearning;
//using MathNet.Numerics.LinearAlgebra;
using Windows.Media;

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
                <description>center point x-coordinate (unnormalized --> value range is [0,inputWidth])</description>
            </item>
            <item>
                <term>1</term>
                <description>center point y-coordinate (unnormalized --> value range is [0,inputHeight])</description>
            </item>
            <item>
                <term>2</term>
                <description>width (unnormalized --> value range is [0,inputWidth])</description>
            </item>
            <item>
                <term>3</term>
                <description>height (unnormalized --> value range is [0,inputHeight])</description>
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
    class Yolov7Mask : YoloInstanceSegmentation
    {

        protected override List<uint> OutputEntrySizes
        {
            get
            {
                List<uint> neuralNetOutputEntrySizes = new List<uint>();
                neuralNetOutputEntrySizes.Add((uint)Dataset.Labels.Length + 5 + 32);
                neuralNetOutputEntrySizes.Add(32);
                Debug.Assert(neuralNetOutputEntrySizes.Count == _outputs.Length);
                return neuralNetOutputEntrySizes;
            }

        }

        public Yolov7Mask(string modelAssetFile, uint inputWidth, uint inputHeight, uint inputDepth,
                            string[] inputs, string[] outputs, Dataset dataset, float confidence)
                            : base(modelAssetFile, inputWidth, inputHeight, inputDepth,
                                    inputs, outputs, dataset, confidence)
        {

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

        public List<InstanceSegmentationResult> NMS(IReadOnlyList<InstanceSegmentationResult> detections,
            float iou_threshold = 0.25f)
        {
            List<InstanceSegmentationResult> final_detections = new List<InstanceSegmentationResult>();
            for (int i = 0; i < detections.Count; i++)
            {
                int j = 0;
                for (j = 0; j < final_detections.Count; j++)
                {
                    if (ComputeIOU(final_detections[j], detections[i]) > iou_threshold)
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
            // good tutorial for yolov5: https://learnopencv.com/object-detection-using-yolov5-and-opencv-dnn-in-c-and-python/
            uint objectDetectionEntrySize = OutputEntrySizes[0];
            uint segmentationOutputSize = OutputEntrySizes[1];
            uint c_anchor_boxes = (uint)results[0].Length / objectDetectionEntrySize;

            List<InstanceSegmentationResult> detections = new List<InstanceSegmentationResult>();

            for (uint i_box = 0; i_box < c_anchor_boxes; i_box++)
            {
                uint index = i_box * objectDetectionEntrySize;
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

                        List<float> masks = new List<float>();
                        for (int j = 0; j < segmentationOutputSize; j++)
                        {
                            masks.Add(results[0][index + 5 + Dataset.Labels.Length + j]);
                        }

                        detections.Add(new InstanceSegmentationResult(
                            Dataset.Labels[label_index],
                            resulting_prob,
                            bbox,
                            masks
                        ));

                    }

                }
            }

            InstanceSegmentationComparer cp = new InstanceSegmentationComparer();
            detections.Sort(cp);
            List<InstanceSegmentationResult> final_detections = NMS(detections);

            // TODO: Implement main function
            for (int m = 0; m < final_detections.Count; m++)
            {
                //Matrix<float> mask_in = Matrix<float>.Build.Dense(1, 32, detections[m].masks.ToArray());
                //// you can read the 160 in the output1 of the .onnx file
                //Matrix<float> protos = Matrix<float>.Build.Dense(32, (int)(maskOutputSizeX * maskOutputSizeY), results[1]);
                //var product = mask_in.Multiply(protos);
                //List<float> result = new List<float>();
                //for (int k = 0; k < product.ColumnCount; k++)
                //{
                //    result.Add(Sigmoid(product[0, k]));
                //}

                //float[] upscaledArr = Scale(result.ToArray(), 4, 4);
                //for (int i = 0; i < inputWidth * inputHeight; i++)
                //{
                //    if (upscaledArr[i] > 0.5f)
                //    {
                //        upscaledArr[i] = 1f;
                //    }
                //    else
                //    {
                //        upscaledArr[i] = 0f;
                //    }
                //}

                //TensorFloat mask_in = TensorFloat.CreateFromArray(new[] { (long)1, (long)1, (long)1, (long)detections[m].masks.Count }, detections[m].masks.ToArray());
                //TensorFloat protos = TensorFloat.CreateFromArray(new[] { (long)1, (long)1, (long)detections[m].masks.Count, 160*160 }, results[1]);

                //mask_in * protos;
            }

            return final_detections;
        }

        public static float Sigmoid(float x)
        {

            return 1f / (1f + (float)Math.Exp(-x));

        }
    }

}
