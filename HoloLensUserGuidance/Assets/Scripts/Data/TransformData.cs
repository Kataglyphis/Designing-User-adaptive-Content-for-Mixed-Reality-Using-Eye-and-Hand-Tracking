// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Utilities;

namespace Assets.Scripts.Data
{
    public class TransformData : ReadOnlyDataCluster
    {
        protected override List<string> RawDescription
        {
            get
            {
                List<string> result = new List<string>() { // VP matrices
                                            "The world to camera matrix camera here is the hololens on your head",
                                            "The corresponding projection matrix of HoloLens2", };
                return result;
            }
        }

        protected override List<string> RawLabels
        {
            get
            {
                return new List<string>() { // VP matrices
                                            "worldToCameraMatrix",
                                            "projectionMatrix",};
            }
        }

        protected override List<object> RawData
        {
            get
            {
                List<object> result = new List<object>() {
                                CameraCache.Main.worldToCameraMatrix.ToString().Replace('\n', ' '),
                                CameraCache.Main.projectionMatrix.ToString().Replace('\n', ' '),};

                return result;

            }
        }
    }
}
