using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Versioning;
using System.Xml.Serialization;
using Rezolver.Resources;

namespace Rezolver
{
	/// <summary>
	/// Represents a target that is rezolved during expression building and/or at rezolve time.
	/// 
	/// That is, a target is located from the rezolver that is supplied to the CreateExpression method,
	/// and that target is then used to donate the expression.
	/// </summary>
	public class RezolvedTarget : RezolveTargetBase
	{
		private readonly Type _resolveType;
		private readonly IRezolveTarget _resolveNameTarget;

		private static readonly MethodInfo RezolverResolveMethod =
			MethodCallExtractor.ExtractCalledMethod((IRezolver c) => c.Resolve(typeof (object), null, null));

		public IRezolveTarget Name { get { return _resolveNameTarget; } }

		internal RezolvedTarget(RezolveTargetAdapter.RezolveCallExpressionInfo rezolveCall)
		{
			_resolveType = rezolveCall.Type;
			_resolveNameTarget = rezolveCall.Name;
		}

		public RezolvedTarget(Type type, string name = null)
			: this(type, name != null ? name.AsObjectTarget() : null)
		{

		}

		public RezolvedTarget(Type type, IRezolveTarget name)
		{
			type.MustNotBeNull("type");
			_resolveType = type;
			_resolveNameTarget = name;
		}

		public override Type DeclaredType
		{
			get { return _resolveType; }
		}

		protected override Expression CreateExpressionBase(IRezolver rezolver, Type targetType = null, ParameterExpression dynamicRezolverExpression = null, Stack<IRezolveTarget> currentTargets = null)
		{
			rezolver.MustNotBeNull("rezolver");

			if (dynamicRezolverExpression != null)
			{
				Func<object> compiledRezolveCall = null;
				ICompiledRezolveTarget compiledNameCall = null;

				//TODO: reuse the passed rezolver's compiler.  Or, could we even re use the rezolver to get the compiled target?

				if (_resolveNameTarget != null)
				{
					//I think in this case, we *have* to defer to a dynamic resolve call on the rezolver in addition to 
					//intrinsic dynamic rezolver because we can't know if the name target reprents a single value, or something which
					//produces lots of different values based on ambient environments.
					//There is the minority case for ObjectTarget and probably SingletonTarget,  which will always produce the 
					//same instance, but there's no reliable way - apart from a type test - to determine that.
					//TODO: make this fetch a compiled target from the rezolver instead of compiling a delegate
					compiledNameCall = rezolver.Compiler.CompileTarget(_resolveNameTarget, rezolver, dynamicRezolverExpression,
						currentTargets);
				}

				//TODO: after changing above to get compiled target, simply invoke it's GetObject method to pass the name
				var resolvedTarget = rezolver.Fetch(DeclaredType, compiledNameCall != null ? (string)compiledNameCall.GetObject() : null);

				if (resolvedTarget != null)
				{
					var toCall = ExpressionHelper.GetFactoryForTarget(rezolver, targetType, resolvedTarget, currentTargets);
					//do not pass a dynamic rezolver to the factory - because this particular expression is only intended
					//to work on this Builder, not the dynaamic one.
					compiledRezolveCall = () => toCall(null);
				}

				//TODO: Look over the rest of this dynamic rezolver implementation see about not using delegates.
				//only one way to do this - do all the checks now so that minimal decisions are made when the resolve operation
				//is called.
				Func<IRezolver, object> lateBoundFounc;
				var finalType = targetType ?? DeclaredType;
				if (compiledNameCall != null)
				{
					if (compiledRezolveCall != null)
					{
						lateBoundFounc = (dynamicRezolver) =>
						{
							if (dynamicRezolver != null)
							{
								var name = (string)compiledNameCall.GetObjectDynamic(dynamicRezolver);
								if (dynamicRezolver.CanResolve(finalType, name))
								{
									return dynamicRezolver.Resolve(finalType, name);
								}
							}
							return compiledRezolveCall();
						};
					}
					else
					{
						//same as above, but an exception is thrown if the dynamic Builder can't resolve
						lateBoundFounc = (dynamicRezolver) =>
						{
							if (dynamicRezolver != null)
							{
								var name = (string)compiledNameCall.GetObjectDynamic(dynamicRezolver);
								if (dynamicRezolver.CanResolve(finalType, name))
								{
									return dynamicRezolver.Resolve(finalType, name);
								}
							}
							throw new InvalidOperationException(string.Format(Exceptions.UnableToResolveTypeFromBuilderFormat, finalType));
						};
					}
				}
				else
				{
					if (compiledRezolveCall != null)
					{

						lateBoundFounc = (dynamicRezolver) =>
						{
							if (dynamicRezolver != null)
							{
								if (dynamicRezolver.CanResolve(finalType, null))
								{
									return dynamicRezolver.Resolve(finalType, null);
								}
							}

							return compiledRezolveCall();
						};
					}
					else
					{
						lateBoundFounc =
							(dynamicRezolver) =>
							{
								if (dynamicRezolver != null)
								{
									if (dynamicRezolver.CanResolve(finalType, null))
									{
										return dynamicRezolver.Resolve(finalType, null);
									}
								}

								throw new InvalidOperationException(string.Format(Exceptions.UnableToResolveTypeFromBuilderFormat, finalType));
							};
					}
				}

				return
					Expression.Call(Expression.Constant(lateBoundFounc), lateBoundFounc.GetType().GetMethod("Invoke"),
						dynamicRezolverExpression);
			}
			else
			{
				//TODO: stop using the compiler below - move to getting the compiled resolve target from the rezolver and executing it.
				string name = _resolveNameTarget != null
					? (string)rezolver.Compiler.CompileTarget(_resolveNameTarget, rezolver,null,currentTargets).GetObject()
					: null;
				var resolvedTarget = rezolver.Fetch(_resolveType, name);
				if (resolvedTarget == null)
					//when null, we simply emit a call back into the rezolver to be executed at runtime which should throw an exception
					return
						Expression.Convert(
							Expression.Call(Expression.Constant(rezolver, typeof (IRezolver)), RezolverResolveMethod,
								new Expression[] { Expression.Constant(_resolveType, typeof(Type)), Expression.Constant(name, typeof(string)), Expression.Constant(null, typeof(IRezolver)) }), targetType ?? DeclaredType);

					//throw new InvalidOperationException(string.Format(Exceptions.UnableToResolveTypeFromScopeFormat, _resolveType));
				return resolvedTarget.CreateExpression(rezolver, targetType: targetType, currentTargets: currentTargets);
			}
		}
	}
}