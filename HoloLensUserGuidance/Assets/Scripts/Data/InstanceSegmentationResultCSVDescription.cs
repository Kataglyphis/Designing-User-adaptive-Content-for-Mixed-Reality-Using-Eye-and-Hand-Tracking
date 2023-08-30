// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Assets.Scripts.Data
{
    public class InstanceSegmentationResultCSVDescription : NNDataCluster
    {

        protected override string[] EntryLabels
        {
            get
            {

                return new string[] {   "segmentation class label",
                                        "segmentation bbox ccords",
                                        "segmentation class prob",
                                        "segmentation result image mask part 1",
                                        "segmentation result image mask part 2",};
            }
        }

        protected override string[] EntryDescriptions
        {
            get
            {
                return new string[] {
                    "one class label from the corresponding data set",
                    "each bbox ccord is in range [0,640] layout of string is as follows: top, left, bottom, right",
                    "class prob is between 0 and 1",
                    "content is the byte[] image mask size 160x160 in base64 code only first part",
                    "2nd part of the image mask",};
            }
        }

        public InstanceSegmentationResultCSVDescription(int maxValues) : base(maxValues) { }

    }
}
