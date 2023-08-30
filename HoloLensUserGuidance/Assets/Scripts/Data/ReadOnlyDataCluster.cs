// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;

namespace Assets.Scripts.Data
{
    public abstract class ReadOnlyDataCluster : IDataClusterMetaInformation
    {

        private void verifyDataLayoutCorrectness()
        {
            //Debug.Assert(Data.Count == Description.Count);
            //Debug.Assert(Description.Count == Labels.Count);
        }

        /**
        <value>
            User data can only be returned and not set from outside
        </value>
        */
        protected abstract List<object> RawData { get; }
        public List<object> Data
        {
            get
            {
                verifyDataLayoutCorrectness();
                return RawData;
            }
        }
        protected abstract List<string> RawDescription { get; }
        public List<string> Description
        {
            get
            {
                verifyDataLayoutCorrectness();
                return RawDescription;
            }
        }
        protected abstract List<string> RawLabels { get; }
        public List<string> Labels
        {
            get
            {
                verifyDataLayoutCorrectness();
                return RawLabels;
            }
        }
    }
}
