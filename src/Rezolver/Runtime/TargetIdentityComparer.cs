﻿// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections.Generic;
using System.Text;

namespace Rezolver.Runtime
{
    internal class TargetIdentityComparer : IEqualityComparer<ITarget>
    {
        internal static TargetIdentityComparer Instance { get; } = new TargetIdentityComparer();

        private TargetIdentityComparer() { }

        public bool Equals(ITarget x, ITarget y)
        {
            return x?.Id == y?.Id;
        }

        public int GetHashCode(ITarget obj)
        {
            return ((obj?.Id) ?? Guid.Empty).GetHashCode();
        }
    }
}