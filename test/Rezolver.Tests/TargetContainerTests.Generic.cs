﻿using Rezolver.Tests.Targets;
using Rezolver.Tests.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests
{
    public class TargetContainer_GenericTests
    {
        [Fact]
        public void ShouldSupportRegisteringOpenGenericAndFetchingAsClosed()
        {
            // Arrange
            ITargetContainer targets = new TargetContainer();
            var target = Target.ForType(typeof(Generic<>));
            targets.Register(target, typeof(IGeneric<>));

            // Act
            var fetched = targets.Fetch(typeof(IGeneric<>));
            var fetchedClosed = targets.Fetch(typeof(IGeneric<int>));

            // Assert
            Assert.Same(target, fetched);
            Assert.Same(target, fetchedClosed);
        }

        [Fact]
        public void ShouldSupportRegisteringSpecialisationOfGeneric()
        {
            // Assert
            ITargetContainer targets = new TargetContainer();
            targets.RegisterType(typeof(Generic<>), typeof(IGeneric<>));

            // Act
            var fetched = targets.Fetch(typeof(IGeneric<int>));

            // Assert
            Assert.NotNull(fetched);
            Assert.False(fetched.UseFallback);
        }

        [Fact]
        public void ShouldFavourSpecialisationOfGenericInt()
        {
            // Arrange
            ITargetContainer targets = new TargetContainer();
            var notExpected = Target.ForType(typeof(Generic<>));
            var expected = Target.ForType(typeof(AltGeneric<int>));
            targets.Register(notExpected, typeof(IGeneric<>));
            targets.Register(expected, typeof(IGeneric<int>));

            // Act
            var fetched = targets.Fetch(typeof(IGeneric<int>));

            // Assert
            Assert.NotSame(notExpected, fetched);
            Assert.Same(expected, fetched);
        }

        [Fact]
        public void ShouldSupportRegisteringAndRetrievingGenericWithGenericParameter()
        {
            // Arrange
            ITargetContainer targets = new TargetContainer();
            var target = Target.ForType(typeof(Generic<>));
            targets.Register(target, typeof(IGeneric<>));

            // Act
            var fetched = targets.Fetch(typeof(IGeneric<IGeneric<int>>));

            // Assert
            Assert.Same(target, fetched);
        }

        [Fact]
        public void ShouldSupportRegisteringAndRetrievingGenericWithAsymmetricGenericBase()
        {
            // Arrange
            ITargetContainer targets = new TargetContainer();
            var target = Target.ForType(typeof(NestedGenericA<>));
            targets.Register(target, typeof(IGeneric<>).MakeGenericType(typeof(IEnumerable<>)));

            // Act
            var fetched = targets.Fetch(typeof(IGeneric<IEnumerable<int>>));

            // Assert
            Assert.Same(target, fetched);
        }

        [Fact]
        public void ShouldFavourGenericSpecialisationOfGeneric()
        {
            // Arrange
            ITargetContainer targets = new TargetContainer();
            var target = Target.ForType(typeof(Generic<>));

            //note here - using MakeGenericType is the only way to get a reference to a type like IFoo<IFoo<>> because
            //supply an open generic as a type parameter to a generic is not valid.
            var target2 = Target.ForType(typeof(NestedGenericA<>));

            targets.Register(target, typeof(IGeneric<>));
            targets.Register(target2, typeof(IGeneric<>).MakeGenericType(typeof(IEnumerable<>)));

            // Act
            var fetched = targets.Fetch(typeof(IGeneric<IEnumerable<int>>));
            var fetched2 = targets.Fetch(typeof(IGeneric<int>));

            // Assert
            Assert.Same(target2, fetched);
            Assert.Same(target, fetched2);
        }

        [Fact]
        public void ShouldNotFetchConstrainedGenericForIncompatibleType()
        {
            // Arrange
            ITargetContainer targets = new TargetContainer();
            var expected = Target.ForType(typeof(Generic<>));
            var notexpected = Target.ForType(typeof(ConstrainedGeneric<>));
            targets.Register(expected, typeof(IGeneric<>));
            targets.Register(notexpected, typeof(IGeneric<>));

            // Act
            var single = targets.Fetch(typeof(IGeneric<string>));
            var all = targets.FetchAll(typeof(IGeneric<string>));

            // Assert
            Assert.Same(expected, single);
            Assert.Single(all, expected);
        }

        [Fact]
        public void ShouldFetchConstrainedGenericInsteadOfOpen()
        {
            // registration order matters, for now, when registering constrained generics
            // because they are registered against the open generic type.  Therefore, if 
            // an unconstrained open generic is registered *after* one with constraints, then
            // that will win for any single-service Fetch.

            // Arrange
            ITargetContainer targets = new TargetContainer();
            var openTarget = Target.ForType(typeof(Generic<>));
            var constrainedTarget = Target.ForType(typeof(ConstrainedGeneric<>));
            targets.Register(openTarget, typeof(IGeneric<>));
            targets.Register(constrainedTarget, typeof(IGeneric<>));

            // Act
            var fetched = targets.Fetch(typeof(IGeneric<BaseClassChild>));
            // this should return both as they both apply
            var all = targets.FetchAll(typeof(IGeneric<BaseClassChild>));

            // Assert
            Assert.Same(constrainedTarget, fetched);
            Assert.Equal(new[] { openTarget, constrainedTarget }, all);
        }
    }
}
