// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using Assets.Scripts.Data.Samples;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;

namespace Assets.Scripts.Data
{
    public class HeadData : UserDataCluster
    {

        private IMixedRealityEyeGazeProvider EyeTrackingProvider =>
                            eyeTrackingProvider ?? (eyeTrackingProvider = CoreServices.InputSystem?.EyeGazeProvider);
        private IMixedRealityEyeGazeProvider eyeTrackingProvider = null;

        private IMixedRealityHandJointService handJointService =
                                            CoreServices.GetInputSystemDataProvider<IMixedRealityHandJointService>();

        public override List<UserTrackingSamplePoint> UserTrackingSamplePoints
        {
            get
            {
                return new List<UserTrackingSamplePoint>() {

                                        new UserTrackingSamplePoint3D(  "HeadOrigin",
                                                                        "world space position of the head"),
                                        new UserTrackingSamplePoint3D(  "HeadDirForward",
                                                                        "world space dir of the head"),
                                        new UserTrackingSamplePoint3D(  "HeadDirRight",
                                                                        "world space right vector of the head"),
                                        new UserTrackingSamplePoint3D(  "HeadMovementDirection",
                                                                        "world space movement dir of the head"),
                                        new UserTrackingSamplePoint3D(  "HeadVelocity",
                                                                        "head velocity in each dir"),};
            }
        }

        protected override List<object> RawData
        {
            get
            {
                return new List<object>() { 
                // Cam / Head tracking
                // https://docs.unity3d.com/ScriptReference/Transform.html
                CameraCache.Main.transform.position.x,
                CameraCache.Main.transform.position.y,
                CameraCache.Main.transform.position.z,
                CameraCache.Main.transform.forward.x,
                CameraCache.Main.transform.forward.y,
                CameraCache.Main.transform.forward.z,
                CameraCache.Main.transform.right.x,
                CameraCache.Main.transform.right.y,
                CameraCache.Main.transform.right.z,

                EyeTrackingProvider.IsEyeTrackingEnabledAndValid ?
                                            EyeTrackingProvider.HeadMovementDirection.x : float.NaN,
                EyeTrackingProvider.IsEyeTrackingEnabledAndValid ?
                                            EyeTrackingProvider.HeadMovementDirection.y : float.NaN,
                EyeTrackingProvider.IsEyeTrackingEnabledAndValid ?
                                            EyeTrackingProvider.HeadMovementDirection.z : float.NaN,

                EyeTrackingProvider.IsEyeTrackingEnabledAndValid ? EyeTrackingProvider.HeadVelocity.x : float.NaN,
                EyeTrackingProvider.IsEyeTrackingEnabledAndValid ? EyeTrackingProvider.HeadVelocity.y : float.NaN,
                EyeTrackingProvider.IsEyeTrackingEnabledAndValid ? EyeTrackingProvider.HeadVelocity.z : float.NaN,};
            }
        }
    }
}
