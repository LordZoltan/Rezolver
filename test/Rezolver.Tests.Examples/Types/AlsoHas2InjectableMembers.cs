﻿// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver.Tests.Examples.Types
{
    //<example>
    // an exact
    public class AlsoHas2InjectableMembers
    {
        public MyService1 Service1 { get; set; }
        public MyService2 Service2 { get; set; }
    }
    //</example>
}
