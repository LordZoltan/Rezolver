﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver.Tests.Types
{
    public delegate BaseClass BaseClassFactory();

    public delegate RequiresInt RequiresIntFactory(int value);

    public delegate IHasIntValue HasIntValueParameterisedFactory(int value);
}
