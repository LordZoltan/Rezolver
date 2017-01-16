// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using Rezolver.Compilation;

namespace Rezolver
{
	/// <summary>
	/// This static class contains methods and properties to aid in building expressions in an 
	/// implementation of the <see cref="ITarget.CreateExpression(ICompileContext)"/> method.
	/// </summary>
	public static class ExpressionHelper
	{
		/// <summary>
		/// A MethodInfo object representing the <see cref="LifetimeScopeRezolverExtensions.GetScopeRoot(IScopedContainer)"/> method - to aid in code generation
		/// where the target scope for tracking an object is the root scope, not the current scope.
		/// </summary>
		public static readonly MethodInfo Scope_GetScopeRootMethod = MethodCallExtractor.ExtractCalledMethod(() => LifetimeScopeRezolverExtensions.GetScopeRoot(null));

		/// <summary>
		/// A MethodInfo object representing the generic definition <see cref="LifetimeScopeRezolverExtensions.GetOrAdd{T}(IScopedContainer, ResolveContext, Func{ResolveContext, T}, bool, bool)"/>
		/// </summary>
		public static readonly MethodInfo Scope_GetOrAddGenericMethod = MethodCallExtractor.ExtractCalledMethod(() => LifetimeScopeRezolverExtensions.GetOrAdd<object>(null, null, null, false)).GetGenericMethodDefinition();

		/// <summary>
		/// The default <see cref="ResolveContext"/> parameter expression to be used during code generation in an implementation of <see cref="ITarget.CreateExpression(ICompileContext)"/>
		/// </summary>
		public static readonly ParameterExpression RezolveContextParameterExpression = Expression.Parameter(typeof(ResolveContext), "context");

		/// <summary>
		/// Provides a standard way to create the method body for a lambda that, when compiled (with the correct signature) will 
		/// execute code that will produce the object represented by the target.
		/// 
		/// Some rewriting optimisations are applied to the expression tree, the resulting expression tree will be able to be 
		/// compiled straight to a method if passed into a LambdaExpression.
		/// 
		/// </summary>
		/// <param name="target"></param>
		/// <param name="context"></param>
		/// <returns></returns>
		/// <remarks>Note that the <see cref="TargetCompilerBase"/> abstract class uses this method to get the expression tree
		/// for a target that is to be compiled, and the <see cref="TargetDelegateCompiler"/> uses that expression tree
		/// as-is.
		/// 
		/// Note, however, that some compilers might override this behaviour, or rewrite the generated expression more - in which case
		/// this code wouldn't be suitable for those compilers.
		/// 
		/// This method, therefore, is exposed to provide a surefire way to generate a whole lambda that can be compiled into an in-memory delegate
		/// for re-use in other targets and scenarios.
		/// 
		/// The SingletonTarget, for example, uses this method during its own <see cref="ITarget.CreateExpression(ICompileContext)"/>
		/// implementation to get a nested lambda for the wrapped target, so that it can dynamically construct a Lazy whose factory
		/// method is the code we'd normally produce for a target.</remarks>
		[Obsolete("Should no longer use the ExpressionHelper expression generation functions - use the ExpressionCompiler in Rezolver.Compilation.Expressions", true)]
		public static Expression GetLambdaBodyForTarget(ITarget target, ICompileContext context)
		{
			throw new NotSupportedException("retired method");
		}

		/// <summary>
		/// First gets the lambda body using <see cref="GetLambdaBodyForTarget(ITarget, ICompileContext)"/>, then
		/// passes that as the body for the returned expression, using also the <see cref="ICompileContext.ResolveContextExpression"/>
		/// <see cref="ParameterExpression"/> from the <paramref name="context"/> parameter as the expression for the ResolveContext that's
		/// passed to the compiled method when invoked.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="context"></param>
		/// <returns></returns>
		[Obsolete("Should no longer use the ExpressionHelper expression generation functions - use the ExpressionCompiler in Rezolver.Compilation.Expressions", true)]
		public static Expression<Func<ResolveContext, object>> GetResolveLambdaForTarget(ITarget target, ICompileContext context)
		{
			return GetResolveLambdaForExpression(GetLambdaBodyForTarget(target, context), context);
		}

		[Obsolete("Should no longer use the ExpressionHelper expression generation functions - use the ExpressionCompiler in Rezolver.Compilation.Expressions", true)]
		internal static Expression<Func<ResolveContext, object>> GetResolveLambdaForExpression(Expression toCompile, ICompileContext context)
		{
			return Expression.Lambda<Func<ResolveContext, object>>(toCompile, context.RezolveContextExpression);
		}

		/// <summary>
		/// Returns an expression that represents a call to the <see cref="Scope_GetScopeRootMethod"/> extension method on the scope of the 
		/// ResolveContext passed to a compiled object target.
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		public static Expression Make_Scope_GetScopeRootCallExpression(ICompileContext context)
		{
			return Expression.Call(Scope_GetScopeRootMethod, context.ContextScopePropertyExpression);
		}

		/// <summary>
		/// Makes an expression which represents calling the <see cref="LifetimeScopeRezolverExtensions.GetOrAdd{T}(IScopedContainer, ResolveContext, Func{ResolveContext, T}, bool)"/>
		/// function for the passed <paramref name="objectType"/>.
		/// 
		/// Used automatically by the built-in scope tracking behaviour, but can also be used by your own custom target if you want
		/// to take control of its scope tracking behaviour.
		/// </summary>
		/// <param name="context">The compile context.</param>
		/// <param name="objectType">Type of the object to be stored or retrieved.</param>
		/// <param name="factoryExpression">Lambda which should be executed to obtain a new instance if a matching object is not already in scope.</param>
		/// <param name="iDisposableOnly">Expected to be a boolean expression indicating whether only IDisposables should be tracked in the scope.  The default 
		/// (if not provided) then will be set to 'true'.</param>
		public static Expression Make_Scope_GetOrAddCallExpression(ICompileContext context, Type objectType, LambdaExpression factoryExpression, Expression iDisposableOnly = null)
		{
			return Expression.Call(Scope_GetOrAddGenericMethod.MakeGenericMethod(objectType),
				context.ContextScopePropertyExpression,
				context.RezolveContextExpression,
				factoryExpression,
				iDisposableOnly ?? Expression.Constant(true));
		}
	}
}