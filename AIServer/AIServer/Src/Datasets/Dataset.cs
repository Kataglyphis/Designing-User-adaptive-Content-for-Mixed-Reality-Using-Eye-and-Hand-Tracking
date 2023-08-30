// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.UI;

namespace AIServer.Datasets
{
    public abstract class Dataset
    {

        protected abstract Color[] Colors { get; }
        public abstract string[] Labels { get; }

        public Dictionary<string, Color> Class_colors
        {
            get
            {
                Debug.Assert(Colors.Length == Labels.Length);
                Dictionary<string, Color> result =
                    Labels.Zip(Colors, (key, value) => new { key, value }).ToDictionary(x => x.key, x => x.value);
                return result;
            }
        }

    }
}
