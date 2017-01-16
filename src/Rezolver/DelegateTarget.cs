﻿// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Rezolver
{
	/// <summary>
	/// An <see cref="ITarget" /> which resolve objects by executing a delegate with argument injection.
	/// </summary>
	/// <remarks>The delegate must be non-void and can have any number of parameters.
	/// 
	/// Any parameters will be automatically resolved from the container, and a parameter
	/// of the type <see cref="ResolveContext"/> will receive the context passed to the
	/// current <see cref="IContainer.Resolve(ResolveContext)"/>.</remarks>
 	public class DelegateTarget : TargetBase
	{
		/// <summary>
		/// Gets the factory method that will be invoked by an expression built by this target.
		/// </summary>
		/// <value>The factory.</value>
		public Delegate Factory { get; private set; }
		private MethodInfo _factoryMethod;
		private Type _declaredType;

		/// <summary>
		/// Gets the declared type of object that is constructed by this target.
		/// </summary>
		public override Type DeclaredType
		{
			get
			{
				return _declaredType ?? _factoryMethod.ReturnType;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DelegateTarget"/> class.
		/// </summary>
		/// <param name="factory">Required - the factory delegate.  Must have a return type and can take 
		/// 0 or more parameters.  As described in the remarks section of this class, parameters will be 
		/// automatically resolved from the container; except parameters of the type <see cref="ResolveContext"/>,
		/// which will receive the context that was passed to the current <see cref="IContainer.Resolve(ResolveContext)"/> 
		/// method.</param>
		/// <param name="declaredType">Optional - type that will be set into the <see cref="DeclaredType"/> for the target;
		/// if not provided, then it will be derived from the <paramref name="factory"/>'s return type</param>
		/// <exception cref="ArgumentNullException">If <paramref name="factory"/> is null</exception>
		/// <exception cref="ArgumentException">If the <paramref name="factory"/> represents a void delegate or if
		/// <paramref name="declaredType"/> is passed but the type is not compatible with the return type of
		/// <paramref name="factory"/>.</exception>
		public DelegateTarget(Delegate factory, Type declaredType = null)
		{
			factory.MustNotBeNull(nameof(factory));
			_factoryMethod = factory.GetMethodInfo();
			_factoryMethod.MustNot(m => _factoryMethod.ReturnType == null || _factoryMethod.ReturnType == typeof(void), "Factory must have a return type", nameof(factory));

			if (declaredType != null)
			{
				if (!TypeHelpers.AreCompatible(_factoryMethod.ReturnType, declaredType) && !TypeHelpers.AreCompatible(declaredType, _factoryMethod.ReturnType))
					throw new ArgumentException(string.Format(ExceptionResources.DeclaredTypeIsNotCompatible_Format, declaredType, _factoryMethod.ReturnType), nameof(declaredType));
			}
			_declaredType = declaredType;
			Factory = factory;
		}
	}

	/// <summary>
	/// Extension methods for the <see cref="Delegate"/> type to aid in the construction of <see cref="DelegateTarget"/>.
	/// </summary>
	public static class DelegateTargetDelegateExtensions
	{
		/// <summary>
		/// Creates a <see cref="DelegateTarget"/> from the <paramref name="factory"/> which can be registered in an 
		/// <see cref="ITargetContainer"/> to resolve an instance of a type compatible with the delegate's return type
		/// an, optionally, with the <paramref name="declaredType" />
		/// </summary>
		/// <param name="factory">The delegate to be used as a factory.</param>
		/// <param name="declaredType">Optional type to set as the <see cref="DelegateTarget.DeclaredType"/> of the target,
		/// if not passed, then the return type of the delegate will be used.</param>
		public static DelegateTarget AsDelegateTarget(this Delegate factory, Type declaredType = null)
		{
			return new DelegateTarget(factory, declaredType);
		}
	}
}
