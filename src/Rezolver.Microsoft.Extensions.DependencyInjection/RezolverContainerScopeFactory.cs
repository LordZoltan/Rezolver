﻿// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver
{
	internal class RezolverContainerScopeFactory : IServiceScopeFactory
	{
		private IContainer _container;
		public RezolverContainerScopeFactory(IContainer container)
		{
			_container = container;
		}

		public IServiceScope CreateScope()
		{
			return new RezolverServiceScope(_container.CreateScope());
		}
	}
}
