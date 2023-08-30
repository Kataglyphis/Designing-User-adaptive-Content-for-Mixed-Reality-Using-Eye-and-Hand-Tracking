// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;

namespace HoloLensUserGuidance.EyeTracking.Logging
{
    [AddComponentMenu("Scripts/HoloLensUserGuidance/LogStructure")]
    public class LogStructure : MonoBehaviour
    {

        ///// <summary>
        ///// The <see cref="PeerConnection"/> this signaler needs to work for.
        ///// </summary>
        //public PeerConnection PeerConnection = GameObject.Find("PeerConnection").;

        public virtual string[] GetHeaderColumns()
        {
            return System.Array.Empty<string>();
        }

        public virtual string[] GetDescriptionColumns()
        {
            return System.Array.Empty<string>();
        }

        public virtual object[] GetData(string inputType, string inputStatus, EyeTrackingTarget intTarget)
        {
            return System.Array.Empty<object>();
        }
    }
}
