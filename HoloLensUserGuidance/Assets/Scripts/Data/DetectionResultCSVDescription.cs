// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Assets.Scripts.Data
{
    public class DetectionResultCSVDescription : NNDataCluster
    {

        protected override string[] EntryLabels
        {
            get
            {
                return new string[] {   "object detection class label",
                                        "object detection bbox ccords",
                                        "object detection class prob",};
            }

        }

        protected override string[] EntryDescriptions
        {
            get
            {
                return new string[] {
                        "one class label from the corresponding data set",
                        "each bbox ccord is in range [0,640] layout of string is as follows: top, left, bottom, right",
                        "class prob is between 0 and 1",};
            }
        }


        public DetectionResultCSVDescription(int maxValues) : base(maxValues) { }

    }
}
