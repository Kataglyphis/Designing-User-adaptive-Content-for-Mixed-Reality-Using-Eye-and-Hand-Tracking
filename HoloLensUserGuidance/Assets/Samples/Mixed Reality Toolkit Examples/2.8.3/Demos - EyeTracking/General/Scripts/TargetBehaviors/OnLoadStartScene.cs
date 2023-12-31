﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using UnityEngine;
using UnityEngine.SceneManagement;

namespace Microsoft.MixedReality.Toolkit.Examples.Demos.EyeTracking
{

    /// <summary>
    /// When the button is selected, it triggers starting the specified scene.
    /// </summary>
    [AddComponentMenu("Scripts/MRTK/Examples/OnLoadStartScene")]
    public class OnLoadStartScene : MonoBehaviour
    {
        private enum LoadOptions
        {
            LoadOnDeviceAndInEditor,
            LoadOnlyOnDevice,
        }
        [SerializeField]
        [Tooltip("Name of the scene to be loaded when the button is selected.")]
        private string SceneToBeLoaded = "";

        [SerializeField]
        [Tooltip("Option to only load the scene if running on the HoloLens device.")]
        private LoadOptions LoadOption = LoadOptions.LoadOnlyOnDevice;

        public void Start()
        {
            if (LoadOption == LoadOptions.LoadOnlyOnDevice)
            {
                LoadOnDevice();
            }
            else
            {
                LoadNewScene();
            }
        }

        private void LoadOnDevice()
        {
            if (!Application.isEditor)
            {
                LoadNewScene();
            }
        }

        private void LoadNewScene()
        {
            if (SceneToBeLoaded != "")
            {
                SceneManager.LoadSceneAsync(SceneToBeLoaded, LoadSceneMode.Additive);
            }
        }
    }
}
