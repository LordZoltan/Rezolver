﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.Types
{
	public class TwiceNestedGenericA<T> : IGeneric<IGeneric<IEnumerable<T>>>
	{
		public IGeneric<IEnumerable<T>> Value
		{
			get; private set;
		}

		//even more complicated now - double nesting of generic parameters
		public void Foo()
		{
			throw new NotImplementedException();
		}
	}
}
