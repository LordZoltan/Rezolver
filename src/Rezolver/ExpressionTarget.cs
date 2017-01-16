// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Rezolver.Compilation;

namespace Rezolver
{
	/// <summary>
	/// A generic target for all expressions not explicitly supported by a particular target.
	/// 
	/// Enables more complex behaviours to be registered and used with the more formal
	/// <see cref="ITarget"/> implementations.
	/// </summary>
	/// <remarks>Note that this target does not support Lambda expressions.  If you wish to create
	/// an <see cref="ITarget"/> from a lambda expression, then you should use the </remarks>
	public class ExpressionTarget : TargetBase
	{
		/// <summary>
		/// Gets the static expression represented by this target - if <c>null</c>, then 
		/// a factory is being used to produce the expression, which is available from
		/// the <see cref="ExpressionFactory"/> property.
		/// </summary>
		public Expression Expression { get; }

		/// <summary>
		/// Gets the type of <see cref="Expression"/> or the type that all expressions returned by the 
		/// <see cref="ExpressionFactory"/> are expected to be equal to.
		/// </summary>
		public override Type DeclaredType
		{
			get;
		}

		/// <summary>
		/// Gets a factory which will be executed to obtain an expression given a particular <see cref="ICompileContext"/>.
		/// 
		/// If <c>null</c>, then a static expression will be used instead and is available
		/// from the <see cref="Expression"/> property.
		/// </summary>
		public Func<ICompileContext, Expression> ExpressionFactory { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="ExpressionTarget" /> class.
		/// </summary>
		/// <param name="expression">Required. The static expression which should be used by compilers.</param>
		/// <param name="declaredType">Declared type of the target to be created (used when registering without
		/// an explicit type or when this target is used as a value inside another target).</param>
		/// <remarks><paramref name="declaredType"/> will automatically be determined if not provided
		/// by examining the type of the <paramref name="expression"/>.  For lambdas, the type will
		/// be derived from the Type of the lambda's body.  For all other expressions, the type is
		/// taken directly from the Type property of the expression itself.</remarks>
		public ExpressionTarget(Expression expression, Type declaredType = null)
		{
			expression.MustNotBeNull(nameof(expression));
			expression.MustNot(e => e.NodeType == ExpressionType.Lambda, "Lambda expressions are not directly supported by the ExpressionTarget class.  Please use an ITargetAdapter to create a target from a Lambda Expression", nameof(expression));
			Expression = expression;
			DeclaredType = declaredType ?? expression.Type;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ExpressionTarget"/> class.
		/// </summary>
		/// <param name="expressionFactory">Required. The factory delegate that a compiler should call to get the expression to use when 
		/// compiling this target.</param>
		/// <param name="declaredType">Required. Static type of all expressions that will be
		/// returned by <paramref name="expressionFactory"/>.</param>
		public ExpressionTarget(Func<ICompileContext, Expression> expressionFactory, Type declaredType)
		{
			expressionFactory.MustNotBeNull(nameof(expressionFactory));
			declaredType.MustNotBeNull(nameof(declaredType));
			ExpressionFactory = expressionFactory;
			DeclaredType = declaredType;
		}
	}
}