using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Rezolver
{
	/// <summary>
	/// Interface for an object that compiles delegates from IRezolveTarget instances.
	/// </summary>
	public interface IRezolverTargetCompiler
	{
		/// <summary>
		/// Compiles a delegate for resolving an object via the given target based on the targets configured
		/// within the passed <paramref name="containerScope"/>.  The delegate will not accept a dynamic container
		/// at call time.
		/// </summary>
		/// <param name="target">The target to be compiled.</param>
		/// <param name="containerScope">The scope within which this target is to be compiled - this will be used
		/// to look up any other targets to be used as dependencies for the statically compiled delegate.</param>
		/// <param name="targetType">Optional - the type of the object that is required - if different from the type
		/// that the target would return by default.  If supplied, it must be compatible with the 
		/// type of the object that is resolved by the target otherwise an exception will occur.</param>
		/// <param name="targetStack">Optional - if this compilation is taking place as part of a wider compilation
		/// then this is used to pass the stack of targets that are already compiling.  Generally you will pass this
		/// as null.</param>
		/// <returns>A delegate that, when executed, returns the object that is resolved by the target.</returns>
		Func<object> CompileStatic(IRezolveTarget target, IRezolverContainer containerScope, Type targetType = null,
			Stack<IRezolveTarget> targetStack = null);

		/// <summary>
		/// Compiles a strongly typed delegate for resolving an object via the given target based on the targets configured 
		/// within the passed <paramref name="containerScope"/>.  The delegate will not accept a dynamic container
		/// at call time.
		/// </summary>
		/// <typeparam name="TTarget">The return type of the delegate to create.  It must be compatible with the type
		/// of the object that is resolved by the target otherwise an exception will occur.</typeparam>
		/// <param name="target">The target to be compiled.</param>
		/// <param name="containerScope">The scope within which this target is to be compiled - this will be used
		/// to look up any other targets to be used as dependencies for the statically compiled delegate.</param>
		/// <param name="targetStack">Optional - if this compilation is taking place as part of a wider compilation
		/// then this is used to pass the stack of targets that are already compiling.  Generally you will pass this
		/// as null.</param>
		/// <returns>A strongly typed delegate that, when executed, returns the object that is resolved by the target.</returns>
		Func<TTarget> CompileStatic<TTarget>(IRezolveTarget target, IRezolverContainer containerScope, Stack<IRezolveTarget> targetStack = null);

		/// <summary>
		/// Compiles a delegate for resolving an object via the given target based either on the targets configured within
		/// the passed <paramref name="containerScope"/> at compile time or a dynamic container passed at call time.
		/// </summary>
		/// <param name="target">The target to be compiled.</param>
		/// <param name="containerScope">The scope within which this target is to be compiled - this will be used
		/// to look up any other targets to be used as dependencies for the statically compiled delegate.</param>
		/// <param name="dynamicContainerExpression">A ParameterExpression to be used as the parameter that
		/// will receive the optional dynamic container when the delegate is called.</param>
		/// <param name="targetType">Optional - the type of the object that is required - if different from the type
		/// that the target would return by default.  If supplied, it must be compatible with the 
		/// type of the object that is resolved by the target otherwise an exception will occur.</param>
		/// <param name="targetStack">Optional - if this compilation is taking place as part of a wider compilation
		/// then this is used to pass the stack of targets that are already compiling.  Generally you will pass this
		/// as null.</param>
		/// <returns>A delegate that, when executed, returns an object from the container provided at
		/// call time or the object that the target that would normally return - equally, the dynamic container
		/// can be used to resolve further dependencies.</returns>
		Func<IRezolverContainer, object> CompileDynamic(IRezolveTarget target, IRezolverContainer containerScope, 
			ParameterExpression dynamicContainerExpression, Type targetType = null, Stack<IRezolveTarget> targetStack = null);

		/// <summary>
		/// Compiles a strongly typed delegate for resolving an object via the given target based either on the targets configured
		/// within the passed <paramref name="containerScope"/> at compile time or a dynamic container passed at call time.
		/// </summary>
		/// <typeparam name="TTarget">The return type of the delegate to create.  It must be compatible with the type
		/// of the object that is resolved by the target otherwise an exception will occur.</typeparam>
		/// <param name="target">The target to be compiled.</param>
		/// <param name="containerScope">The scope within which this target is to be compiled - this will be used
		/// to look up any other targets to be used as dependencies for the statically compiled delegate.</param>
		/// <param name="dynamicContainerExpression">A ParameterExpression to be used as the parameter that
		/// will receive the optional dynamic container when the delegate is called.</param>
		/// <param name="targetStack">Optional - if this compilation is taking place as part of a wider compilation
		/// then this is used to pass the stack of targets that are already compiling.  Generally you will pass this
		/// as null.</param>
		/// <returns>A strongly-typed delegate that, when executed, returns an object from the container provided at
		/// call time or the object that the target that would normally return - equally, the dynamic container
		/// can be used to resolve further dependencies.</returns>
		Func<IRezolverContainer, TTarget> CompileDynamic<TTarget>(IRezolveTarget target, IRezolverContainer containerScope,
			ParameterExpression dynamicContainerExpression, Stack<IRezolveTarget> targetStack = null);


	}
}