// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using AIServer.Datasets;
using SharedResultsBetweenServerAndHoloLens;
using Windows.Media;

namespace AIServer
{
    /**
    <summary>
        We also want real time instance segmentation.
    </summary>
     */
    public abstract class YoloInstanceSegmentation : Yolo
    {
        public abstract List<InstanceSegmentationResult> EvaluateFrame(VideoFrame vf);

        public YoloInstanceSegmentation(string modelAssetFile, uint inputWidth, uint inputHeight, uint inputDepth,
                                        string[] inputs, string[] outputs, Dataset dataset, float confidence)
                                        : base(modelAssetFile, inputWidth, inputHeight, inputDepth, inputs, outputs,
                                          dataset, confidence)
        {

        }

    }
}
