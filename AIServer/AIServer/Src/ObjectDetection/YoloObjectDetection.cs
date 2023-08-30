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
        Our object detection interface
    </summary>
    <remarks>
        
    </remarks>
    */
    public abstract class YoloObjectDetection : Yolo
    {

        public abstract List<DetectionResult> EvaluateFrame(VideoFrame vf);

        protected YoloObjectDetection(string modelAssetFile, uint inputWidth, uint inputHeight, uint inputDepth,
                                      string[] inputs, string[] outputs, Dataset dataset, float confidence)
                                        : base(modelAssetFile, inputWidth, inputHeight, inputDepth,
                                                inputs, outputs, dataset, confidence)
        {

        }

    }

}
