﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Rezolver.Compilation.Expressions
{
	/// <summary>
	/// Extensions for <see cref="IExpressionCompiler"/>.
	/// </summary>
	public static class ExpressionCompilerBuildExtensions
    {
		/// <summary>
		/// This method is a shortcut for building a lambda expression directly from an <see cref="ITarget" />.
		/// It calls <see cref="IExpressionCompiler.Build(ITarget, IExpressionCompileContext)" /> and passes the result to
		/// <see cref="IExpressionCompiler.BuildResolveLambda(Expression, IExpressionCompileContext)" />, which should yield an
		/// optimised lambda expression for the expression produced from the target which can then be
		/// compiled and used as the factory for that target.
		/// </summary>
		/// <param name="compiler">The compiler.</param>
		/// <param name="target">The target.</param>
		/// <param name="context">The current compilation context.</param>
		public static Expression<Func<ResolveContext, object>> BuildResolveLambda(this IExpressionCompiler compiler, ITarget target, IExpressionCompileContext context)
		{
			compiler.MustNotBeNull(nameof(compiler));
			target.MustNotBeNull(nameof(target));

			var expression = compiler.Build(target, context);
			return compiler.BuildResolveLambda(expression, context);
		}
	}
}