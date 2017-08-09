﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver.Tests.Examples.Types
{
    // <example>
    public class CallsYouBackOnCreate
    {
        public CallsYouBackOnCreate(Action<CallsYouBackOnCreate> callback)
        {
            callback(this);
        }
    }
    // </example>
}
