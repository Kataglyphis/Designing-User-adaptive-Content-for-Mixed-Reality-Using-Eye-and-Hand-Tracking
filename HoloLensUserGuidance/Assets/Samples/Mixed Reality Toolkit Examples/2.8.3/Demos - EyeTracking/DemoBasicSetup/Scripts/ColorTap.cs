﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Examples.Demos.EyeTracking
{
    [AddComponentMenu("Scripts/MRTK/Examples/ColorTap")]
    public class ColorTap : MonoBehaviour, IMixedRealityFocusHandler, IMixedRealityPointerHandler
    {
        [SerializeField]
        private MixedRealityInputAction tapAction = MixedRealityInputAction.None;

        [SerializeField]
        private Color color_IdleState = Color.cyan;

        [SerializeField]
        private Color color_OnHover = Color.white;

        [SerializeField]
        private Color color_OnSelect = Color.blue;

        private Material material;

        private void Awake()
        {
            material = GetComponent<Renderer>().material;
        }

        void IMixedRealityFocusHandler.OnFocusEnter(FocusEventData eventData)
        {
            material.color = color_OnHover;
        }

        void IMixedRealityFocusHandler.OnFocusExit(FocusEventData eventData)
        {
            material.color = color_IdleState;
        }

        void IMixedRealityPointerHandler.OnPointerUp(MixedRealityPointerEventData eventData)
        {
            if (eventData.MixedRealityInputAction == tapAction)
            {
                material.color = color_OnHover;
            }
        }

        void IMixedRealityPointerHandler.OnPointerDown(MixedRealityPointerEventData eventData)
        {
            if (eventData.MixedRealityInputAction == tapAction)
            {
                material.color = color_OnSelect;
            }
        }

        void IMixedRealityPointerHandler.OnPointerDragged(MixedRealityPointerEventData eventData) { }

        void IMixedRealityPointerHandler.OnPointerClicked(MixedRealityPointerEventData eventData) { }
    }
}