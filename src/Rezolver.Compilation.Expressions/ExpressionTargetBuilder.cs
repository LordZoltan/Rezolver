﻿// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Rezolver.Compilation.Expressions
{
	/// <summary>
	/// An <see cref="IExpressionBuilder"/> specialised for building the expression trees for the <see cref="ExpressionTarget"/>
	/// target type.
	/// 
	/// This builder takes care of all expressions, including lambdas (where additional parameters beyond the standard 
	/// <see cref="ResolveContext"/> are turned into local variables with injected values), producing an expression which can 
	/// be compiled by an <see cref="IExpressionCompiler"/> after a <see cref="TargetExpressionRewriter"/> has been used to
	/// expand any targets embedded in the expression.
	/// </summary>
	/// <seealso cref="Rezolver.Compilation.Expressions.ExpressionBuilderBase{Rezolver.ExpressionTarget}" />
	public class ExpressionTargetBuilder : ExpressionBuilderBase<ExpressionTarget>
	{
		internal static readonly MethodInfo[] RezolveMethods =
		{
			MethodCallExtractor.ExtractCalledMethod(() => Functions.Resolve<int>()).GetGenericMethodDefinition()
		};

		/// <summary>
		/// The work horse for the TargetAdapter
		/// </summary>
		/// <seealso cref="System.Linq.Expressions.ExpressionVisitor" />
		private class ExpressionTranslator : ExpressionVisitor
		{
			private readonly IExpressionCompileContext _context;

			public ExpressionTranslator(IExpressionCompileContext context)
			{
				_context = context;
			}

			internal Type ExtractRezolveCallType(Expression e)
			{
				var methodExpr = e as MethodCallExpression;

				if (methodExpr == null || !methodExpr.Method.IsGenericMethod)
					return null;

				var match = RezolveMethods.SingleOrDefault(m => m.Equals(methodExpr.Method.GetGenericMethodDefinition()));

				if (match == null)
					return null;

				return methodExpr.Method.GetGenericArguments()[0];
			}

			protected override Expression VisitConstant(ConstantExpression node)
			{
				return new TargetExpression(new ObjectTarget(node.Value, node.Type));
			}

			protected override Expression VisitNew(NewExpression node)
			{
				return new TargetExpression(ConstructorTarget.FromNewExpression(node.Type, node));
			}

			protected override Expression VisitMemberInit(MemberInitExpression node)
			{
				//var constructorTarget = ConstructorTarget.FromNewExpression(node.Type, node.NewExpression, _adapter);
				return new TargetExpression(new ExpressionTarget(c =>
				{
					var adaptedCtorExp = Visit(node.NewExpression);
					//var ctorTargetExpr = constructorTarget.CreateExpression(c.New(node.Type));

					//the goal here, then, is to find the new expression for this type and replace it 
					//with a memberinit equivalent to the one we visited.  Although the constructor target produces 
					//a NewExpression, it isn't going to be the root expression, because of the scoping boilerplate 
					//that is put around nearly all expressions produced by RezolveTargetBase implementations. 
					var rewriter = new NewExpressionMemberInitRewriter(node.Type, node.Bindings.Select(mb => VisitMemberBinding(mb)));
					return rewriter.Visit(adaptedCtorExp);
				}, node.Type));
			}

			protected override Expression VisitLambda<T>(Expression<T> node)
			{
				Expression body = node.Body;
				try
				{
					ParameterExpression rezolveContextParam = node.Parameters.SingleOrDefault(p => p.Type == typeof(ResolveContext));
					//if the lambda had a parameter of the type ResolveContext, swap it for the 
					//RezolveContextParameterExpression parameter expression that all the internal
					//components use when building expression trees from targets.
					if (rezolveContextParam != null && rezolveContextParam != _context.ResolveContextExpression)
						body = new ExpressionSwitcher(new[] {
							new ExpressionReplacement(rezolveContextParam, _context.ResolveContextExpression)
						}).Visit(body);
				}
				catch (InvalidOperationException ioex)
				{
					//throw by the SingleOrDefault call inside the Try.
					throw new ArgumentException($"The lambda expression { node } is not supported - it has multiple ResolveContext parameters, and only a maximum of one is allowed", nameof(node), ioex);
				}
				var variables = node.Parameters.Where(p => p.Type != typeof(ResolveContext)).ToArray();
				//if we have lambda parameters which need to be converted to block variables which are resolved
				//by assignment (dynamic service location I suppose you'd call it) then we need to wrap everything
				//in a block expression.
				if (variables.Length != 0)
				{
					return Expression.Block(node.Body.Type,
						//all parameters from the Lambda, except one typed as ResolveContext, are fed into the new block as variables
						variables,
						//start the block with a run of assignments for all the parameters of the original lambda
						//with services resolved from the container
						variables.Select(p => Expression.Assign(p, new TargetExpression(new RezolvedTarget(p.Type)))).Concat(
							new[] {
								//and then concatenate the original body of the Lambda, which might have had
								//any references to a ResolveContext parameter switched for the global RezolveContextParameterExpression
								base.Visit(body)
							}
						)
					);
				}
				else
					return base.Visit(body);
			}

			protected override Expression VisitMethodCall(MethodCallExpression node)
			{
				var rezolvedType = ExtractRezolveCallType(node);
				if (rezolvedType != null)
					return new TargetExpression(new RezolvedTarget(rezolvedType));
				return base.VisitMethodCall(node);
			}
		}

		protected override Expression Build(ExpressionTarget target, IExpressionCompileContext context, IExpressionCompiler compiler)
		{
			//reasonably simple - get the underlying expression, push it through the ExpressionTranslator to perform any parameter augmentations
			//or conversion to other targets (like RezolvedTarget, CconstructorTarget etc) and then push the result through a 
			//TargetExpressionRewriter to compile any newly created targets into their respective expressions and into the resulting
			//expression.

			var translator = new ExpressionTranslator(context);
			var translated = translator.Visit(target.ExpressionFactory != null ? target.ExpressionFactory(context) : target.Expression);
			//the translator does lots of things - including identifying common code constructs which have rich target equivalents - such as
			//the NewExpression being the same as the ConstructorTarget.  When it creates a target in place of an expression, it wrap it
			//inside a TargetExpression - so these then have to be compiled again via the TargetExpressionRewriter.
			var targetRewriter = new TargetExpressionRewriter(compiler, context);
			return targetRewriter.Visit(translated);
		}
	}
}