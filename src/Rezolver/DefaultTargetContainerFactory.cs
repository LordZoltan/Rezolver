﻿// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections.Generic;
using System.Text;

namespace Rezolver
{
    internal class DefaultTargetContainerFactory : ITargetContainerFactory
    {
        public static DefaultTargetContainerFactory Instance { get; } = new DefaultTargetContainerFactory();

        private DefaultTargetContainerFactory()
        {
        }

        public ITargetContainer CreateContainer(Type type, ITargetContainer targets, ITargetContainer rootTargetContainer)
        {
            return new TargetListContainer(rootTargetContainer, type);
        }
    }
}