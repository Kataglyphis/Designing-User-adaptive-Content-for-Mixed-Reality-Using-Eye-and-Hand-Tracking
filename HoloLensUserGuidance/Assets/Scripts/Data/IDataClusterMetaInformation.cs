// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;

namespace Assets.Scripts.Data
{
    public interface IDataClusterMetaInformation
    {
        /**
        <value>
            Holds a description for each value in the cluster.
            For example data range, data space, etc.
        </value>
        */
        public List<string> Description { get; }

        /**
        <value>
            Holding information for the csv header row
        </value>
        */
        public List<string> Labels { get; }

    }
}
