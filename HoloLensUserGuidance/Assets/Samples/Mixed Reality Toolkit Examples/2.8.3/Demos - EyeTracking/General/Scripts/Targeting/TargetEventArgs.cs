// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.MixedReality.Toolkit.Input;

namespace Microsoft.MixedReality.Toolkit.Examples.Demos.EyeTracking
{
    /// <summary>
    /// Class specifying targeting event arguments.
    /// </summary>
    public class TargetEventArgs : System.EventArgs
    {
        public EyeTrackingTarget HitTarget { get; private set; }

        public TargetEventArgs(EyeTrackingTarget hitTarget)
        {
            HitTarget = hitTarget;
        }
    }
}
