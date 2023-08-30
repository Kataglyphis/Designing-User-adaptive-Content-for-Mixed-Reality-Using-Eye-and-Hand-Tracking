
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Assets.Scripts.Data.Samples;
using SharedResultsBetweenServerAndHoloLens;

namespace Assets.Scripts.Data
{
    public abstract class UserDataCluster : ReadOnlyDataCluster
    {
        public abstract List<UserTrackingSamplePoint> UserTrackingSamplePoints { get; }

        protected override List<string> RawLabels
        {
            get
            {
                List<string> labels = new List<string>();
                foreach (UserTrackingSamplePoint samplePoint in UserTrackingSamplePoints)
                {
                    labels.AddRange(samplePoint.Labels);
                }

                labels.ForEach(label => Debug.Assert(!label.Contains(CSVFileHelper.CsvSeparator)));

                return labels;
            }
        }

        protected override List<string> RawDescription
        {
            get
            {
                List<string> descriptions = new List<string>();
                foreach (UserTrackingSamplePoint samplePoint in UserTrackingSamplePoints)
                {
                    descriptions.AddRange(samplePoint.Descriptions);
                }

                descriptions.ForEach(descriptions =>
                                    Debug.Assert(!descriptions.Contains(CSVFileHelper.CsvSeparator)));

                return descriptions;
            }
        }
    }
}
