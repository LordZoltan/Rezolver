﻿// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using Rezolver.Targets;

namespace Rezolver
{
    public static partial class TargetContainerExtensions
    {
        /// <summary>
        /// Generic version of <see cref="RegisterAlias(ITargetContainer, Type, Type)"/>, see that method for more.
        /// </summary>
        /// <typeparam name="TAlias">Type being registered as an alias to another type</typeparam>
        /// <typeparam name="TOriginal">The target type of the alias.</typeparam>
        /// <param name="targetContainer">The target container in which the alias is to be registered</param>
        public static void RegisterAlias<TAlias, TOriginal>(this ITargetContainer targetContainer)
        {
            RegisterAlias(targetContainer, typeof(TAlias), typeof(TOriginal));
        }

        /// <summary>
        /// Registers an alias for one type to another type.
        ///
        /// The created entry will effectively represent a second Resolve call into the container for the aliased type.
        /// </summary>
        /// <param name="targetContainer">The builder in which the alias is to be registered</param>
        /// <param name="aliasType">The type to be registered as an alias</param>
        /// <param name="originalType">The type being aliased.</param>
        /// <remarks>Use this when it's important that a given target type is always served through the same compiled target, even when the consumer
        /// expects it to be of a different type.  A very common scenario is when you have a singleton instance of the <paramref name="originalType" />,
        /// and need to serve that same instance for <paramref name="aliasType"/>.  If you register the same singleton for both types, you get two
        /// separate singletons for each type, whereas if you create an alias, both will be served by the same alias.
        /// </remarks>
        public static void RegisterAlias(this ITargetContainer targetContainer, Type aliasType, Type originalType)
        {
            if(targetContainer == null) throw new ArgumentNullException(nameof(targetContainer));
            if (aliasType == originalType) throw new ArgumentException("The aliased type and its alias must be different", nameof(aliasType));

            ITarget target = new ResolvedTarget(originalType);
            // if there's no implicit conversion to our alias type from the aliased type, but there is
            // the other way around, then we need to stick in an explicit change of type, otherwise the registration will
            // fail.  This does, unfortunately, give rise to the situation where we could be performing an invalid cast - but that
            // will come out in the wash at runtime.
            if (!aliasType.IsAssignableFrom(originalType) &&
              originalType.IsAssignableFrom(aliasType))
            {
                target = new ChangeTypeTarget(target, aliasType);
            }

            targetContainer.Register(target, aliasType);
        }
    }
}
