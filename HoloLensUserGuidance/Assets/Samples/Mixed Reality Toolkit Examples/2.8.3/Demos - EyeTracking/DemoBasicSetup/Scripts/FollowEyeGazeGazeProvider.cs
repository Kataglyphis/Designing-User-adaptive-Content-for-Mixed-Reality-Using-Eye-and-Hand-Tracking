// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Examples.Demos.EyeTracking
{
    /// <summary>
    /// Sample for allowing the game object that this script is attached to follow the user's eye gaze
    /// at a given distance of "DefaultDistanceInMeters". 
    /// </summary>
    [AddComponentMenu("Scripts/MRTK/Examples/FollowEyeGazeGazeProvider")]
    public class FollowEyeGazeGazeProvider : MonoBehaviour
    {
        [Tooltip("Display the game object along the eye gaze ray at a default distance (in meters).")]
        [SerializeField]
        private float defaultDistanceInMeters = 2f;

        private void Update()
        {
            var gazeProvider = CoreServices.InputSystem?.GazeProvider;
            if (gazeProvider != null)
            {
                gameObject.transform.position = gazeProvider.GazeOrigin + gazeProvider.GazeDirection.normalized * defaultDistanceInMeters;
            }
        }
    }
}