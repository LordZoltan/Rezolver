﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Rezolver.Resources;

namespace Rezolver
{
	/// <summary>
	/// Abstract base class, suggested as a starting point for implementations of IRezolveTarget.
	/// </summary>
	public abstract class RezolveTargetBase : IRezolveTarget
	{
		private class RezolveTargetExpressionRewriter : ExpressionVisitor
		{
			readonly CompileContext _sourceCompileContext;

			public RezolveTargetExpressionRewriter(CompileContext context)
			{
				_sourceCompileContext = context;
			}
			public override Expression Visit(Expression node)
			{
				if (node != null)
				{
					if (node.NodeType == ExpressionType.Extension)
					{
						RezolveTargetExpression re = node as RezolveTargetExpression;
						if (re != null)
						{
							return re.Target.CreateExpression(new CompileContext(_sourceCompileContext, re.Type, true));
						}
						RezolveContextPlaceholderExpression pe = node as RezolveContextPlaceholderExpression;
						if (pe != null)
							return _sourceCompileContext.RezolveContextParameter;
					}
				}
				return base.Visit(node);
			}
		}

		/// <summary>
		/// Abstract method called to create the expression - this is called by <see cref="CreateExpression"/> after the
		/// target type has been validated, if provided.
		/// 
		/// Note - if your implementation needs to support dynamic Resolve operations from the rezolver that is passed
		/// to an IRezolver's Resolve method, you can use the <see cref="ExpressionHelper.DynamicRezolverParam"/> property,
		/// all the default implementations of this class (and others) use that by default.
		/// </summary>
		/// <param name="context">The current compile context</param>
		/// <returns></returns>
		protected abstract Expression CreateExpressionBase(CompileContext context);

		/// <summary>
		/// Called to check whether a target can create an expression that builds an instance of the given <paramref name="type" />.
		/// </summary>
		/// <param name="type">Required</param>
		/// <returns><c>true</c> if this target supports the given type, <c>false</c> otherwise.</returns>
		public virtual bool SupportsType(Type type)
		{
			type.MustNotBeNull("type");
			return TypeHelpers.AreCompatible(DeclaredType, type);
		}

		/// <summary>
		/// Virtual method implementing IRezolveTarget.CreateExpression.  Rather than overriding this method,
		/// your starting point is to implement the abstract method <see cref="CreateExpressionBase"/>.
		/// </summary>
		/// <param name="context">The current compile context</param>
		/// <returns></returns>
		public virtual Expression CreateExpression(CompileContext context)
		{
			if (context.TargetType != null && !SupportsType(context.TargetType))
				throw new ArgumentException(String.Format(Exceptions.TargetDoesntSupportType_Format, context.TargetType),
					"targetType");

			if (!context.PushCompileStack(this))
				throw new InvalidOperationException(string.Format(Exceptions.CyclicDependencyDetectedInTargetFormat, GetType(), DeclaredType));

			try
			{
				var result = CreateExpressionBase(context);
				Type convertType = context.TargetType ?? DeclaredType;

				if (convertType == typeof(object) && TypeHelpers.IsValueType(result.Type)
					|| !convertType.IsAssignableFrom(DeclaredType)
					|| !convertType.IsAssignableFrom(result.Type))
					return Expression.Convert(result, convertType);

				//now have to rewrite any RezolveTargetExpression objects that are in the tree
				return new RezolveTargetExpressionRewriter(context).Visit(result);

				//return result;
			}
			finally
			{
				context.PopCompileStack();
			}
		}

		/// <summary>
		/// Gets the declared type of object that is constructed by this target.  This will be the return type of
		/// any expression built by <see cref="CreateExpression"/> unless otherwise instructed to build a different type.
		/// </summary>
		public abstract Type DeclaredType
		{
			get;
		}
	}
}