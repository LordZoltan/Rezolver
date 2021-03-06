﻿// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Linq;
using System.Reflection;

namespace Rezolver
{
    /// <summary>
    /// Represents a binding to a method whose arguments will be supplied by <see cref="ITarget" /> instances.
    /// </summary>
    public class MethodBinding
    {
        /// <summary>
        /// Gets the method to be invoked.
        /// </summary>
        /// <value>The method.</value>
        public MethodBase Method { get; }
        /// <summary>
        /// Gets the argument bindings for the method call.
        ///
        /// Never null but can be empty.
        /// </summary>
        /// <value>The bound arguments.</value>
        public ParameterBinding[] BoundArguments { get; }
        /// <summary>
        /// Initializes a new instance of the <see cref="MethodBinding"/> class.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="boundArgs">Optional.  The bound arguments.  Can be null or empty.</param>
        public MethodBinding(MethodBase method, ParameterBinding[] boundArgs = null)
        {
            if (method == null) throw new ArgumentNullException(nameof(method));
            if (method.IsAbstract) throw new ArgumentException("Method cannot be abstract", nameof(method));

            if (boundArgs != null)
            {
                var parameters = method.GetParameters();
                foreach (var boundArg in boundArgs)
                {
                    if (boundArg == null) throw new ArgumentException("All parameter bindings must be non-null", nameof(boundArgs));
                    if (!parameters.Contains(boundArg.Parameter)) throw new ArgumentException("All parameter bindings must be for parameters declared on the method", nameof(boundArgs));
                }
            }

            Method = method;
            BoundArguments = boundArgs ?? ParameterBinding.None;
        }
    }
}
