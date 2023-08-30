// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Globalization;

namespace SharedResultsBetweenServerAndHoloLens
{

    [Serializable]
    public class DetectionResult : IToCSVEntryConvertable
    {

        public virtual uint NumEntries
        {
            get
            {
                return 3;
            }
        }

        public string label = "dummy";

        /** <value>
        detection results bbox layout here is:
        bbox[0] : top
        bbox[1] : left
        bbox[2] : bottom 
        bbox[3] : right
        </value>
        */
        public List<float> bbox = new List<float>();
        public double prob = -1;

        public virtual string ConvertToCSVFilePresentation(bool isLastEntry)
        {
            string detectionResults = "";
            detectionResults += label + CSVFileHelper.CsvSeparator;
            for (int i = 0; i < bbox.Count; i++)
            {
                detectionResults += bbox[i].ToString() + " ";
            }
            detectionResults += CSVFileHelper.CsvSeparator;
            detectionResults += prob;
            //if (!isLastEntry) detectionResults += CSVFileHelper.CsvSeparator;

            return detectionResults;
        }
    }
}
