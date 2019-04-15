﻿// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Concurrent;

namespace Rezolver.Targets
{
    /// <summary>
    /// A target that simply creates a default instance of a given type.  I.e. the same
    /// as doing default(type) in C#.
    /// </summary>
    /// <remarks>The type also implements the <see cref="ICompiledTarget"/> interface for direct
    /// resolving.</remarks>
    public class DefaultTarget : TargetBase, IFactoryProvider, IInstanceProvider, IDirectTarget
    {
        private static class Default<T>
        {
            public static readonly T Value = default;
            public static readonly Func<ResolveContext, object> Factory;
            public static readonly Func<ResolveContext, T> Factory2;

            static Default()
            {
                Factory = c => Value;
                Factory2 = c => Value;
            }
        }

        private static readonly ConcurrentDictionary<Type, object> _defaultCallbacks = new ConcurrentDictionary<Type, object>();
        private static readonly ConcurrentDictionary<Type, Func<ResolveContext, object>> _defaultFactories = new ConcurrentDictionary<Type, Func<ResolveContext, object>>();

        // internal to allow other classes take advantage of late-bound defaults
        internal static object GetDefault(Type type)
        {
            return _defaultCallbacks.GetOrAdd(type, t =>
            {
                return typeof(Default<>)
                    .MakeGenericType(t)
                    .GetStaticField("Value")
                    .GetValue(null);
            });
        }

        internal static Func<ResolveContext, object> GetFactory(Type type)
        {
            return _defaultFactories.GetOrAdd(type, t =>
            {
                return (Func<ResolveContext, object>)typeof(Default<>)
                    .MakeGenericType(t)
                    .GetStaticField("Factory")
                    .GetValue(null);
            });
        }

        internal static Func<ResolveContext, T> GetFactory<T>()
        {
            return Default<T>.Factory2;
        }

        /// <summary>
        /// Override of <see cref="TargetBase.ScopeBehaviour"/> - always returns
        /// <see cref="ScopeBehaviour.None"/>.
        /// </summary>
        /// <value>The scope behaviour.</value>
        public override ScopeBehaviour ScopeBehaviour
        {
            get
            {
                return ScopeBehaviour.None;
            }
        }

        private readonly Type _declaredType;

        /// <summary>
        /// Always equal to the type for which the default value will be returned
        /// </summary>
        public override Type DeclaredType
        {
            get { return _declaredType; }
        }

        /// <summary>
        /// Gets the actual default value represented by this instance.
        /// </summary>
        public object Value
        {
            get
            {
                return GetDefault(DeclaredType);
            }
        }

        /// <summary>
        /// Implementation of <see cref="IFactoryProvider.Factory"/>
        /// </summary>
        public Func<ResolveContext, object> Factory => GetFactory(_declaredType);

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultTarget"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        public DefaultTarget(Type type)
        {
            _declaredType = type ?? throw new ArgumentNullException(nameof(type));

        }

        object IDirectTarget.GetValue() => Value;

        object IInstanceProvider.GetInstance(ResolveContext context) => Value;
    }
}