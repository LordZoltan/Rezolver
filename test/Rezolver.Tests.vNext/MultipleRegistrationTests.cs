﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xunit;

namespace Rezolver.Tests.vNext
{
	/// <summary>
	/// tests rezolverbuilder and rezolver types for whether they support IEnumerable{Service} - both after registering
	/// a single entry, or multiple.
	/// </summary>
	public class MultipleRegistrationTests : TestsBase
	{
		//first half of the tests do registration

		class FakeTarget : IRezolveTarget
		{
			public Type DeclaredType
			{
				get
				{
					return typeof(MultipleRegistrationTests);
				}
			}

			public Expression CreateExpression(CompileContext context)
			{
				return null;
			}

			public bool SupportsType(Type type)
			{
				return true;
			}
		}

		[Fact]
		public void ShouldFetchDefault()
		{
			RezolverBuilder builder = new RezolverBuilder();
			var targets = Enumerable.Range(0, 3).Select(i => new FakeTarget()).ToArray();

			foreach (var target in targets)
			{
				builder.Register(target, typeof(MultipleRegistrationTests));
			}

			var entry = builder.Fetch(typeof(MultipleRegistrationTests), null);

			Assert.Same(targets[2], entry.DefaultTarget);
		}

		[Fact]
		public void ShouldFetchIEnumerableForSingleRegistration()
		{
			RezolverBuilder builder = new RezolverBuilder();
			var target = new FakeTarget();
			builder.Register(target, typeof(MultipleRegistrationTests));

			var entry = builder.Fetch(typeof(IEnumerable<MultipleRegistrationTests>), null);

			Assert.Same(target, entry.DefaultTarget);
		}

		//this half does resolving

		public interface IService
		{

		}

		public class ServiceA : IService
		{
			public int IntValue { get; private set; }
			public ServiceA(int intValue)
			{
				IntValue = intValue;
			}
		}

		public class ServiceB : IService
		{
			public double DoubleValue { get; private set; }
			public string StringValue { get; private set; }
		}

		public class RequiresServices
		{
			public IEnumerable<IService> Services { get; private set; }
			public RequiresServices(IEnumerable<IService> services)
			{
				Services = services;
			}
		}

		[Fact]
		public void ShouldResolveOneServiceForIEnumerableDependency()
		{
			var rezolver = CreateADefaultRezolver();
			rezolver.Register((10).AsObjectTarget());
			rezolver.RegisterType<RequiresServices>();
			rezolver.RegisterType<ServiceA, IService>();
			var result = rezolver.Resolve<RequiresServices>();
			Assert.Single<IService>(result.Services);
			Assert.IsType<ServiceA>(result.Services.First());
		}

		[Fact]
		public void ShouldRegisterAndResolveMultipleServiceInstances()
		{
			var rezolver = CreateADefaultRezolver();
			rezolver.Register((10).AsObjectTarget());
			rezolver.Register((20.0).AsObjectTarget());
			rezolver.Register("hello multiple".AsObjectTarget());
			rezolver.RegisterMultiple(new[] { ConstructorTarget.Auto<ServiceA>(), ConstructorTarget.Auto<ServiceB>() }, typeof(IService));

			var result = rezolver.Resolve(typeof(IEnumerable<IService>));
			Assert.NotNull(result);
			var resultArray = ((IEnumerable<IService>)result).ToArray();
			Assert.Equal(2, resultArray.Length);
		}

		[Fact]
		public void ShouldRegisterAndResolveMultipleServiceInstancesAsDependency()
		{
			var rezolver = CreateADefaultRezolver();
			rezolver.Register((10).AsObjectTarget());
			rezolver.Register((20.0).AsObjectTarget());
			rezolver.Register("hello multiple".AsObjectTarget());
			rezolver.RegisterMultiple(new[] { ConstructorTarget.Auto<ServiceA>(), ConstructorTarget.Auto<ServiceB>() }, typeof(IService));
			rezolver.Register(ConstructorTarget.Auto<RequiresServices>());

			var result = (RequiresServices)rezolver.Resolve(typeof(RequiresServices));
			Assert.NotNull(result.Services);
			Assert.Equal(2, result.Services.Count());
			Assert.Equal(1, result.Services.OfType<ServiceA>().Count());
			Assert.Equal(1, result.Services.OfType<ServiceB>().Count());
		}
	}
}