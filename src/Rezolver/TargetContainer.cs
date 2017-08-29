﻿// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using Rezolver.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rezolver
{
    /// <summary>
    /// Root container for <see cref="ITarget"/>s that can be used as the backing for the standard
    /// <see cref="IContainer"/> classes - <see cref="Container"/> and <see cref="ScopedContainer"/>.
    /// 
    /// Stores and retrieves registrations of <see cref="ITarget"/>s, is also Generic type aware,
    /// unlike its base class - <see cref="TargetDictionaryContainer"/>.
    /// </summary>
    /// <remarks>This is the type used by default for the <see cref="ContainerBase.Targets"/> of all
    /// the standard containers in the core framework, e.g. <see cref="Container"/>, 
    /// <see cref="ScopedContainer"/> etc, when you don't supply an instance of an 
    /// <see cref="ITargetContainer"/> explicitly on construction.
    /// 
    /// Although you can derive from this class to extend its functionality; it's also possible to 
    /// extend it via configuration (see <see cref="ITargetContainerConfig"/>) - which is how, for example,
    /// the framework enables automatic injection of enumerables (see <see cref="Configuration.InjectEnumerables"/>) and
    /// lists (see <see cref="Configuration.InjectLists"/>).
    /// 
    /// The <see cref="DefaultConfig"/> is used for new instances which are not passed an explicit configuration.</remarks>
    public class TargetContainer : TargetDictionaryContainer
    {
        /// <summary>
        /// The default configuration used for <see cref="TargetContainer"/> objects created via the <see cref="TargetContainer.TargetContainer(ITargetContainerConfig)"/>
        /// constructor when no configuration is explicitly passed.
        /// </summary>
        /// <remarks>The simplest way to configure all target container instances is to add/remove configs to this collection.
        /// 
        /// Note also that the <see cref="OverridingTargetContainer"/> class also uses this.
        /// 
        /// #### Default configurations
        /// 
        /// The configurations applied by default are:
        /// 
        /// - <see cref="Configuration.InjectEnumerables"/>
        /// - <see cref="Configuration.InjectArrays"/>
        /// - <see cref="Configuration.InjectLists"/>
        /// - <see cref="Configuration.InjectCollections"/>
        /// - <see cref="Configuration.InjectResolveContext"/>
        /// </remarks>
        public static CombinedTargetContainerConfig DefaultConfig { get; } = new CombinedTargetContainerConfig(new ITargetContainerConfig[]
        {
            Configuration.InjectEnumerables.Instance,
            Configuration.InjectArrays.Instance,
            Configuration.InjectLists.Instance,
            Configuration.InjectCollections.Instance,
            Configuration.InjectResolveContext.Instance
        });

        /// <summary>
        /// Constructs a new instance of the <see cref="TargetContainer"/> class.
        /// </summary>
        /// <param name="config">Optional.  The configuration to apply to this target container.  If null, then
        /// the <see cref="DefaultConfig"/> is used.
        /// </param>
        /// <remarks>Note to inheritors: this constructor will throw an <see cref="InvalidOperationException"/> if called by derived
        /// classes.  You must instead use the <see cref="TargetContainer.TargetContainer()"/> constructor and apply configuration in your
        /// constructor.</remarks>
        public TargetContainer(ITargetContainerConfig config = null)
        {
            if (this.GetType() != typeof(TargetContainer))
                throw new InvalidOperationException("Derived types must not use this constructor because it triggers virtual method calls via the configuration callbacks.  Please use the protected parameterless constructor instead");

            (config ?? DefaultConfig).Configure(this);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="TargetContainer"/> class without attaching any
        /// <see cref="ITargetContainerConfig"/> to it.  This is desirable for derived types as behaviours typically
        /// will invoke methods on this target container which are declared virtual and which are, therefore, 
        /// unsafe to be called during construction.
        /// </summary>
        protected TargetContainer()
        {

        }

        private static bool ShouldUseGenericTypeDef(Type serviceType)
        {
            return TypeHelpers.IsGenericType(serviceType) && !TypeHelpers.IsGenericTypeDefinition(serviceType);
        }

        /// <summary>
        /// Called to get the type that's to be used to fetch a child <see cref="ITargetContainer"/> for targets registered
        /// against a given <paramref name="serviceType"/>.
        /// </summary>
        /// <param name="serviceType">The service type - usually pulled from the <see cref="ITarget.DeclaredType"/> of a 
        /// <see cref="ITarget"/> that is to be registered, or the service type passed to <see cref="ITargetContainer.Fetch(Type)"/>.</param>
        /// <returns>The redirected type, or the <paramref name="serviceType"/> if no type redirection is necessary.</returns>
        protected override Type GetTargetContainerType(Type serviceType)
        {
            if (ShouldUseGenericTypeDef(serviceType))
                return serviceType.GetGenericTypeDefinition();

            var typeResolver = this.GetOption<ITargetContainerTypeResolver>(serviceType);
            return typeResolver?.GetContainerType(serviceType) ?? base.GetTargetContainerType(serviceType);
        }


        /// <summary>
        /// Implementation of <see cref="ITargetContainer.RegisterContainer(Type, ITargetContainer)"/>,
        /// overriding the base version to extend special support for open generic types and for 
        /// <see cref="ITargetContainerTypeResolver"/> options.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="container"></param>
        public override void RegisterContainer(Type type, ITargetContainer container)
        {
            // for explicit container registrations of a generic type, we must ensure that the container
            // exists for the open generic.
            if (ShouldUseGenericTypeDef(type))
            {
                EnsureContainer(type).RegisterContainer(type, container);
                return;
            }
            else
            {
                // should we be looking up a container type for the passed type?  I think so,
                // but all the tests I've created so far seem to suggest that it's not actually
                // required...
            }


            base.RegisterContainer(type, container);
        }

        /// <summary>
        /// Overrides <see cref="TargetDictionaryContainer.CreateContainer(Type)"/> to provide special support 
        /// for open generic types and to support <see cref="ITargetContainerFactory"/> options.
        /// </summary>
        /// <param name="targetContainerType"></param>
        /// <returns></returns>
        protected override ITargetContainer CreateContainer(Type targetContainerType)
        {
            if (TypeHelpers.IsGenericTypeDefinition(targetContainerType))
                return new GenericTargetContainer(Root, targetContainerType);
            var factory = this.GetOption<ITargetContainerFactory>(targetContainerType);
            //REVIEW: Support a default ITargetContainerFactory supplied on construction?  Or rely on Global option?
            return factory?.CreateContainer(targetContainerType, this, Root) ?? base.CreateContainer(targetContainerType);
        }

        /// <summary>
        /// Overrides the base method to block registration if the <paramref name="target"/> does not support the 
        /// <paramref name="serviceType"/> (checked by calling the target's <see cref="ITarget.SupportsType(Type)"/> method).
        /// </summary>
        /// <param name="target">The target to be registered.</param>
        /// <param name="serviceType">Optional - the type against which the target is to be registered, if different from the
        /// target's <see cref="ITarget.DeclaredType"/>.</param>
        public override void Register(ITarget target, Type serviceType = null)
        {
            target.MustNotBeNull(nameof(target));
            if (serviceType != null && !target.SupportsType(serviceType))
                throw new ArgumentException(string.Format(ExceptionResources.TargetDoesntSupportType_Format, serviceType), nameof(target));

            base.Register(target, serviceType);
        }
    }
}
