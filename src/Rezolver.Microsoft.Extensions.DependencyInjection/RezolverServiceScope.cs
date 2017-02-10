﻿// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver
{
	internal class RezolverServiceScope : IServiceScope
	{
		public IServiceProvider ServiceProvider
		{
			get
			{
				return _scope;
			}
		}

		public void Dispose()
		{
			_scope.Dispose();
		}

		private IContainerScope _scope;

		public RezolverServiceScope(IContainerScope scope)
		{
			_scope = scope;
		}
	}
}
