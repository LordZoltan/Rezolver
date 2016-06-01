﻿using Rezolver.Tests.TestTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests
{
	public class DecoratorTests : TestsBase
	{
		[Fact]
		public void ShouldDecorateDecoratedType()
		{
			RezolverBuilder builder = new RezolverBuilder();
			builder.RegisterType<DecoratedType, IDecorated>();
			builder.RegisterDecorator<DecoratorType, IDecorated>();
			var rezolver = new DefaultRezolver(builder);
			var result = rezolver.Resolve<IDecorated>();
			Assert.IsType<DecoratorType>(result);
			//yes, we've checked the type - but we also need to check the 
			//argument is passed into the constructor.  Obviously, we could do this
			//by null-checking in the constructor, but this is more fun.
			Assert.Equal("Hello World", result.DoSomething());
		}

		[Fact]
		public void ShouldDecorateDecoratedTypeAddedAfterDecorator()
		{
			RezolverBuilder builder = new RezolverBuilder();
			builder.RegisterDecorator<DecoratorType, IDecorated>();
			builder.RegisterType<DecoratedType, IDecorated>();
			var rezolver = new DefaultRezolver(builder);
			var result = rezolver.Resolve<IDecorated>();
			Assert.IsType<DecoratorType>(result);

			Assert.Equal("Hello World", result.DoSomething());
		}

		[Fact]
		public void ShouldDecorateDecorator()
		{
			//see if stacking multiple decorators works.
			RezolverBuilder builder = new RezolverBuilder();
			builder.RegisterType<DecoratedType, IDecorated>();
			builder.RegisterDecorator<DecoratorType, IDecorated>();
			builder.RegisterDecorator<AnotherDecoratorType, IDecorated>();

			var rezolver = new DefaultRezolver(builder);
			var result = rezolver.Resolve<IDecorated>();
			Assert.IsType<AnotherDecoratorType>(result);

			Assert.Equal("OMG: Hello World", result.DoSomething());
		}
	}
}
