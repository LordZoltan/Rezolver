﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests.Types
{
    public class Decorator : IDecorated
    {
        public IDecorated Decorated { get; }
        public Decorator(IDecorated decorated)
        {
            Assert.NotNull(decorated);
            Decorated = decorated;
        }
    }
}
