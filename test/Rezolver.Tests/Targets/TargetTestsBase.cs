﻿using Rezolver.Compilation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Rezolver.Tests.Targets
{
	public class TargetTestsBase
	{
		public ITestOutputHelper Output { get; }

		public TargetTestsBase(ITestOutputHelper output)
		{
			this.Output = output;
		}

		protected class TestCompileContext : CompileContext
		{
			public TestCompileContext(ICompileContext parentContext, Type targetType = null, ScopeBehaviour? scopeBehaviourOverride = null)
				: base(parentContext, targetType, scopeBehaviourOverride) { }

			public TestCompileContext(IResolveContext resolveContext, ITargetContainer dependencyTargetContainer, Type targetType = null)
				: base(resolveContext, dependencyTargetContainer, targetType) { }
		}

		protected class TestContextProvider : ICompileContextProvider
		{
			public TestContextProvider() { }

			public ICompileContext CreateContext(IResolveContext resolveContext, ITargetContainer targets)
			{
                return new TestCompileContext(resolveContext, targets, targetType: resolveContext.RequestedType);
			}
		}

		protected class TestCompilerConfigProvider : IContainerBehaviour
		{
			public void Configure(IContainer container, ITargetContainer targets)
			{
				targets.RegisterObject<ICompileContextProvider>(new TestContextProvider());
			}
		}

		protected virtual IContainerBehaviour GetTestCompilerConfigProvider()
		{
			return new TestCompilerConfigProvider();
		}

		protected virtual IContainer GetDefaultContainer()
		{
			return new Container(new TestCompilerConfigProvider());
		}

		protected virtual ITargetContainer GetDefaultTargetContainer(IContainer container = null)
		{
			if (container is ITargetContainer)
				return ((ITargetContainer)container);
			return new TargetContainer();
		}

		protected virtual ICompileContextProvider GetContextProvider()
		{
			return new TestContextProvider();
		}



		/// <summary>
		/// Gets the compile context for the specified target under test.
		/// 
		/// The <see cref="ICompileContext.TargetType"/> is automatically set to the <see cref="ITarget.DeclaredType"/>
		/// of the passed target.
		/// </summary>
		/// <param name="target">The target.</param>
		/// <param name="container">The container to use for the <see cref="IResolveContext"/> from which the compile context
		/// will be created.  If null, then the <see cref="GetDefaultContainer"/> method is called.</param>
		/// <param name="targets">The target container to use for the compile context.  If null, then the <paramref name="container"/>
		/// will be passed to the <see cref="GetDefaultTargetContainer(IContainer)"/> method (including if one is automatically
		/// built) - with the target container that's returned being used instead.</param>
		protected virtual ICompileContext GetCompileContext(ITarget target, IContainer container = null, ITargetContainer targets = null, Type targetType = null)
		{
			container = container ?? GetDefaultContainer();
			targets = targets ?? GetDefaultTargetContainer(container);
			return GetContextProvider().CreateContext(new ResolveContext(container, targetType ?? target.DeclaredType), targets);
		}
	}
}
