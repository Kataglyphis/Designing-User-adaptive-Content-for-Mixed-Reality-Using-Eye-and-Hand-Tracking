// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace SharedResultsBetweenServerAndHoloLens
{
    /**
    https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/abstract-and-sealed-classes-and-class-members
    enforce a derived class to override a method
    */
    [Serializable]
    public abstract class DetectionResultWithUniqueNumEntries : DetectionResult
    {
        public abstract override uint NumEntries { get; }
    }
}
