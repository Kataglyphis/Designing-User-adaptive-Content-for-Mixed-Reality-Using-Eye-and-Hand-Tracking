// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;

namespace Assets.Scripts.Data.Samples
{
    public class UserTrackingSamplePoint1D : UserTrackingSamplePoint
    {

        public UserTrackingSamplePoint1D(string name, string description) : base(name, description)
        {

        }

        public override List<string> Labels
        {
            get { return new List<string> { name }; }
        }

        public override List<string> VectorizedDescription
        {
            get { return new List<string> { $"1 dimensional value {name}" + " " + description }; }
        }
    }
}
