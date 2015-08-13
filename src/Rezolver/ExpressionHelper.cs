using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace Rezolver
{
	/// <summary>
	/// This static class contains methods and properties to aid in building expressions suitable to be used in
	/// rezolver targets
	/// </summary>
	public static class ExpressionHelper
	{
		/// <summary>
		/// A MethodInfo object representing the <see cref="LifetimeScopeRezolverExtensions.GetScopeRoot(ILifetimeScopeRezolver)"/> method - to aid in code generation
		/// where the target scope for tracking an object is the root scope, not the current scope.
		/// </summary>
		public static readonly MethodInfo Scope_GetScopeRootMethod = MethodCallExtractor.ExtractCalledMethod(() => LifetimeScopeRezolverExtensions.GetScopeRoot(null));

		/// <summary>
		/// A MethodInfo object representing the generic definition <see cref="LifetimeScopeRezolverExtensions.GetOrAdd{T}(ILifetimeScopeRezolver, RezolveContext, Func{RezolveContext, T}, bool, bool)"/>
		/// </summary>
		public static readonly MethodInfo Scope_GetOrAddGenericMethod = MethodCallExtractor.ExtractCalledMethod(() => LifetimeScopeRezolverExtensions.GetOrAdd<object>(null, null, null, false)).GetGenericMethodDefinition();
		/// <summary>
		/// This parameter expression is to be used by all targets and rezolvers in this library by default to perform late binding 
		/// to a rezolver provided at run time when a caller is trying to resolve something through code built from
		/// a target.
		/// </summary>
		public static readonly ParameterExpression DynamicRezolverParam = Expression.Parameter(typeof(IRezolver), "dynamicRezolver");
		public static readonly ParameterExpression RezolverNameParameter = Expression.Parameter(typeof(string), "name");

		public static readonly ParameterExpression RezolveContextParameter = Expression.Parameter(typeof(RezolveContext), "context");

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
		/// <remarks>Note that the <see cref="RezolveTargetCompilerBase"/> abstract class uses this method to get the expression tree
		/// for a target that is to be compiled, and the <see cref="RezolveTargetDelegateCompiler"/> uses that expression tree
		/// as-is.
		/// 
		/// Note, however, that some compilers might override this behaviour, or rewrite the generated expression more - in which case
		/// this code wouldn't be suitable for those compilers.
		/// 
		/// This method, therefore, is exposed to provide a surefire way to generate a whole lambda that can be compiled into an in-memory delegate
		/// for re-use in other targets and scenarios.
		/// 
		/// The SingletonTarget, for example, uses this method during its own <see cref="IRezolveTarget.CreateExpression(CompileContext)"/>
		/// implementation to get a nested lambda for the wrapped target, so that it can dynamically construct a Lazy whose factory
		/// method is the code we'd normally produce for a target.</remarks>
		public static Expression GetLambdaBodyForTarget(IRezolveTarget target, CompileContext context)
		{
			context = new CompileContext(context, context.TargetType, true, context.SuppressScopeTracking);
			var toBuild = target.CreateExpression(context);
			if (toBuild.Type != typeof(object))
				toBuild = Expression.Convert(toBuild, typeof(object));
			//if we have shared conditionals, then we want to try and reorder them as the intention
			//of the use of shared expressions is to consolidate them into one.  We do this on the boolean
			//expressions that might be used as tests for conditionals
			var sharedConditionalTests = context.SharedExpressions.Where(e => e.Type == typeof(Boolean)).ToArray();
			if (sharedConditionalTests.Length != 0)
				toBuild = new ConditionalRewriter(toBuild, sharedConditionalTests).Rewrite();

			toBuild = toBuild.Optimise();

			//shared locals are local variables generated by targets that would normally be duplicated
			//if multiple targets of the same type are used in one compiled target.  By sharing them,
			//they reduce the size of the stack required for any generated code, but in turn 
			//the compiler is required to lift them out and add them to an all-encompassing BlockExpression
			//surrounding all the code - otherwise they won't be in scope.
			var sharedLocals = context.SharedExpressions.OfType<ParameterExpression>().ToArray();
			if (sharedLocals.Length != 0)
			{
				toBuild = Expression.Block(toBuild.Type, sharedLocals, new BlockExpressionLocalsRewriter(sharedLocals).Visit(toBuild));
			}

			return toBuild;
		}

		/// <summary>
		/// First gets the lambda body using <see cref="GetLambdaBodyForTarget(IRezolveTarget, CompileContext)"/>, then
		/// passes that as the body for the returned expression, using also the <see cref="CompileContext.RezolveContextParameter"/>
		/// <see cref="ParameterExpression"/> from the <paramref name="context"/> parameter as the expression for the RezolveContext that's
		/// passed to the compiled method when invoked.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="context"></param>
		/// <returns></returns>
		public static Expression<Func<RezolveContext, object>> GetResolveLambdaForTarget(IRezolveTarget target, CompileContext context)
		{
			return GetResolveLambdaForExpression(GetLambdaBodyForTarget(target, context), context);
		}

		internal static Expression<Func<RezolveContext, object>> GetResolveLambdaForExpression(Expression toCompile, CompileContext context)
		{
			return Expression.Lambda<Func<RezolveContext, object>>(toCompile, context.RezolveContextParameter);
		}

		/// <summary>
		/// Returns an expression that represents a call to the <see cref="Scope_GetScopeRootMethod"/> extension method on the scope of the 
		/// RezolveContext passed to a compiled object target.
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		public static Expression Make_Scope_GetScopeRootCallExpression(CompileContext context)
		{
			return Expression.Call(Scope_GetScopeRootMethod, context.ContextScopePropertyExpression);
		}

		public static Expression Make_Scope_GetOrAddCallExpression(CompileContext context, Type objectType, LambdaExpression factoryExpression, Expression iDisposableOnly = null)
		{
			return Expression.Call(Scope_GetOrAddGenericMethod.MakeGenericMethod(objectType),
					context.ContextScopePropertyExpression,
					context.RezolveContextParameter,
					factoryExpression,
					iDisposableOnly ?? Expression.Constant(true));
		}
	}
}