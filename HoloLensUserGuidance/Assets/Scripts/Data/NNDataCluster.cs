// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Assets.Scripts.Data
{
    public abstract class NNDataCluster : IDataClusterMetaInformation
    {

        protected int _maxValues;

        protected abstract string[] EntryLabels { get; }

        protected abstract string[] EntryDescriptions { get; }

        private void VerifyDataLayoutCorrectness()
        {
            Debug.Assert(EntryDescriptions.Length == EntryLabels.Length);
        }

        public NNDataCluster(int maxValues)
        {
            this._maxValues = maxValues;
            // we only store one associated string with correspondig csv-separators
            _data = new List<object>(1);
            _data.Add(string.Empty);
        }


        private List<object> _data;
        /**
        <value>
            User data can only be returned and not set from outside
        </value>
        */
        public List<object> Data
        {
            get
            {
                return _data;
            }
            set
            {
                // for now i except one string for the cluster
                //Debug.Assert(value.Capacity == 1);
                this._data = value;
            }
        }

        public List<string> Description
        {
            get
            {
                VerifyDataLayoutCorrectness();
                return (from number in Enumerable.Range(0, _maxValues)
                        select EntryDescriptions)
                        .SelectMany(item => item)
                        .ToList();
            }
        }

        public List<string> Labels
        {
            get
            {
                VerifyDataLayoutCorrectness();
                return (from number in Enumerable.Range(0, _maxValues)
                        select EntryLabels.Select(entry => $"{number + 1}. " + entry))
                        .SelectMany(item => item)
                        .ToList();
            }
        }
    }
}
