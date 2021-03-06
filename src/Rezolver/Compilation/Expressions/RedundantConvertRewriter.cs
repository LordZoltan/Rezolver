﻿// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System.Linq.Expressions;

namespace Rezolver.Compilation.Expressions
{
    /// <summary>
    /// Removes unnecessary convert expressions from an expression.
    ///
    /// An unnecessary conversion is one where the target type is equal to, or a base of, the source type.
    ///
    /// Only boxing/unboxing conversions or upcasts are left intact.
    /// </summary>
    /// <seealso cref="System.Linq.Expressions.ExpressionVisitor" />
    public class RedundantConvertRewriter : ExpressionVisitor
    {
        /// <summary>
        /// Visits the children of the <see cref="T:System.Linq.Expressions.UnaryExpression" />.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        protected override Expression VisitUnary(UnaryExpression node)
        {
            if (node.NodeType == ExpressionType.Convert &&
              node.Type == node.Operand.Type ||
              (!node.Operand.Type.IsValueType && node.Type.IsAssignableFrom(node.Operand.Type)))
            {
                return node.Operand;
            }

            return base.VisitUnary(node);
        }
    }
}
