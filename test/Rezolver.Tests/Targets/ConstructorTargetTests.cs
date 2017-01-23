﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rezolver.Targets;
using Xunit;
using System.Reflection;
using Xunit.Abstractions;

namespace Rezolver.Tests.Targets
{
	/// <summary>
	/// Testing the base functionality of the ConstructorTarget (no compilation)
	/// </summary>
	public class ConstructorTargetTests : TargetTestsBase
	{
		public ConstructorTargetTests(ITestOutputHelper output)
			: base(output)
		{

		}

		[Fact]
		public void ShouldCreateTargetForType()
		{
			var target = new ConstructorTarget(typeof(NoCtor));

			Assert.Equal(typeof(NoCtor), target.DeclaredType);
			//constructor should not be bound up front
			Assert.Null(target.Ctor);
			Assert.Null(target.MemberBindingBehaviour);
			Assert.Equal(0, target.NamedArgs.Count);
			Assert.Equal(0, target.ParameterBindings.Count);
		}

		[Fact]
		public void ShouldCreateTargetForCtor()
		{
			var ctor = TypeHelpers.GetConstructor(typeof(NoCtor), Type.EmptyTypes);
			var target = new ConstructorTarget(ctor);

			//declared type should be lifted from the ctor
			Assert.Equal(typeof(NoCtor), target.DeclaredType);
			Assert.Same(ctor, target.Ctor);
			Assert.Null(target.MemberBindingBehaviour);
			Assert.Equal(0, target.NamedArgs.Count);
			Assert.Equal(0, target.ParameterBindings.Count);
		}

		[Fact]
		public void ShouldSetMemberBindingBehaviour()
		{
			//test both Ctors here
			var ctor = TypeHelpers.GetConstructor(typeof(NoCtor), Type.EmptyTypes);
			var target1 = new ConstructorTarget(typeof(NoCtor), memberBindingBehaviour: DefaultMemberBindingBehaviour.Instance);
			var target2 = new ConstructorTarget(ctor, memberBindingBehaviour: DefaultMemberBindingBehaviour.Instance);

			Assert.Same(target1.MemberBindingBehaviour, DefaultMemberBindingBehaviour.Instance);
			Assert.Same(target2.MemberBindingBehaviour, DefaultMemberBindingBehaviour.Instance);
		}

		[Fact]
		public void ShouldSetNamedArgs()
		{
			//named arguments are used when create a JIT-bound target for a type
			//NOTE: it doesn't matter that the argument doesn't have a matching parameter
			Dictionary<string, ITarget> namedArgs = new Dictionary<string, ITarget>()
			{
				["arg"] = new TestTarget()
			};

			var target = new ConstructorTarget(typeof(NoCtor), namedArgs: namedArgs);

			Assert.Equal(1, target.NamedArgs.Count);
			Assert.Same(target.NamedArgs["arg"], namedArgs["arg"]);
		}

		[Fact]
		public void ShouldSetParameterBindings()
		{
			var ctor = TypeHelpers.GetConstructor(typeof(OneCtor), new[] { typeof(int) });
			var bindings = new[] { new ParameterBinding(ctor.GetParameters()[0], new TestTarget(typeof(int))) };

			var target = new ConstructorTarget(ctor, parameterBindings: bindings);

			Assert.Equal(1, target.ParameterBindings.Count);
			//bindings should not be cloned
			Assert.Same(bindings[0], target.ParameterBindings[0]);
		}

		//on to the bindings tests.
		//need to test:
		//1) Type-only constructor binding
		//  - a) Default constructor (simple)
		//  - b) Binding one non-default constructor
		//	- c) Greedy matching with multiple ctors (default + one with params) and no optional params
		//  - d) Greedy matching with multiple ctors, disambiguating by number of optional params ONLY
		//  - e) Greedy matching with multiple ctors, disambiguating by number of resolved args
		//  - f) Greedy matching multiple ctors, disambiguating by named arg matches
		//2) Constructor-specific binding
		//  - a) Supplied default constructor
		//  - b) Supplied constructor with parameters, but no parameter bindings
		//  - c) Supplied constructor with parameters + all parameter bindings
		//  - d) Supplied constructor with parameters + some bindings (with others being auto-created: NEW feature)
		// Exceptions:
		// 1) No constructors found
		// 2) Type-only binding can't choose between 2 or more possible matches

