// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;

namespace SharedResultsBetweenServerAndHoloLens
{
    //  This class will be used to draw bounding boxes around the detected objects.
    public class DetectionComparer : IComparer<DetectionResult>
    {
        public int Compare(DetectionResult x, DetectionResult y)
        {
            return y.prob.CompareTo(x.prob);
        }
    }

    //  This class will be used to draw bounding boxes around the detected objects.
    public class InstanceSegmentationComparer : IComparer<InstanceSegmentationResult>
    {
        public int Compare(InstanceSegmentationResult x, InstanceSegmentationResult y)
        {
            return y.prob.CompareTo(x.prob);
        }
    }
}
