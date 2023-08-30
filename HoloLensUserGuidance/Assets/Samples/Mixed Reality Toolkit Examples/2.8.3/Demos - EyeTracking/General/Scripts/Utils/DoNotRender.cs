// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Examples.Demos.EyeTracking
{
    /// <summary>
    /// Simple class that automatically hides a target on startup. This is, for example, useful for trigger zones and visual guides that are useful 
    /// to show in the Editor, but not in the final application.
    /// </summary>
    [AddComponentMenu("Scripts/MRTK/Examples/DoNotRender")]
    public class DoNotRender : MonoBehaviour
    {
        private void Start()
        {
            GetComponent<Renderer>().enabled = false;
        }
    }
}
