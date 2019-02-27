﻿// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver
{
    internal class StubContainer : IContainer
    {
        internal static StubContainer Instance { get; } = new StubContainer();

        private StubContainer() { }

        public bool CanResolve(ResolveContext context)
        {
            throw new InvalidOperationException("The ResolveContext has no Container set");
        }

        public IContainerScope CreateScope()
        {
            throw new InvalidOperationException("The ResolveContext has no Container set");
        }

        public ICompiledTarget GetCompiledTarget(ResolveContext context)
        {
            throw new InvalidOperationException("The ResolveContext has no Container set");
        }

        public object GetService(Type serviceType)
        {
            throw new InvalidOperationException("The ResolveContext has no Container set");
        }

        public object Resolve(ResolveContext context)
        {
            throw new InvalidOperationException("The ResolveContext has no Container set");
        }

        public bool TryResolve(ResolveContext context, out object result)
        {
            throw new InvalidOperationException("The ResolveContext has no Container set");
        }
    }
}
