// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using Windows.Media.Core;

namespace AIServer.Src
{
    public abstract class YoloResultsCreator
    {
        private List<IYoloResultObserver> _observers;

        public YoloResultsCreator()
        {
            _observers = new List<IYoloResultObserver>();
        }

        public void Subscribe(IYoloResultObserver b)
        {
            if (!_observers.Contains(b))
                _observers.Add(b);
        }

        public void Unsubscribe(IYoloResultObserver b)
        {
            if (_observers.Contains(b))
                _observers.Remove(b);
        }

        public void Notify(YoloResults yoloResults)
        {
            foreach (var b in _observers)
                b.RenderYoloResults(yoloResults);
        }

        public void Notify(MediaStreamSource remoteVideoSource)
        {
            foreach (var b in _observers)
                b.UpdateRemoteVideoSource(remoteVideoSource);
        }

    }
}
