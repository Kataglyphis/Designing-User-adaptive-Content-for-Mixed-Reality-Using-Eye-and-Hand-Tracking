﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Examples.Demos.EyeTracking
{
    /// <summary>
    /// This script continuously updates the orientation of the associated game object to keep facing the camera/user.
    /// </summary>
    [System.Obsolete("This component is no longer supported", true)]
    [AddComponentMenu("Scripts/MRTK/Obsolete/KeepFacingCamera")]
    public class KeepFacingCamera : MonoBehaviour
    {
        private Vector3 origForwardVector;

        private void Awake()
        {
            Debug.LogError(this.GetType().Name + " is deprecated");
        }

        private void Start()
        {
            // Let's figure out the original orientation of the target to keep it in the same orientation with respect to the camera when moving
            origForwardVector = Quaternion.FromToRotation(Vector3.forward, transform.rotation.eulerAngles).eulerAngles.normalized;
        }

        private void Update()
        {
            Vector3 target2CamDir = transform.position - CameraCache.Main.transform.position;
            transform.rotation = Quaternion.FromToRotation(origForwardVector, target2CamDir);
        }
    }
}
