// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace SharedResultsBetweenServerAndHoloLens
{
    /**
    <summary>
    This file also resides on HoloLens side; therefore, it is important to keep it in sync.!!!!
    It is a bit dirty but necessary for everyone who will work on this to keep this constraints in mind
    </summary> 
    */
    public static class CSVFileHelper
    {
        public const int MaxResults = 4;
        public const char CsvSeparator = ';';
        public const int MaxCharacterPerCSVCell = 32000;

        private static byte[] CombineBase64(byte[] first, byte[] second)
        {
            return first.Concat(second).ToArray();
        }

        public static string ConvertDetectionResultsToString(List<InstanceSegmentationResult> detectionResults)
        {
            //string result = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(string.Empty));

            //for (int i = 0; i < MaxResults; i++)
            //{
            //    if (i < detectionResults.Count)
            //    {
            //        bool isLastEntry = i == MaxResults - 1;
            //        result = System.Convert.ToBase64String(CombineBase64(System.Convert.FromBase64String(result), System.Convert.FromBase64String(detectionResults[i].ConvertToCSVFilePresentation(isLastEntry))));
            //    }
            //    else
            //    {
            //        bool isLastEntry = (i == MaxResults - 1);
            //        int numOfSeparatorsToAdd = (isLastEntry) ? 5 - 1 : 5;
            //        var aux = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(new string(CsvSeparator, numOfSeparatorsToAdd)));
            //        result = System.Convert.ToBase64String(CombineBase64(System.Convert.FromBase64String(result), System.Convert.FromBase64String(aux)));
            //    }
            //}

            //return result;

            string result = string.Empty;

            if (detectionResults.Count > 0)
            {
                result = detectionResults[0].ConvertToCSVFilePresentation(false);
            }

            //for (int i = 0; i < MaxResults; i++)
            //{
            //    if (i < detectionResults.Count)
            //    {
            //        bool isLastEntry = i == MaxResults - 1;
            //        result += detectionResults[i].ConvertToCSVFilePresentation(isLastEntry);
            //    }
            //    else
            //    {
            //        bool isLastEntry = (i == MaxResults - 1);
            //        int numOfSeparatorsToAdd = (isLastEntry) ? 5 - 1 : 5;
            //        var aux = new string(CsvSeparator, numOfSeparatorsToAdd);
            //        result += aux;
            //    }
            //}

            return result;
        }

        public static string ConvertDetectionResultsToString(List<DetectionResult> detectionResults)                                                   
        {
            string result = string.Empty;

            for (int i = 0; i < MaxResults; i++)
            {
                if (i < detectionResults.Count)
                {
                    bool isLastEntry = i == MaxResults - 1;
                    result += detectionResults[i].ConvertToCSVFilePresentation(isLastEntry);
                }
                else
                {
                    bool isLastEntry = (i == MaxResults - 1);
                    int numOfSeparatorsToAdd = (isLastEntry) ? 3 - 1 :
                                                               3;
                    result += new string(CsvSeparator, numOfSeparatorsToAdd);
                }
            }

            return result;
        }
    }

}