		/// <summary>
		/// The workhorse for our expected type bindings theories run by <see cref="ShouldBindToConstructor(ExpectedTypeBinding)"/>.
		/// 
		/// Covers searching for the best constructor based on a type, the services
		/// available in the container and, optionally, a provided set of named argument
		/// bindings.
		/// </summary>
		public class ExpectedTypeBinding
		{
			public Type Type { get; }
			public ConstructorInfo ExpectedConstructor { get; }
			public Func<IContainer> ContainerFactory { get; }
			public Func<IDictionary<string, ITarget>> NamedArgsFactory { get; }
			public string Description { get; }

			public ExpectedTypeBinding(Type type, 
				ConstructorInfo expectedConstructor, 
				Func<IContainer> containerFactory = null, 
				Func<Dictionary<string, ITarget>> namedArgsFactory = null,
				string description = null)
			{
				Type = type;
				ExpectedConstructor = expectedConstructor;
				ContainerFactory = containerFactory ?? (() => null);
				NamedArgsFactory = NamedArgsFactory ?? (() => null);
				Description = description;
			}

			public override string ToString()
			{
				return $"Type: { Type }, Expected ctor: { ExpectedConstructor }, Container?: { ContainerFactory != null }, Named Args?: { NamedArgsFactory != null }";
			}

			public void Run(ConstructorTargetTests host)
			{
				host.Output.WriteLine("Running {0}.  Parameters:", Description ?? "Unknown Bindings Test");
				host.Output.WriteLine(ToString());
				var target = new ConstructorTarget(Type);
				var compileContext = host.GetCompileContext(target, ContainerFactory());

				var binding = target.Bind(compileContext);

				Xunit.Assert.NotNull(binding);
				Xunit.Assert.Same(ExpectedConstructor, binding.Constructor);
				host.Output.WriteLine("Test Complete");
			}
		}

		public static IEnumerable<object[]> ConstructorBindingData()
		{
			return new object[][]
			{
				new[] {  new ExpectedTypeBinding(
					typeof(NoCtor),
					TypeHelpers.GetConstructor(typeof(NoCtor), Type.EmptyTypes),
					description: "Default constructor"
				)},
				new[] {  new ExpectedTypeBinding(
					typeof(OneCtor),
					TypeHelpers.GetConstructor(typeof(OneCtor), new[] { typeof(int) }),
					description: "Constructor with parameters"
				)},
				new[] {  new ExpectedTypeBinding(
					typeof(TwoCtors),
					TypeHelpers.GetConstructor(typeof(TwoCtors), new[] { typeof(string), typeof(int) }),
					description: "Constructor with greatest number of parameters (i.e. 'greedy')"
				)},

				new [] { new ExpectedTypeBinding(
					typeof(TwoCtorsOneNoOptional),
					TypeHelpers.GetConstructor(typeof(TwoCtorsOneNoOptional), new[] { typeof(string), typeof(int), typeof(object) }),
					description: "Constructor with least number of optional parameters when more than one have the largest number of parameters"
				)},
				new[] { new ExpectedTypeBinding(
					typeof(TwoCtors),
					TypeHelpers.GetConstructor(typeof(TwoCtors), new [] { typeof(string) }),
					() => {
						var container = new Container();
						//fallback switched off for the target because we're simulating a direct match
						container.Register(new TestTarget(typeof(string), useFallBack: false));
						return container;
					},
					description: "Constructor with greatest number of resolved arguments supersedes greedy behaviour"
				)}
			};
		}

		[Theory]
		[MemberData(nameof(ConstructorBindingData))]
		public void ShouldBindToConstructor(ExpectedTypeBinding test)
		{
			test.Run(this);
		}
	}
}
