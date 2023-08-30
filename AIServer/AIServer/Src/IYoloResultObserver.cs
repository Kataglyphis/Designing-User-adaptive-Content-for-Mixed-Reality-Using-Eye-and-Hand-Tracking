// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using SharedResultsBetweenServerAndHoloLens;
using Windows.Media.Core;
using Windows.UI;

namespace AIServer
{

    public struct YoloResults
    {
        public List<DetectionResult> detectionResults;
        public List<Color> detectionResultColors;
        public List<InstanceSegmentationResult> detectionResultsInstanceSegmentation;
        public List<Color> detectionResultsInstanceSegmentationColors;
        public uint instanceSegmentationInputWidth;
        public uint instanceSegmentationInputHeight;
        public uint objectDetectionInputWidth;
        public uint objectDetectionInputHeight;
        public uint maskOutputSizeX;
        public uint maskOutputSizeY;

        public YoloResults(List<DetectionResult> detectionResults,
                            List<Color> detectionResultColors,
                            List<InstanceSegmentationResult> detectionResultsInstanceSegmentation,
                            List<Color> detectionResultsInstanceSegmentationColors,
                            uint instanceSegmentationInputWidth, uint instanceSegmentationInputHeight,
                            uint objectDetectionInputWidth, uint objectDetectionInputHeight,
                            uint maskOutputSizeX, uint maskOutputSizeY) : this()
        {
            this.detectionResults = detectionResults;
            this.detectionResultColors = detectionResultColors;
            this.detectionResultsInstanceSegmentation = detectionResultsInstanceSegmentation;
            this.detectionResultsInstanceSegmentationColors = detectionResultsInstanceSegmentationColors;
            this.instanceSegmentationInputWidth = instanceSegmentationInputWidth;
            this.instanceSegmentationInputHeight = instanceSegmentationInputHeight;
            this.objectDetectionInputWidth = objectDetectionInputWidth;
            this.objectDetectionInputHeight = objectDetectionInputHeight;
            this.maskOutputSizeX = maskOutputSizeX;
            this.maskOutputSizeY = maskOutputSizeY;
        }
    }

    public interface IYoloResultObserver
    {
        void RenderYoloResults(YoloResults results);

        void UpdateRemoteVideoSource(MediaStreamSource remoteVideoSource);

    }
}
