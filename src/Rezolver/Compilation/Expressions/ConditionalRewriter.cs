﻿// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Rezolver.Compilation.Expressions
{
    /// <summary>
    /// Reorders an expression tree where duplicate conditional expressions are found
    /// in multiple places throughout that expression tree. Those duplicated conditionals
    /// are moved further up the expression tree into one conditional.
    /// </summary>
    /// <remarks>Although there's no reason for this type to be internal, it is for now until someone specifically needs it.
    ///
    /// The library already uses it automatically for all expressions created by custom targets so long as the standard base types
    /// are used to create and compile them.
    ///
    /// In essence, what we're doing is taking an expression that's been generated as something like this:
    ///
    /// <code>return a == b ? new c(a == b ? new d() : new e()) : new f(a == b ? new d() : new e())</code>
    ///
    /// And rewriting it to be:
    ///
    /// <code>return a == b ? new c(new d()) : new f(new e())</code>
    ///
    /// So, it's looking for one or more duplicate conditionals which appear inside the true or false branches of
    /// the same conditional, and removing all the redundant checks - reducing the whole thing to one conditional.
    ///
    /// In the example above, we only create an instance of c when a == b, therefore we will only ever create an
    /// instance of d as its constructor argument - so the second a == b conditional is not required.  When a != b,
    /// similarly, we will only ever create an instance of e as the constructor argument.
    ///
    /// As you can see this can be a significant optimisation to the expressions that are generated by the
    /// <see cref="ExpressionCompiler"/>, since each target's expression is built in isolation and
    /// the existence of previous identical conditionals is not considered.  The optimisation only works when the
    /// equality expression used in all identical conditionals is exactly the same expression - therefore the
    /// shared expressions functionality of the <see cref="IExpressionCompileContext"/> is absolutely crucial to
    /// achieving a good result.
    /// </remarks>
    internal class ConditionalRewriter : ExpressionVisitor
    {
        private enum RewriteStages
        {
            NotRun,
            GatheringConditionals,
            RewritingConditionals
        }

        private enum ConditionalRewritePart
        {
            TruePart,
            FalsePart
        }

        private class ConditionalExpressionInfo : IEquatable<ConditionalExpressionInfo>
        {
            public ConditionalExpression Expression;
            public Stack<Expression> Hierarchy;
            public CandidateTest Test;

            public ConditionalExpressionInfo(ConditionalExpression node, Stack<Expression> stack, CandidateTest matchingTest)
            {
                this.Expression = node;
                this.Hierarchy = stack;
                this.Test = matchingTest;
                this.Test.MatchCount++;
            }

            // equality is exceptionally simple - only performs reference equality check.
            // this works only because the specific scenario that this rewriter is designed to
            // handle relates to the ResolvedTarget - which uses shared expressions.
            public bool Equals(ConditionalExpressionInfo other)
            {
                return object.ReferenceEquals(this.Test, other.Test);
            }

            public override bool Equals(object obj)
            {
                return this.Equals(obj as ConditionalExpressionInfo);
            }

            public override int GetHashCode()
            {
                return this.Test.GetHashCode();
            }
        }

        private class ConditionalExpressionGroupInfo
        {
            public IGrouping<ConditionalExpressionInfo, ConditionalExpressionInfo> Group;
            public Expression CommonParent;
        }

        private class ConditionalExpressionRewriteState
        {
            public ConditionalExpressionGroupInfo GroupInfo;
            public ConditionalRewritePart Mode;
        }

        private class CandidateTest
        {
            /// <summary>
            /// The shared test
            /// </summary>
            public Expression Test = null;
            /// <summary>
            /// The number of times a conditional expression with this test has been rewritten
            /// </summary>
            public int RewriteCount = 0;
            /// <summary>
            /// The number of times this test has been discovered in a conditional expression
            /// </summary>
            public int MatchCount = 0;
        }

        private Expression _expression;
        private CandidateTest[] _candidateTests;
        private Stack<Expression> _currentStack = new Stack<Expression>();
        private List<ConditionalExpressionInfo> _allConditionals = new List<ConditionalExpressionInfo>();
        private ConditionalExpressionGroupInfo[] _groupedConditionals = null;
        private Stack<ConditionalExpressionRewriteState> _currentlyRewriting = new Stack<ConditionalExpressionRewriteState>();
        private RewriteStages _currentStage = RewriteStages.NotRun;

        public ConditionalRewriter(Expression expression, IEnumerable<Expression> candidateTests)
        {
            this._expression = expression;
            this._candidateTests = candidateTests.Select(c => new CandidateTest() { Test = c }).ToArray();
        }

        public Expression Rewrite()
        {
            // keep rewriting until we've got no more rewrites left to do - one pass
            // only rewrites one conditional most of the time because of the way that
            // unoptimised expressions branch.
            Expression result = this._expression;
            int maxLoops = this._candidateTests.Length;
            int currentLoop = 0;
            while (currentLoop++ < maxLoops)
            {
                this._currentStage = RewriteStages.GatheringConditionals;
                this._allConditionals.Clear();
                result = this.Visit(result);
                // on the first loop, adjust the maxLoops to the number of
                // candidate tests which are actually present in the expression
                if (currentLoop == 1)
                {
                    maxLoops = this._candidateTests.Count(c => c.MatchCount != 0);
                    // if none of our tests were found, then break immediately as there's
                    // no work to do.
                    if (maxLoops == 0)
                    {
                        break;
                    }
                }

                // allConditionals only contains conditionals with tests that haven't already been rewritten,
                // since we should only need to rewrite our conditionals once.
                if (this._allConditionals.Count <= 1)
                {
                    break;
                }

                var grouped = this._allConditionals.GroupBy(i => i).ToArray();

                if (grouped.Length == this._allConditionals.Count)
                {
                    break;
                }

                this._currentStage = RewriteStages.RewritingConditionals;

                // now we have to find whether this group has a shared parent expression that we can rewrite
                // this involves finding the bottom-most expression which is present in all the stacks that
                // we gathered.  We won't remove that expression, we're just going to push it down and clone it
                // as a new pair of iffalse and iftrue branches in a new conditional expression.
                this._groupedConditionals = (from grp in grouped
                                        let grpArray = grp.ToArray()
                                        where grpArray.Length > 1
                                        let commonParent = (from expr in grpArray[0].Hierarchy
                                                            let otherHierarchies = (from conditional2 in grp.Skip(1)
                                                                                    select conditional2.Hierarchy)
                                                            where otherHierarchies.All(h => h.Any(expr2 => object.ReferenceEquals(expr, expr2)))
                                                            select expr).LastOrDefault()
                                        where commonParent != null
                                        select new ConditionalExpressionGroupInfo { Group = grp, CommonParent = commonParent }).ToArray();

                result = this.Visit(result);
            }
;

            return result;
        }

        public override Expression Visit(Expression node)
        {
            if (this._currentStage == RewriteStages.GatheringConditionals)
            {
                this._currentStack.Push(node);
                var result = base.Visit(node);
                this._currentStack.Pop();
                return result;
            }
            else if (this._currentStage == RewriteStages.RewritingConditionals)
            {
                var matchingGroup = this._groupedConditionals.FirstOrDefault(gc => object.ReferenceEquals(node, gc.CommonParent));
                if (matchingGroup != null)
                {
                    var state = new ConditionalExpressionRewriteState() { GroupInfo = matchingGroup, Mode = ConditionalRewritePart.TruePart };
                    // so now we are going to place this expression inside a
                    this._currentlyRewriting.Push(state);
                    var truePart = base.Visit(node);
                    this._currentlyRewriting.Pop();
                    state = new ConditionalExpressionRewriteState() { GroupInfo = matchingGroup, Mode = ConditionalRewritePart.FalsePart };
                    this._currentlyRewriting.Push(state);
                    var falsePart = base.Visit(node);
                    this._currentlyRewriting.Pop();

                    // because of optimisations that we perform elsewhere, we need to check that the true/false parts have compatible types
                    // if not equal, then we inject a conversion to one or the other to ensure the types are equal.
                    if (truePart.Type != falsePart.Type)
                    {
                        // note that this code relies on NULLs (either via DefaultExpression or ConstantExpression) being strongly-typed
                        if (TypeHelpers.IsAssignableFrom(truePart.Type, falsePart.Type))
                        {
                            falsePart = Expression.Convert(falsePart, truePart.Type);
                        }
                        else if (TypeHelpers.IsAssignableFrom(falsePart.Type, truePart.Type))
                        {
                            truePart = Expression.Convert(truePart, falsePart.Type);
                        }
                    }

                    var test = this._candidateTests.SingleOrDefault(ct => ct.Test == matchingGroup.Group.Key.Expression.Test);
                    if (test != null)
                    {
                        test.RewriteCount++;
                    }

                    return Expression.Condition(matchingGroup.Group.Key.Expression.Test, truePart, falsePart);
                }
            }

            return base.Visit(node);
        }

        protected override Expression VisitConditional(ConditionalExpression node)
        {
            if (this._currentStage == RewriteStages.GatheringConditionals)
            {
                // check that this expression is one of those that has not yet been rewritten
                var matchingTest = this._candidateTests.FirstOrDefault(e => e.RewriteCount == 0 && object.ReferenceEquals(node.Test, e.Test));
                if (matchingTest != null)
                {
                    this._allConditionals.Add(new ConditionalExpressionInfo(node, new Stack<Expression>(this._currentStack), matchingTest));
                }

                return base.VisitConditional(node);
            }
            else if (this._currentStage == RewriteStages.RewritingConditionals && this._currentlyRewriting.Count != 0)
            {
                var currentRewrite = this._currentlyRewriting.Peek();
                var matching = currentRewrite.GroupInfo.Group.FirstOrDefault(e => object.ReferenceEquals(e.Expression, node));
                if (matching != null)
                {
                    switch (currentRewrite.Mode)
                    {
                        case ConditionalRewritePart.TruePart:
                            {
                                return this.Visit(matching.Expression.IfTrue);
                            }

                        case ConditionalRewritePart.FalsePart:
                            {
                                return this.Visit(matching.Expression.IfFalse);
                            }
                    }
                }
            }

            return base.VisitConditional(node);
        }

        // protected override Expression VisitLambda<T>(Expression<T> node)
        // {
        // //prevent the rewriter from lifting code out of a lambda
        // //this means that any quoted lambdas must already be optimised.
        // return node;
        // }
    }
}
