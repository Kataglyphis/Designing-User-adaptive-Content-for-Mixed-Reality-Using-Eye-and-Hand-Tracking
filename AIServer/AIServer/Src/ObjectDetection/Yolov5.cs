// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using AIServer.Datasets;
using SharedResultsBetweenServerAndHoloLens;
using Windows.AI.MachineLearning;
using Windows.Media;

namespace AIServer
{
    /**
    <summary>
        
    </summary>
    <remarks>
        see full AI example: 
            https://learn.microsoft.com/en-us/windows/ai/windows-ml/tutorials/tensorflow-deploy-model# </br>
        very good yolov5 example: https://github.com/mentalstack/yolov5-net </br>

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
        </list>
    </remarks>
    */

    class Yolov5 : YoloObjectDetection
    {

        protected override List<uint> OutputEntrySizes
        {
            get
            {
                List<uint> neuralNetOutputEntrySizes = new List<uint>();
                neuralNetOutputEntrySizes.Add((uint)Dataset.Labels.Length + 5);
                Debug.Assert(neuralNetOutputEntrySizes.Count == _outputs.Length);
                return neuralNetOutputEntrySizes;
            }

        }

        async public static Task<Yolov5> CreateAsync(string modelAssetFile, uint inputWidth, uint inputHeight,
                                                        uint inputDepth,
                                                        string[] inputs, string[] outputs, Dataset dataset,
                                                        float confidence)
        {
            Yolov5 newInstance = new Yolov5(modelAssetFile, inputWidth, inputHeight, inputDepth, inputs, outputs, dataset, confidence);
            await newInstance.LoadModelAsync();
            return newInstance;
        }

        private Yolov5(string modelAssetFile, uint inputWidth, uint inputHeight, uint inputDepth,
                        string[] inputs, string[] outputs, Dataset dataset,
                        float confidence)
                        : base(modelAssetFile, inputWidth, inputHeight, inputDepth,
                                inputs, outputs, dataset, confidence)
        {

        }

        public override List<DetectionResult> EvaluateFrame(VideoFrame vf)
        {
            // all important stuff is listet here: https://learn.microsoft.com/de-de/windows/ai/windows-ml/bind-a-model
            // we have to normalize our frame input first; so you HAVE to first create a normalized tensor!
            var frameTensor = TensorUtils.ExtractPixelsAndNormalize(vf);

            binding.Clear();

            binding.Bind(_inputs[0], frameTensor);
            var results = session.Evaluate(binding, "");

            TensorFloat result = results.Outputs[_outputs[0]] as TensorFloat;
            var data = result.GetAsVectorView();
            var data_arr = data.ToArray();

            List<DetectionResult> detections = ParseResult(data_arr);
            DetectionComparer cp = new DetectionComparer();
            detections.Sort(cp);
            List<DetectionResult> final_detections = NMS(detections);

            return final_detections;

        }

        public List<DetectionResult> ParseResult(float[] results)
        {

            // details in YOLOv3 paper about layout: https://pjreddie.com/media/files/papers/YOLOv3.pdf
            // good tutorial for yolov5: https://learnopencv.com/object-detection-using-yolov5-and-opencv-dnn-in-c-and-python/   
            var objectDetectionEntrySize = OutputEntrySizes[0];
            uint c_anchor_boxes = (uint)results.Length / objectDetectionEntrySize;

            List<DetectionResult> detections = new List<DetectionResult>();

            for (uint i_box = 0; i_box < c_anchor_boxes; i_box++)
            {
                uint index = i_box * objectDetectionEntrySize;
                float objectness_score = results[index + 4];

                if (objectness_score > Confidence)
                {

                    int label_index = 0;
                    float max_prob = 0;

                    for (int i_classes = 0; i_classes < Dataset.Labels.Length; i_classes++)
                    {

                        if (results[index + 5 + i_classes] > max_prob)
                        {
                            max_prob = results[index + 5 + i_classes];
                            label_index = i_classes;
                        }

                    }

                    float resulting_prob = max_prob * objectness_score;

                    if (resulting_prob > Confidence)
                    {

                        List<float> bbox = new List<float>();

                        float center_x = results[index + 0];
                        float center_y = results[index + 1];
                        float width = results[index + 2];
                        float height = results[index + 3];

                        // be aware of our detection result layout
                        bbox.Add(center_y - height / 2f); // top
                        bbox.Add(center_x - width / 2f); // left
                        bbox.Add(center_y + height / 2f); // bottom
                        bbox.Add(center_x + width / 2f); // right

                        detections.Add(new DetectionResult()
                        {
                            label = Dataset.Labels[label_index],
                            bbox = bbox,
                            prob = resulting_prob
                        });

                    }
                }
            }
            return detections;
        }

    }
}

