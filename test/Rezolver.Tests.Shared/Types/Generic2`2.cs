﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.Types
{
	public class Generic2<Ta, Tb> : GenericBase<Ta>, IGeneric2<Ta, Tb>
	{
		public Tb ValueB { get; protected set; }
	}
}
