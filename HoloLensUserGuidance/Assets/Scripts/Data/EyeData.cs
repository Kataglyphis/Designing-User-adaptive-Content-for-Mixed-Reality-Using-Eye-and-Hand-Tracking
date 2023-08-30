// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using Assets.Scripts.Data.Samples;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;

namespace Assets.Scripts.Data
{
    public class EyeData : UserDataCluster
    {

        private IMixedRealityEyeGazeProvider EyeTrackingProvider =>
                            eyeTrackingProvider ?? (eyeTrackingProvider = CoreServices.InputSystem?.EyeGazeProvider);
        private IMixedRealityEyeGazeProvider eyeTrackingProvider = null;

        public override List<UserTrackingSamplePoint> UserTrackingSamplePoints
        {
            get
            {
                return new List<UserTrackingSamplePoint>() {

                                        new UserTrackingSamplePoint3D(  "EyeOrigin",
                                                                        "world space position of the eye"),
                                        new UserTrackingSamplePoint3D(  "EyeDir",
                                                                        "world space dir of the eye"),
                                        new UserTrackingSamplePoint1D(  "Distance to target",
                                                                        "Distance to closest hit"),
                                        new UserTrackingSamplePoint3D(  "EyeHitPos",
                                                                        "world space dir of the eye"),
                                        new UserTrackingSamplePoint3D(  "EyeHitNormal",
                                                                        "world space dir of the eye")};
            }
        }

        protected override List<object> RawData
        {
            get
            {
                var eyeGazeProvider = CoreServices.InputSystem?.EyeGazeProvider;
                // Eye gaze hit position
                Vector3? eyeHitPos = null;
                Vector3? eyeHitNormal = null;
                MixedRealityRaycastHit? hitInfo = null;
                if (eyeGazeProvider != null)
                {
                    eyeHitPos = eyeGazeProvider.HitPosition;
                    eyeHitNormal = eyeGazeProvider.HitNormal;
                    hitInfo = eyeGazeProvider.HitInfo;
                }
                //    var aux = EyeTrackingProvider.IsEyeGazeValid;
                //if (EyeTrackingProvider?.GazeTarget != null && EyeTrackingProvider.IsEyeTrackingEnabledAndValid)
                //{
                //    eyeHitPos = EyeTrackingProvider.HitPosition;
                //    eyeHitNormal = EyeTrackingProvider.HitNormal;
                //    hitInfo = EyeTrackingProvider.HitInfo;
                //}

                return new List<object>() {

                    // Smoothed eye gaze signal 
                    eyeGazeProvider.IsEyeTrackingEnabledAndValid ? EyeTrackingProvider.GazeOrigin.x : float.NaN,
                    eyeGazeProvider.IsEyeTrackingEnabledAndValid ? EyeTrackingProvider.GazeOrigin.y : float.NaN,
                    eyeGazeProvider.IsEyeTrackingEnabledAndValid ? EyeTrackingProvider.GazeOrigin.z : float.NaN,

                    eyeGazeProvider.IsEyeTrackingEnabledAndValid ? EyeTrackingProvider.GazeDirection.x : float.NaN,
                    eyeGazeProvider.IsEyeTrackingEnabledAndValid ? EyeTrackingProvider.GazeDirection.y : float.NaN,
                    eyeGazeProvider.IsEyeTrackingEnabledAndValid ? EyeTrackingProvider.GazeDirection.z : float.NaN,

                    //EyeTrackingProvider.IsEyeTrackingEnabledAndValid? EyeTrackingProvider.GazeOrigin.x : float.NaN,
                    //EyeTrackingProvider.IsEyeTrackingEnabledAndValid? EyeTrackingProvider.GazeOrigin.y : float.NaN,
                    //EyeTrackingProvider.IsEyeTrackingEnabledAndValid? EyeTrackingProvider.GazeOrigin.z : float.NaN,

                    //EyeTrackingProvider.IsEyeTrackingEnabledAndValid? EyeTrackingProvider.GazeDirection.x : float.NaN,
                    //EyeTrackingProvider.IsEyeTrackingEnabledAndValid? EyeTrackingProvider.GazeDirection.y : float.NaN,
                    //EyeTrackingProvider.IsEyeTrackingEnabledAndValid? EyeTrackingProvider.GazeDirection.z : float.NaN,

                    (hitInfo != null) ? hitInfo.Value.distance : float.NaN,

                    (eyeHitPos != null) ? eyeHitPos.Value.x : float.NaN,
                    (eyeHitPos != null) ? eyeHitPos.Value.y : float.NaN,
                    (eyeHitPos != null) ? eyeHitPos.Value.z : float.NaN,

                    (eyeHitNormal != null) ? eyeHitNormal.Value.x : float.NaN,
                    (eyeHitNormal != null) ? eyeHitNormal.Value.y : float.NaN,
                    (eyeHitNormal != null) ? eyeHitNormal.Value.z : float.NaN,

                };
            }
        }

    }

}
