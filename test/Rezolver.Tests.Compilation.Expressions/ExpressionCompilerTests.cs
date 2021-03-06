﻿using Rezolver.Configuration;
using Rezolver.Compilation;
using Rezolver.Compilation.Expressions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Rezolver.Tests.Compilation.Expressions
{
	public class ExpressionCompilerTests : Rezolver.Tests.Compilation.Specification.CompilerTestsBase<ExpressionCompiler>
	{
		public ExpressionCompilerTests(ITestOutputHelper output)
			: base(output)
		{

		}

		protected override IContainerConfig<ITargetCompiler> GetCompilerConfig([CallerMemberName] string testName = null)
		{
            return ExpressionCompilation.Instance;
		}
	}
}
