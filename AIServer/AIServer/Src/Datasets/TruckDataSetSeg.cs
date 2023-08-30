// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Windows.UI;

namespace AIServer.Datasets
{
    public class TruckDataSetSeg : Yolov5Dataset
    {
        public TruckDataSetSeg(string YamlFileName) : base(YamlFileName)
        {

        }

        protected override Color[] Colors
        {
            get
            {
                return new[] { Windows.UI.Colors.OrangeRed, Windows.UI.Colors.Olive, Windows.UI.Colors.AliceBlue };
            }
        }

    }
}
