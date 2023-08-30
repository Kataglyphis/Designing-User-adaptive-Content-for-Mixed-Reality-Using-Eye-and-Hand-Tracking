// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;

namespace Assets.Scripts.Data.Samples
{
    public class UserTrackingSamplePoint4D : UserTrackingSamplePoint
    {
        public UserTrackingSamplePoint4D(string name, string descriptions) : base(name, descriptions)
        {

        }

        public override List<string> Labels
        {
            get
            {
                return new List<string>() { $"{name}.x",
                                            $"{name}.y",
                                            $"{name}.z",
                                            $"{name}.w"};
            }
        }

        public override List<string> VectorizedDescription
        {
            get
            {
                return new List<string>() { $"1st dimension value of 4 dimensional value named {name}",
                                            $"2nd dimension value of 4 dimensional value named {name}",
                                            $"3th dimension value of 4 dimensional value named {name}",
                                            $"4th dimension value of 4 dimensional value named {name}"};
            }
        }
    }
}
