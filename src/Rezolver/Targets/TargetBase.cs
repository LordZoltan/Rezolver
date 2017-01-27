﻿// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Linq.Expressions;
using System.Reflection;
using Rezolver.Compilation;

namespace Rezolver.Targets
{
	/// <summary>
	/// Abstract base class, suggested as a starting point for implementations of <see cref="ITarget"/>.
	/// </summary>
	public abstract class TargetBase : ITarget
	{
		///// <summary>
		///// Required for the scope tracking wrapping code.
		///// </summary>
		//private static readonly MethodInfo ILifetimeScopeRezolver_AddObject = MethodCallExtractor.ExtractCalledMethod((IScopedContainer s) => s.AddToScope(null, null));
		//private static readonly MethodInfo ILifetimeScopeRezolver_TrackIfScopedAndDisposableAndReturnGeneric =
		//    MethodCallExtractor.ExtractCalledMethod(() => LifetimeScopeRezolverExtensions.TrackIfScopedAndDisposableAndReturn<object>(null, null)).GetGenericMethodDefinition();

		/// <summary>
		/// this is probably going to be removed or at least changed.
		/// </summary>
		protected virtual bool SuppressScopeTracking
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// Implementation of <see cref="ITarget.UseFallback"/>
		/// 
		/// Base version always returns <c>false</c>.
		/// </summary>
		public virtual bool UseFallback
		{
			get { return false; }
		}

		///// <summary>
		///// This is called by <see cref="CreateExpression(ICompileContext)"/> after the derived class generates its expression
		///// via a call to <see cref="CreateExpressionBase(ICompileContext)"/> - unless <see cref="SuppressScopeTracking"/> is true either 
		///// on this object, or on the passed <paramref name="context"/>.
		///// 
		///// The purpose is to generate the code that will ensure that any instance produced will be tracked in a lifetime scope,
		///// if required.
		///// </summary>
		///// <param name="context">The current compile context.</param>
		///// <param name="expression">The code generated from the <see cref="CreateExpressionBase(ICompileContext)"/> method, albeit
		///// possibly rewritten and optimised.</param>
		///// <remarks>
		///// By default, if there is a lifetime scope, then its <see cref="IScopedContainer.AddToScope(object, ResolveContext)"/> 
		///// method is called with the object that's produced by the target, before the object is returned.  If there is no scope, then 
		///// no tracking is performed.
		///// 
		///// Note that, also, by default, an object will only be tracked in the scope if it's <see cref="IDisposable"/>.
		///// 
		///// As mentioned in the summary, if you need to disable the automatic generation of this scope tracking code, then you
		///// can override the <see cref="SuppressScopeTracking"/> property, and return false.  It can also be suppressed on a per-compilation
		///// basis by setting the <see cref="ICompileContext.SuppressScopeTracking"/> property of the <paramref name="context"/> to true.
		///// 
		///// This is something that the <see cref="ScopedTarget"/> does on its nested target, since by definition it generates
		///// its own explicit scope tracking code.
		///// 
		///// If the target simply needs to select a different scope from the current (at the time <see cref="IContainer.Resolve(ResolveContext)"/> 
		///// is called), then it can override the <see cref="CreateScopeSelectionExpression(ICompileContext, Expression)"/> method.
		///// </remarks>
		///// <returns></returns>
		//protected virtual Expression CreateScopeTrackingExpression(ICompileContext context, Expression expression)
		//{
		//  return Expression.Call(ILifetimeScopeRezolver_TrackIfScopedAndDisposableAndReturnGeneric.MakeGenericMethod(expression.Type),
		//      CreateScopeSelectionExpression(context, expression), expression);
		//}

		///// <summary>
		///// Called to generate the expression that represents the argument that'll be passed to the 
		///// <see cref="IScopedContainer.AddToScope(object, ResolveContext)"/> method when an object is being tracked in a lifetime scope.  
		///// By default, the base implementation generates an expression that represents null - because usually there really is little point in 
		///// adding a specific context along with the object being tracked, unless you're also grabbing instances back out of the scope which isn't
		///// done by the base class behaviour by default.
		///// </summary>
		///// <param name="context"></param>
		///// <param name="expression"></param>
		///// <returns></returns>
		//protected virtual Expression CreateRezolveContextExpressionForScopeAddCall(ICompileContext context, Expression expression)
		//{
		//  return Expression.Default(typeof(ResolveContext));
		//}

		///// <summary>
		///// Called by <see cref="CreateScopeTrackingExpression(ICompileContext, Expression)"/> to generate the code that selects the correct 
		///// scope instance that is to be used for scope tracking for the object produced by the code generated by 
		///// <see cref="CreateExpressionBase(ICompileContext)"/>.
		///// </summary>
		///// <param name="context"></param>
		///// <param name="expression"></param>
		///// <returns></returns>
		// protected virtual Expression CreateScopeSelectionExpression(ICompileContext context, Expression expression)
		// {
		//throw new NotSupportedException();
		//   //return context.ContextScopePropertyExpression;
		// }

		/// <summary>
		/// Implementation of <see cref="ITarget.SupportsType(Type)"/>. Returns a boolean indicating whether the target 
		/// is able to produce an instance of, or an instance that is compatible with, the passed <paramref name="type" />.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <remarks>It is strongly suggested that you use this method to check whether the target can construct
		/// an instance of a given type rather than performing any type checking yourself on the
		/// <see cref="DeclaredType" />, because an <see cref="ITarget" /> might be able to support a much wider
		/// range of types other than just those which are directly compatible with its <see cref="DeclaredType" />.
		/// For example, the <see cref="GenericConstructorTarget" /> is statically bound to an open generic, so therefore
		/// traditional type checks on the <see cref="DeclaredType" /> do not work.  That class' implementation of this
		/// method, however, contains the complex logic necessary to determine if the open generic can be closed into a
		/// generic type which is compatible with the given <paramref name="type" />.
		/// Implementations of <see cref="Compilation.ITargetCompiler" /> should always consult this function in their
		/// implementation of <see cref="Compilation.ITargetCompiler.CompileTarget(ITarget, Compilation.ICompileContext)" />
		/// to determine if the target is compatible with the <see cref="Compilation.CompileContext.TargetType" /> of the
		/// <see cref="Compilation.CompileContext" />
		/// Please note that any <paramref name="type" /> that's a generic type definition will always yield a false result,
		/// because it's impossible to build an instance of an open generic type.</remarks>
		public virtual bool SupportsType(Type type)
		{
			type.MustNotBeNull("type");
			return TypeHelpers.AreCompatible(DeclaredType, type) && !TypeHelpers.IsGenericTypeDefinition(type);
		}

		/// <summary>
		/// Gets the declared type of object that is constructed by this target.
		/// </summary>
		public abstract Type DeclaredType
		{
			get;
		}
	}
}