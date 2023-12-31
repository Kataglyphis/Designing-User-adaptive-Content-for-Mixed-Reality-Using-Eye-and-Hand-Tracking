﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;
using UnityEngine.Events;

namespace Microsoft.MixedReality.Toolkit.Examples.Demos.EyeTracking
{
    /// <summary>
    /// When the button is selected, it triggers starting the specified scene.
    /// </summary>
    [RequireComponent(typeof(EyeTrackingTarget))]
    [System.Obsolete("This component is no longer supported", true)]
    [AddComponentMenu("Scripts/MRTK/Obsolete/OnSelectVisualizerInputController")]
    public class OnSelectVisualizerInputController : BaseEyeFocusHandler, IMixedRealityPointerHandler
    {
        [SerializeField]
        public UnityEvent EventToTrigger;

        private void Awake()
        {
            Debug.LogError(this.GetType().Name + " is deprecated");
        }

        private void OnTargetSelected()
        {
            Debug.LogFormat(">> [{0}] Selected! -- {1} -- {2}", name, ToString(), name);
        }

        void IMixedRealityPointerHandler.OnPointerClicked(MixedRealityPointerEventData eventData)
        {
            if (HasFocus)
            {
                OnTargetSelected();
            }
        }

        void IMixedRealityPointerHandler.OnPointerDown(MixedRealityPointerEventData eventData) { }

        void IMixedRealityPointerHandler.OnPointerUp(MixedRealityPointerEventData eventData) { }

        void IMixedRealityPointerHandler.OnPointerDragged(MixedRealityPointerEventData eventData) { }
    }
}
