﻿// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Reflection;

namespace Rezolver
{
    internal class BindableCollectionType
    {
        public BindableCollectionType(Type type, MethodInfo addMethod, Type elementType)
        {
            Type = type;
            AddMethod = addMethod;
            ElementType = elementType;
        }

        public Type Type { get; }

        public MethodInfo AddMethod { get; }

        public Type ElementType { get; }
    }
}
