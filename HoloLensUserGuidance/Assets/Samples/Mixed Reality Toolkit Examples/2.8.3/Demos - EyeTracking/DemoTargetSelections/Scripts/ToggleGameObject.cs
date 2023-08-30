// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Examples.Demos.EyeTracking
{
    [AddComponentMenu("Scripts/MRTK/Examples/ToggleGameObject")]
    public class ToggleGameObject : MonoBehaviour
    {
        [SerializeField]
        private GameObject objToShowHide = null;

        public void ShowIt()
        {
            ShowIt(true);
        }

        public void HideIt()
        {
            ShowIt(false);
        }

        private void ShowIt(bool showIt)
        {
            if (objToShowHide != null)
            {
                objToShowHide.SetActive(showIt);
            }
        }
    }
}
