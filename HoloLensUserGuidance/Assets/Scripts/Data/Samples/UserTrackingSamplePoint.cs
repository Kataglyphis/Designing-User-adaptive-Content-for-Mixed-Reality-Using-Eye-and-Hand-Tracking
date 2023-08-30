// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Data.Samples
{
    public abstract class UserTrackingSamplePoint
    {

        protected string name;
        protected string description;

        public UserTrackingSamplePoint(string name, string description)
        {
            this.name = name;
            this.description = description;
        }

        /**
        <value>
            Get the corresponding labels for one sample point
        </value>
        */
        public abstract List<string> Labels { get; }

        /**
        <value>
            Get the corresponding labels for one sample point
        </value>
        */
        public abstract List<string> VectorizedDescription { get; }

        /**
        <value>
            Get the corresponding labels for one sample point
        </value>
        */
        public List<string> Descriptions
        {
            get
            {
                List<string> result = VectorizedDescription;
                result.ForEach(i => i.Concat("; ").Concat(description));
                return result;
            }

        }
    }
}
