// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        see full AI example: https://learn.microsoft.com/en-us/windows/ai/windows-ml/tutorials/tensorflow-deploy-model#
        https://github.com/microsoft/Windows-Machine-Learning/tree/master/Samples/Tutorial%20Samples/YOLOv4ObjectDetection
        integrate tensorflow into uwp: https://learn.microsoft.com/en-us/windows/ai/windows-ml/tutorials/tensorflow-deploy-model

        the output0 layout ia as follows; one detection result has #outputEntrySize[0] entries
        <list type= "table">
            <listheader>
                <term>index</term>
                <term>content</term>
            </listheader>
            <item>
                <term>0</term>
                <description>top (normalized --> value range [0,1])</description>
            </item>
            <item>
                <term>1</term>
                <description>left (normalized --> value range [0,1])</description>
            </item>
            <item>
                <term>2</term>
                <description>bottom (normalized --> value range [0,1])</description>
            </item>
            <item>
                <term>3</term>
                <description>right (normalized --> value range [0,1])</description>
            </item>
            <item>
                <term>4 ... 4 + (dataset.c_classes - 1)</term>
                <description>objectness score</description>
            </item>
        </list>
    </remarks>
    */
    class Yolov4 : YoloObjectDetection
    {

        protected override List<uint> OutputEntrySizes
        {
            get
            {
                List<uint> neuralNetOutputEntrySizes = new List<uint>();
                neuralNetOutputEntrySizes.Add((uint)Dataset.Labels.Length + 4);
                Debug.Assert(neuralNetOutputEntrySizes.Count == _outputs.Length);
                return neuralNetOutputEntrySizes;
            }

        }

        public Yolov4(string modelAssetFile, uint inputWidth, uint inputHeight, uint inputDepth,
                        string[] inputs, string[] outputs, Dataset dataset, float confidence)
                        : base(modelAssetFile, inputWidth, inputHeight, inputDepth,
                                inputs, outputs, dataset, confidence)
        {

        }

        public override List<DetectionResult> EvaluateFrame(VideoFrame vf)
        {

            binding.Clear();

            binding.Bind(_inputs[0], vf);
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
            var objectDetectionEntrySize = OutputEntrySizes[0];
            uint c_boxes = (uint)results.Length / objectDetectionEntrySize;

            List<DetectionResult> detections = new List<DetectionResult>();
            for (uint i_box = 0; i_box < c_boxes; i_box++)
            {
                float max_prob = 0.0f;
                int label_index = -1;
                for (uint j_confidence = 4; j_confidence < objectDetectionEntrySize; j_confidence++)
                {
                    uint index = i_box * objectDetectionEntrySize + j_confidence;
                    if (results[index] > max_prob)
                    {
                        max_prob = results[index];
                        label_index = (int)j_confidence - 4;
                    }
                }
                if (max_prob > Confidence)
                {
                    List<float> bbox = new List<float>();
                    // top
                    bbox.Add(results[i_box * objectDetectionEntrySize + 0] * InputHeight);
                    // left
                    bbox.Add(results[i_box * objectDetectionEntrySize + 1] * InputWidth);
                    // bottom
                    bbox.Add(results[i_box * objectDetectionEntrySize + 2] * InputHeight);
                    // right
                    bbox.Add(results[i_box * objectDetectionEntrySize + 3] * InputWidth);

                    detections.Add(new DetectionResult()
                    {
                        label = Dataset.Labels[label_index],
                        bbox = bbox,
                        prob = max_prob
                    });
                }
            }
            return detections;
        }

    }
}


