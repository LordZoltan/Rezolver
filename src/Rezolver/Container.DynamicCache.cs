﻿// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


#if USEDYNAMIC

using Rezolver.Compilation;
using Rezolver.Compilation.Expressions;
using Rezolver.Targets;
using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace Rezolver
{
    public partial class Container
    {
        private sealed class DynamicCache
        {
            private abstract class ContainerCache
            {
                public abstract ResolveContext GetDefaultContext<TService>();
                public abstract ResolveContext GetDefaultContext(Type serviceType);

                public abstract Func<ResolveContext, TService> GetFactory<TService>();
                public abstract Func<ResolveContext, object> GetFactory(Type serviceType);

                public abstract TService Resolve<TService>();
                public abstract TService Resolve<TService>(ResolveContext context);

                public abstract object Resolve(Type serviceType);
                public abstract object Resolve(ResolveContext context);
            }

            private class Entry
            {
                public ResolveContext ResolveContext;
                public Func<ResolveContext, object> Factory;

                [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
                public object Resolve() => Factory(ResolveContext);
                [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
                public object Resolve(ResolveContext context) => Factory(context);
            }

            private sealed class ContainerCache<TContainer> : ContainerCache
            {
                private static Container TheContainer;

                private static PerTypeCache<Entry>  _entries = new PerTypeCache<Entry>(t => (Entry)Activator.CreateInstance(typeof(Entry<>).MakeGenericType(typeof(TContainer), t)));

                public ContainerCache(Container container)
                {
                    TheContainer = container;
                }

                public override Func<ResolveContext, TService> GetFactory<TService>()
                {
                    return Entry<TService>.Compiled.FactoryStrong;
                }

                public override Func<ResolveContext, object> GetFactory(Type serviceType)
                {
                    return _entries.Get(serviceType).Factory;
                }

                public override ResolveContext GetDefaultContext<TService>()
                {
                    return Entry<TService>.Context.Value;
                }

                public override ResolveContext GetDefaultContext(Type serviceType)
                {
                    return _entries.Get(serviceType).ResolveContext;
                }

                public override TService Resolve<TService>()
                {
                    return Entry<TService>.Compiled.FactoryStrong(Entry<TService>.Context.Value);
                }

                public override TService Resolve<TService>(ResolveContext context)
                {
                    return Entry<TService>.Compiled.FactoryStrong(context);
                }

                public override object Resolve(Type serviceType)
                {
                    return _entries.Get(serviceType).Resolve();
                }

                public override object Resolve(ResolveContext context)
                {
                    return _entries.Get(context.RequestedType).Resolve(context);
                }

                private class Entry<TService> : Entry
                {
                    public static class Context
                    {
                        public static readonly ResolveContext Value = new ResolveContext(TheContainer, typeof(TService));
                    }

                    public static class Compiled
                    {
                        public static readonly Func<ResolveContext, object> Factory = TheContainer.GetWorker(Context.Value);
                        public static readonly Func<ResolveContext, TService> FactoryStrong = TheContainer.GetWorker<TService>(Context.Value);
                    }

                    public Entry()
                    {
                        // lift the fields out of the statics
                        ResolveContext = Context.Value;
                        Factory = Compiled.Factory;
                    }
                }
            }

            private readonly ContainerCache _cache;

            public DynamicCache(Container parent)
            {
                var (assembly, module) = DynamicAssemblyHelper.Create("Containers");
                var fakeContainerType = module.DefineType($"ContainerHook", TypeAttributes.Class | TypeAttributes.Abstract | TypeAttributes.Sealed).CreateType();
                _cache = (ContainerCache)Activator.CreateInstance(typeof(ContainerCache<>).MakeGenericType(fakeContainerType), parent);
            }

            [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
            public ResolveContext GetContext<TService>() => _cache.GetDefaultContext<TService>();

            [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
            public ResolveContext GetContext(Type serviceType) => _cache.GetDefaultContext(serviceType);

            [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
            public Func<ResolveContext, TService> GetFactory<TService>() => _cache.GetFactory<TService>();

            [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
            public Func<ResolveContext, object> GetFactory(Type serviceType) => _cache.GetFactory(serviceType);

            [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
            public TService Resolve<TService>() => _cache.Resolve<TService>();

            [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
            public TService Resolve<TService>(ResolveContext context) => _cache.Resolve<TService>(context);

            [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
            public object Resolve(Type serviceType) => _cache.Resolve(serviceType);

            [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
            public object Resolve(ResolveContext context) => _cache.Resolve(context);
        }
    }
}
#endif