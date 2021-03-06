﻿// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;

namespace Rezolver.Sdk
{
    /// <summary>
    /// This exception is related to the <see cref="IDependant"/> functionality used by Rezolver.
    ///
    /// It is raised when two objects dependn on each other, or if a required dependency is missing
    /// from the collection passed to a <see cref="DependencyMetadata"/> object's
    /// <see cref="DependencyMetadata.GetDependencies{T}(IEnumerable{T})"/> method.
    /// </summary>
    /// <remarks>Creation of this exception is currently kept internal</remarks>
    [System.Serializable]
    public sealed class DependencyException : Exception
    {
        internal DependencyException() { }

        internal DependencyException(string message) : base(message) { }

        internal DependencyException(string message, Exception inner) : base(message, inner) { }
#pragma warning disable CS0628 // New protected member declared in sealed class
        /// <summary>
        /// Required constructor for Serialization
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected DependencyException(

          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
#pragma warning restore CS0628 // New protected member declared in sealed class
    }
}
