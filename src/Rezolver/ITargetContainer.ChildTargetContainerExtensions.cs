﻿// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rezolver.Runtime;

namespace Rezolver
{
    internal static class ChildTargetContainerExtensions
    {
        internal static Type GetChildContainerType(this ITargetContainer targets, Type serviceType, IRootTargetContainer root)
        {
            return root.GetOption<ITargetContainerTypeResolver>(serviceType)?.GetContainerType(serviceType);
        }

        internal static Type GetChildContainerType(this IRootTargetContainer targets, Type serviceType)
        {
            return GetChildContainerType(targets, serviceType, targets);
        }

        internal static ITargetContainer CreateChildContainer(this ITargetContainer targets, Type targetContainerType, IRootTargetContainer root)
        {
            return root.GetOption<ITargetContainerFactory>(targetContainerType)?.CreateContainer(targetContainerType, targets, root);
        }

        internal static ITargetContainer CreateChildContainer(this IRootTargetContainer targets, Type targetContainerType)
        {
            return CreateChildContainer(targets, targetContainerType, targets);
        }
    }
}
