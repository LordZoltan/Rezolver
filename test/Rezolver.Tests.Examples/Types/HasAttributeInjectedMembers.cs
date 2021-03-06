﻿// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.Examples.Types
{
    //<example>
    public class HasAttributeInjectedMembers
    {
        [Inject]
        public IMyService InjectedServiceField;

        [Inject(typeof(MyService6))]
        public IMyService InjectedServiceProp { get; set; }

        //these two will not be injeccted

        public IMyService ServiceField;
        public IMyService ServiceProp { get; set; }
    }
    //</example>
}
