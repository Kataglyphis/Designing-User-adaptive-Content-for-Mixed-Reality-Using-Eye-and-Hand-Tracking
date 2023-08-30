// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace SharedResultsBetweenServerAndHoloLens
{

    public class InstanceSegmentationResult : DetectionResultWithUniqueNumEntries
    {

        public InstanceSegmentationResult(string label, double prob, List<float> bbox, List<float> masks, byte[] mask_img)
        {
            this.label = label;
            this.prob = prob;
            this.bbox = bbox;
            this.masks = masks;
            this.mask_img = mask_img;
        }

        public InstanceSegmentationResult(string label, double prob, List<float> bbox, List<float> masks)
        {
            this.label = label;
            this.prob = prob;
            this.bbox = bbox;
            this.masks = masks;
        }

        public InstanceSegmentationResult()
        {
            this.label = "dummy";
            this.prob = -1;
            this.bbox = new List<float>() { -1, -1, -1, -1 };
        }

        public override uint NumEntries
        {
            get
            {
                return 5;
            }
        }

        public List<float> masks;
        public byte[] mask_img;

        private byte[] CombineBase64(byte[] first, byte[] second)
        {
            return first.Concat(second).ToArray();
        }

        public override string ConvertToCSVFilePresentation(bool isLastEntry)
        {
            // mask_img size = 160*160 > maxCharacters in on csv file therefore split it up into two
            //string mask_img_str = Convert.ToBase64String(mask_img);
            //byte[] result = Encoding.UTF8.GetBytes(base.ConvertToCSVFilePresentation(false));
            //result = CombineBase64(result, Encoding.UTF8.GetBytes(mask_img_str.Substring(0, CSVFileHelper.MaxCharacterPerCSVCell)));
            //result = CombineBase64(result, Encoding.UTF8.GetBytes(Char.ToString(CSVFileHelper.CsvSeparator)));
            //result = CombineBase64(result, Encoding.UTF8.GetBytes(mask_img_str.Substring(CSVFileHelper.MaxCharacterPerCSVCell)));
            //if(!isLastEntry) result = CombineBase64(result, Encoding.UTF8.GetBytes(Char.ToString(CSVFileHelper.CsvSeparator)));

            //return System.Convert.ToBase64String(result);

            //string mask_img_str = Convert.ToBase64String(mask_img); // Encoding.UTF8.GetString(mask_img);//
            string result = base.ConvertToCSVFilePresentation(false);
            //result += mask_img_str;
            //result += mask_img_str.Substring(0, CSVFileHelper.MaxCharacterPerCSVCell);
            //result += CSVFileHelper.CsvSeparator;
            //result += mask_img_str.Substring(CSVFileHelper.MaxCharacterPerCSVCell);
            //if (!isLastEntry) result += CSVFileHelper.CsvSeparator;

            return result;
        }

    }
}
