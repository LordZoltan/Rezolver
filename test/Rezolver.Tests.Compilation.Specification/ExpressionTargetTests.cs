﻿using Rezolver.Tests.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests.Compilation.Specification
{
    public partial class CompilerTestsBase
    {
		[Fact]
		public void ExpressionTarget_ShouldResolveAnInt()
		{
			var targets = CreateTargetContainer();
			targets.RegisterExpression(() => 17);
			var container = CreateContainer(targets);

			Assert.Equal(17, container.Resolve<int>());
		}

		[Fact]
		public void ExpressionTarget_ShouldResolveAnIntFromArithmetic()
		{
			var targets = CreateTargetContainer();
			targets.RegisterExpression(() => (34 * 20) / 4);
			var container = CreateContainer(targets);
			
			Assert.Equal(170, container.Resolve<int>());
		}

		[Fact]
		public void ExpressionTarget_ShouldInjectResolveContext()
		{
			//tests that the lambda's ResolveContext parameter gets correctly mapped to the
			//ResolveContext that is passed directly to the container.  Test is only possible
			//here because it's a direct resolve, i.e. caller -> container -> target
			var targets = CreateTargetContainer();
			//note that the single parameter overload of the RegisterExpression extension methods
			//default to the concrete ResolveContext versions.
		    targets.RegisterExpression(rc => rc);
			var container = CreateContainer(targets);

			ResolveContext context = new ResolveContext(container, typeof(ResolveContext));
			Assert.Same(context, container.Resolve(context));
		}

		[Fact]
		public void ExpressionTarget_ShouldInjectArgumentAndResolveMethodCall()
		{
			//although this looks the same as the RegisterDelegate, it's not, because of what happens to 
			//lambda expressions to make argument injection work.  There were two choices with additional
			//lambda arguments: compile the lambda into a delegate and then just use it like a delegate target,
			//or rewrite the lambda so that it could be 'imported' into other target expressions just like
			//the rest.  We chose the latter.  So a lambda with arguments actually gets rewritten into an 
			//expression with a block of local variables which are explicitly resolved in at the start
			//with the necessary services - i.e. if a lambda starts off:
			// (IService myservice) => myservice.MethodCall();
			//It will be rewritten to:
			// () => {
			//   IService myservice = Functions.Resolve<IService>();
			//	 return myservice.MethodCall();
			// }
			//The resolve op will also, of course, be rewritten to the correct expression for IService.

			var targets = CreateTargetContainer();
			targets.RegisterType<ServiceWithFunctions>();
			targets.RegisterExpression((ServiceWithFunctions s) => s.GetChild(4));

			var container = CreateContainer(targets);
			
			Assert.Equal(new ServiceWithFunctions().GetChild(4).Output, container.Resolve<ServiceChild>().Output);
		}

		[Fact]
		public void ExpressionTarget_LambdaShouldResolveViaGenericHelperAndReturnAMethodCall()
		{
			//When you want to explicitly resolve something in an expression, but don't want to or can't 
			//use a lambda argument, you can also use the Function.Resolve helper method.  It does nothing
			//at run time (except throw an exception) but it instructs the target adapter to replace that
			//call with a ResolvedTarget, which is more efficient and flexible than simply emitting a 
            //runtime call to the container's Resolve method.
			var targets = CreateTargetContainer();
			targets.RegisterType<ServiceWithFunctions>();
			targets.RegisterExpression(() => ExpressionFunctions.Resolve<ServiceWithFunctions>().GetChild(5));

			var container = CreateContainer(targets);

			Assert.Equal(new ServiceWithFunctions().GetChild(5).Output, container.Resolve<ServiceChild>().Output);
		}

        [Fact]
        public void ExpressionTarget_LambdaShouldResolveViaHelperAndReturnAMethodCall()
        {
            //same as above, except not using the generic version
            var targets = CreateTargetContainer();
            targets.RegisterType<ServiceWithFunctions>();
            targets.RegisterExpression(() => ((ServiceWithFunctions)ExpressionFunctions.Resolve(typeof(ServiceWithFunctions))).GetChild(5));

            var container = CreateContainer(targets);

            Assert.Equal(new ServiceWithFunctions().GetChild(5).Output, container.Resolve<ServiceChild>().Output);
        }

        [Fact]
        public void ExpressionTarget_LambdaShouldResolveViaGenericResolveContextAndReturnAMethodCall()
        {
            //same as above, just invoking the ResolveContext's Resolve{T} method directly (which should be rewritten
            //as a ResolvedTarget also
            var targets = CreateTargetContainer();
            targets.RegisterType<ServiceWithFunctions>();
            targets.RegisterExpression(rc => rc.Resolve<ServiceWithFunctions>().GetChild(6));

            var container = CreateContainer(targets);

            Assert.Equal(new ServiceWithFunctions().GetChild(6).Output, container.Resolve<ServiceChild>().Output);
        }

        [Fact]
        public void ExpressionTarget_LambdaShouldResolveViaResolveContextAndReturnAMethodCall()
        {
            //same as above, just invoking the ResolveContext's Resolve{T} method directly (which should be rewritten
            //as a ResolvedTarget also
            var targets = CreateTargetContainer();
            targets.RegisterType<ServiceWithFunctions>();
            targets.RegisterExpression(rc => ((ServiceWithFunctions)rc.Resolve(typeof(ServiceWithFunctions))).GetChild(7));

            var container = CreateContainer(targets);

            Assert.Equal(new ServiceWithFunctions().GetChild(7).Output, container.Resolve<ServiceChild>().Output);
        }

        [Fact]
        public void ExpressionTarget_ExpressionShouldResolveViaGenericHelper()
        {
            //testing that the ExpressionFunctions static works in a non-lambda
            //to do this we just use the body of a lambda
            var targets = CreateTargetContainer();
            targets.RegisterType<ServiceWithFunctions>();
            Expression<Func<ServiceChild>> f = () => ExpressionFunctions.Resolve<ServiceWithFunctions>().GetChild(8);
            targets.RegisterExpression(f.Body, typeof(ServiceChild));

            var container = CreateContainer(targets);
            Assert.Equal(new ServiceWithFunctions().GetChild(8).Output, container.Resolve<ServiceChild>().Output);
        }

        [Fact]
        public void ExpressionTarget_ExpressionShouldResolveViaHelper()
        {
            //testing that the ExpressionFunctions static works in a non-lambda
            //to do this we just use the body of a lambda
            var targets = CreateTargetContainer();
            targets.RegisterType<ServiceWithFunctions>();
            Expression<Func<ServiceChild>> f = () => ((ServiceWithFunctions)ExpressionFunctions.Resolve(typeof(ServiceWithFunctions))).GetChild(9);
            targets.RegisterExpression(f.Body, typeof(ServiceChild));

            var container = CreateContainer(targets);
            Assert.Equal(new ServiceWithFunctions().GetChild(9).Output, container.Resolve<ServiceChild>().Output);
        }

        [Fact]
        public void ExpressionTarget_ResolvingByContextShouldGoThroughActiveScope()
        {
            var targets = CreateTargetContainer();
            targets.RegisterType<Disposable>();
            //note - this expression is ultimately identical to using 'RegisterType'
            targets.RegisterExpression(rc => new RequiresDisposable(rc.Resolve<Disposable>()));

            var container = CreateContainer(targets);

            RequiresDisposable outerResult = null;
            RequiresDisposable innerResult = null;

            using(var scope = container.CreateScope())
            {
                outerResult = scope.Resolve<RequiresDisposable>();
                using(var childScope = scope.CreateScope())
                {
                    innerResult = childScope.Resolve<RequiresDisposable>();
                }
                Assert.Equal(1, innerResult.DisposedCount);
                Assert.True(innerResult.Disposable.Disposed);
            }
            Assert.Equal(1, innerResult.DisposedCount);
            Assert.True(innerResult.Disposable.Disposed);

            Assert.Equal(1, outerResult.DisposedCount);
            Assert.True(outerResult.Disposable.Disposed);
        }

        [Fact]
        public void ExpressionTarget_ResolvingByHelperShouldGoThroughActiveScope()
        {
            var targets = CreateTargetContainer();
            targets.RegisterType<Disposable>();
            //note - this expression is ultimately identical to using 'RegisterType'
            targets.RegisterExpression(() => new RequiresDisposable(ExpressionFunctions.Resolve<Disposable>()));

            var container = CreateContainer(targets);

            RequiresDisposable outerResult = null;
            RequiresDisposable innerResult = null;

            using (var scope = container.CreateScope())
            {
                outerResult = scope.Resolve<RequiresDisposable>();
                using (var childScope = scope.CreateScope())
                {
                    innerResult = childScope.Resolve<RequiresDisposable>();
                }
                Assert.Equal(1, innerResult.DisposedCount);
                Assert.True(innerResult.Disposable.Disposed);
            }
            Assert.Equal(1, innerResult.DisposedCount);
            Assert.True(innerResult.Disposable.Disposed);

            Assert.Equal(1, outerResult.DisposedCount);
            Assert.True(outerResult.Disposable.Disposed);
        }
    }
}
