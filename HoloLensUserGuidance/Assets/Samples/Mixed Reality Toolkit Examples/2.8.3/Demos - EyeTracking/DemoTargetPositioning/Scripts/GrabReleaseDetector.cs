// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;
using UnityEngine.Events;

namespace Microsoft.MixedReality.Toolkit.Examples.Demos.EyeTracking
{
    [System.Obsolete("This component is no longer supported", true)]
    [AddComponentMenu("Scripts/MRTK/Obsolete/GrabReleaseDetector")]
    public class GrabReleaseDetector : MonoBehaviour, IMixedRealityPointerHandler
    {
        [SerializeField]
        private UnityEvent OnGrab = null;

        [SerializeField]
        private UnityEvent OnRelease = null;

        private void Awake()
        {
            Debug.LogError(this.GetType().Name + " is deprecated");
        }

        void IMixedRealityPointerHandler.OnPointerClicked(MixedRealityPointerEventData eventData) { }

        void IMixedRealityPointerHandler.OnPointerDown(MixedRealityPointerEventData eventData)
        {
            Debug.Log("OnGrab");
            OnGrab.Invoke();
        }

        void IMixedRealityPointerHandler.OnPointerUp(MixedRealityPointerEventData eventData)
        {
            Debug.Log("OnRelease");
            OnRelease.Invoke();
        }

        void IMixedRealityPointerHandler.OnPointerDragged(MixedRealityPointerEventData eventData) { }
    }
}