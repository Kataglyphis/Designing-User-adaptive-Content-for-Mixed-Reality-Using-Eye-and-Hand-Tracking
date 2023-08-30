// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
        output layout
        <list type= "table">
            <listheader>
                <term>index</term>
                <term>content</term>
            </listheader>
            <item>
                <term>0</term>
                <description>batch id</description>
            </item>
            <item>
                <term>1</term>
                <description>left x1 coordinate; (unnormalized --> values can be out of bounce)</description>
            </item>
            <item>
                <term>2</term>
                <description>top  y1 coordinate; (unnormalized --> values can be out of bounce)</description>
            </item>
            <item>
                <term>3</term>
                <description>right x2 coordinate; (unnormalized --> values can be out of bounce)</description>
            </item>
            <item>
                <term>4</term>
                <description>bottom y2 coordinate; (unnormalized --> values can be out of bounce)</description>
            </item>
            <item>
                <term>5</term>
                <description>label index</description>
            </item>
            <item>
                <term>6</term>
                <description>probability</description>
            </item>
        </list>
    </remarks>
    */
    class Yolov7 : YoloObjectDetection
    {

        protected override List<uint> OutputEntrySizes
        {
            get
            {
                List<uint> neuralNetOutputEntrySizes = new List<uint>();
                neuralNetOutputEntrySizes.Add(7);
                Debug.Assert(neuralNetOutputEntrySizes.Count == _outputs.Length);
                return neuralNetOutputEntrySizes;
            }

        }

        public Yolov7(string modelAssetFile, uint inputWidth, uint inputHeight, uint inputDepth,
                        string[] inputs, string[] outputs, Dataset dataset, float confidence)
                        : base(modelAssetFile, inputWidth, inputHeight,
                                inputDepth, inputs, outputs, dataset, confidence)
        {

        }

        public override List<DetectionResult> EvaluateFrame(VideoFrame vf)
        {
            // you can use Netron for visualizing .onnx nets; there you find the naming for in-&outputs
            // https://github.com/lutzroeder/netron
            // all important stuff is listet here: https://learn.microsoft.com/de-de/windows/ai/windows-ml/bind-a-model
            // we have to normalize our frame input first; so you HAVE to first create a normalized tensor!
            var frameTensor = TensorUtils.ExtractPixelsAndNormalize(vf);

            binding.Clear();

            binding.Bind(_inputs[0], frameTensor);
            var results = session.Evaluate(binding, "");

            TensorFloat result = results.Outputs[_outputs[0]] as TensorFloat;
            var data = result.GetAsVectorView();
            float[] data_arr = data.ToArray();

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
            uint objectDetectionEntrySize = OutputEntrySizes[0];
            uint c_anchor_boxes = (uint)results.Length / objectDetectionEntrySize;

            List<DetectionResult> detections = new List<DetectionResult>();

            for (uint i_box = 0; i_box < c_anchor_boxes; i_box++)
            {
                uint index = i_box * objectDetectionEntrySize;

                int batch_id = (int)results[index];
                int label_index = (int)results[index + 5];
                float max_prob = results[index + 6];


                if (max_prob > Confidence)
                {

                    List<float> bbox = new List<float>();

                    bbox.Add(Math.Clamp(results[index + 1 + 1], 0f, InputHeight)); // top
                    bbox.Add(Math.Clamp(results[index + 1 + 0], 0f, InputWidth)); // left
                    bbox.Add(Math.Clamp(results[index + 1 + 3], 0f, InputHeight)); // bottom
                    bbox.Add(Math.Clamp(results[index + 1 + 2], 0f, InputWidth)); // right

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
