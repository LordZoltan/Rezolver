﻿// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Rezolver.Targets
{
    public partial class ConstructorTarget
    {
		private static readonly IDictionary<string, ITarget> _emptyArgsDictionary = new Dictionary<string, ITarget>();

        /// <summary>
        /// Generic version of the <see cref="Auto(Type, IMemberBindingBehaviour)"/> method.
        /// </summary>
        /// <typeparam name="T">The type that is to be constructed when the new target is compiled and executed.</typeparam>
        /// <param name="memberBindingBehaviour">See the documentation for the <paramref name="memberBindingBehaviour"/> parameter
        /// on the non-generic version of this method.</param>
        /// <returns>Either a <see cref="ConstructorTarget"/> or <see cref="GenericConstructorTarget"/>, depending on whether
        /// <typeparamref name="T"/> is a generic type definition.</returns>
        [Obsolete("This method has been replaced by the Target.ForType overloaded method and will be removed in 1.2")]
        public static ITarget Auto<T>(IMemberBindingBehaviour memberBindingBehaviour = null)
		{
            return Target.ForType<T>(memberBinding: memberBindingBehaviour);
		}

		/// <summary>
		/// Creates a late bound <see cref="ConstructorTarget"/> or <see cref="GenericConstructorTarget"/> for the 
		/// given <paramref name="type"/>.
		/// </summary>
		/// <param name="type">The type that is to be constructed when this target is compiled and executed.</param>
		/// <param name="memberBindingBehaviour">Optional.  An object which selects properties on the new instance which are
		/// to be bound from the container.</param>
		/// <returns>Either a <see cref="ConstructorTarget"/> or <see cref="GenericConstructorTarget"/>, depending on whether the 
		/// <paramref name="type"/> is a generic type definition.</returns>
		/// <remarks>This factory is merely a shortcut for calling the <see cref="ConstructorTarget.ConstructorTarget(Type, IDictionary{string, ITarget}, IMemberBindingBehaviour)"/>
		/// with only the <paramref name="type"/> and <paramref name="memberBindingBehaviour"/> arguments supplied.  When creating a 
		/// <see cref="GenericConstructorTarget"/>, the function uses the <see cref="GenericConstructorTarget.GenericConstructorTarget(Type, IMemberBindingBehaviour)"/> constructor.</remarks>
        [Obsolete("This method has been replaced by the Target.ForType overloaded method and will be removed in 1.2")]
		public static ITarget Auto(Type type, IMemberBindingBehaviour memberBindingBehaviour = null)
		{
            return Target.ForType(type, memberBinding: memberBindingBehaviour);
		}

        /// <summary>
        /// Non-generic version of <see cref="WithArgs{T}(IDictionary{string, ITarget})"/>.
        /// Creates a <see cref="ConstructorTarget"/> with a set of named targets which will be used like
        /// named arguments to late-bind the constructor when code-generation occurs.
        /// </summary>
        /// <param name="declaredType">The type whose constructor is to be bound.</param>
        /// <param name="namedArgs">The named arguments to be used when building the expression.</param>
        /// <remarks>Both versions of this method will create a target which will try to find the best-matching
        /// constructor where all of the named arguments match, and with the fewest number of auto-resolved
        /// arguments.
        /// 
        /// So, a class with a constructor such as 
        /// 
        /// <code>Foo(IService1 s1, IService2 s2)</code>
        /// 
        /// Can happily be bound if you only provide a named argument for 's1'; the target will simply
        /// attempt to auto-resolve the argument for the <code>IService2 s2</code> parameter when constructing 
        /// the object - and will fail only if it can't be resolved at that point.
        /// </remarks>
        [Obsolete("This method has been replaced by the Target.ForType overloaded method and will be removed in 1.2")]
        public static ITarget WithArgs(Type declaredType, IDictionary<string, ITarget> namedArgs)
		{
            return Target.ForType(declaredType, namedArgs);
		}

        /// <summary>
        /// Performs the same operation as <see cref="WithArgs(Type, IDictionary{string, ITarget})"/> except the
        /// arguments are pulled from the publicly readable properties and fields of the passed <paramref name="namedArgs"/>
        /// object.
        /// </summary>
        /// <param name="declaredType">The type whose constructor is to be bound.</param>
        /// <param name="namedArgs">An object whose properties/fields provide the names and values for the argument
        /// which are to be used when binding the type's constructor.  Only properties and fields whose value is
        /// <see cref="ITarget"/> are considered.</param>
        /// <remarks>This overload exists to simplify the process of creating a ConstructorTarget with argument bindings
        /// by removing the need to create an argument dictionary in advance.  An anonymous type can instead be used
        /// to supply the arguments.</remarks>
        /// <example>This example shows how to provide an ObjectTarget for the parameter 'param1' when creating a
        /// ConstructorTarget for the type 'MyType':
        /// <code>ConstructorTarget.WithArgs(typeof(MyType), new { param1 = new ObjectTarget(&quot;Hello World&quot;) });</code></example>
        [Obsolete("This method has been replaced by the Target.ForType overloaded method and will be removed in 1.2")]
        public static ITarget WithArgs(Type declaredType, object namedArgs)
		{
            return Target.ForType(declaredType, namedArgs);
		}

        /// <summary>
        /// Similar to <see cref="WithArgs(Type, IDictionary{string, ITarget})"/> except this one creates
        /// a <see cref="ConstructorTarget"/> that is specifically bound to a particular constructor on a 
        /// given type, using any matched argument bindings from the provided <paramref name="namedArgs" /> dictionary,
        /// and using <see cref="ResolvedTarget"/> targets for any that are not matched.
        /// </summary>
        /// <param name="ctor">Required. The constructor to be bound.</param>
        /// <param name="namedArgs">Optional. Any arguments to be supplied to parameters on the <paramref name="ctor"/>
        /// by name.  Any parameters for which matches are not found in this dictionary will be automatically
        /// bound either from compile-time defaults or by resolving those types dynamically.</param>
        /// <remarks>Although this overload accepts a dictionary of arguments, note that it will not
        /// result in the <see cref="NamedArgs"/> property being set on the target that is created - it's
        /// just an alternative for deriving the <see cref="ParameterBindings"/> with which the target
        /// will be created.
        /// 
        /// Also, this function will not fail if the args dictionary contains named arguments that cannot
        /// be matched to parameters on the <paramref name="ctor"/>.</remarks>
        [Obsolete("This method has been replaced by the Target.ForType overloaded method and will be removed in 1.2")]
        public static ITarget WithArgs(ConstructorInfo ctor, IDictionary<string, ITarget> namedArgs)
		{
			ctor.MustNotBeNull("ctor");
			namedArgs = namedArgs ?? _emptyArgsDictionary;

			ParameterBinding[] bindings = ParameterBinding.BindMethod(ctor, namedArgs);

			return new ConstructorTarget(ctor, bindings, null);
		}

        /// <summary>
        /// Performs the same operation as <see cref="WithArgs(ConstructorInfo, IDictionary{string, ITarget})"/>
        /// except the arguments are pulled from the publicly readable properties and fields of the passed <paramref name="namedArgs"/>
        /// object.
        /// </summary>
        /// <param name="ctor">Required. The constructor to be bound.</param>
        /// <param name="namedArgs">An object whose properties/fields provide the names and values for the argument
        /// which are to be used when binding the type's constructor.  Only properties and fields whose value is
        /// <see cref="ITarget"/> are considered.</param>
        /// <remarks>Although this overload accepts a dictionary of arguments, note that it will not
        /// result in the <see cref="NamedArgs"/> property being set on the target that is created - it's
        /// just an alternative for deriving the <see cref="ParameterBindings"/> with which the target
        /// will be created.
        /// 
        /// Also, this function will not fail if the args dictionary contains named arguments that cannot
        /// be matched to parameters on the <paramref name="ctor"/>.</remarks>
        [Obsolete("This method has been replaced by the Target.ForType overloaded method and will be removed in 1.2")]
        public static ITarget WithArgs(ConstructorInfo ctor, object namedArgs)
		{
			return WithArgs(ctor, namedArgs.ToMemberValueDictionary<ITarget>());
		}

        /// <summary>
        /// Creates a <see cref="ConstructorTarget"/> with a set of named targets which will be used like
        /// named arguments to late-bind the constructor when code-generation occurs.
        /// </summary>
        /// <typeparam name="T">The type whose constructor is to be bound</typeparam>
        /// <param name="namedArgs">The named arguments to be used when building the expression.</param>
        /// <remarks>Both versions of this method will create a target which will try to find the best-matching
        /// constructor where all of the named arguments match, and with the fewest number of auto-resolved
        /// arguments.
        /// 
        /// So, a class with a constructor such as 
        /// 
        /// <code>Foo(IService1 s1, IService2 s2)</code>
        /// 
        /// Can happily be bound if you only provide a named argument for 's1'; the target will simply
        /// attempt to auto-resolve the argument for the <code>IService2 s2</code> parameter when constructing 
        /// the object - and will fail only if it can't be resolved at that point.
        /// </remarks>
        [Obsolete("This method has been replaced by the Target.ForType overloaded method and will be removed in 1.2")]
        public static ITarget WithArgs<T>(IDictionary<string, ITarget> namedArgs)
		{
			namedArgs.MustNotBeNull("args");

			return WithArgsInternal(typeof(T), namedArgs);
		}

        /// <summary>
        /// Performs the same operation as <see cref="WithArgs{T}(IDictionary{string, ITarget})"/> except the
        /// arguments are pulled from the publicly readable properties and fields of the passed <paramref name="namedArgs"/>
        /// object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="namedArgs">An object whose properties/fields provide the names and values for the argument
        /// which are to be used when binding the type's constructor.  Only properties and fields whose value is
        /// <see cref="ITarget"/> are considered.</param>
        /// <remarks>This overload exists to simplify the process of creating a ConstructorTarget with argument bindings
        /// by removing the need to create an argument dictionary in advance.  An anonymous type can instead be used
        /// to supply the arguments.</remarks>
        /// <example>This example shows how to provide an ObjectTarget for the parameter 'param1' when creating a
        /// ConstructorTarget for the type 'MyType':
        /// <code>ConstructorTarget.WithArgs&lt;MyType&gt;(new { param1 = new ObjectTarget(&quot;Hello World&quot;) });</code></example>
        [Obsolete("This method has been replaced by the Target.ForType overloaded method and will be removed in 1.2")]
        public static ITarget WithArgs<T>(object namedArgs)
		{
			return WithArgsInternal(typeof(T), namedArgs.ToMemberValueDictionary<ITarget>());
		}

		internal static ITarget WithArgsInternal(Type declaredType, IDictionary<string, ITarget> namedArgs)
		{
			return new ConstructorTarget(declaredType, namedArgs: namedArgs);
		}
	}
}
