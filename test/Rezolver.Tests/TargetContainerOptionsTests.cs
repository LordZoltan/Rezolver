﻿using Rezolver.Options;
using Rezolver.Tests.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests
{
    public class TargetContainerOptionsTests
    {
        [Fact]
        public void ShouldGetAndSetOptions()
        {
            var targets = new TargetContainer();

            targets.SetOption<TestOption>("first")
                .SetOption<TestOption, int>("second")
                .SetOption<TestOption>("third", typeof(IEnumerable<>))
                .SetOption<TestOption, IEnumerable<int>>("fourth");

            Assert.Equal("first", targets.GetOption<TestOption>());
            Assert.Equal("second", targets.GetOption<TestOption, int>());
            Assert.Equal("third", targets.GetOption<TestOption>(typeof(IEnumerable<>)));
            Assert.Equal("fourth", targets.GetOption<TestOption, IEnumerable<int>>());
        }

        [Fact]
        public void ShouldUseOpenGenericServiceOptionForClosed()
        {
            var targets = new TargetContainer();

            targets.SetOption<TestOption>("open", typeof(IEnumerable<>));

            Assert.Equal("open", targets.GetOption<TestOption, IEnumerable<int>>());
        }

        [Fact]
        public void ClosedGenericServiceOptionShouldOverrideOpenGeneric()
        {
            var targets = new TargetContainer();

            targets.SetOption<TestOption>("open", typeof(IEnumerable<>));
            targets.SetOption<TestOption, IEnumerable<int>>("int");

            Assert.Equal("int", targets.GetOption<TestOption, IEnumerable<int>>());
        }

        [Fact]
        public void ShouldFallBackToGlobalOptionByDefault()
        {
            // setting a global option should apply for all types that don't have
            // that option explicitly set.
            var targets = new TargetContainer();

            targets.SetOption<TestOption>("global");

            Assert.Equal("global", targets.GetOption<TestOption, int>());
            Assert.Equal("global", targets.GetOption<TestOption, IEnumerable<int>>());
            Assert.Equal("global", targets.GetOption<TestOption>(typeof(IEnumerable<>)));
        }

        [Fact]
        public void ShouldUseDefaultInsteadOfGlobalWhenConfigured()
        {
            var targets = new TargetContainer();

            // you can control how the options functionality works by using options :)
            targets.SetOption<EnableGlobalOptions>(false);

            // set a global option which will not be used.
            targets.SetOption<TestOption>("global");

            Assert.Equal("default", targets.GetOption<TestOption, int>("default"));
            Assert.Equal("default", targets.GetOption<TestOption, IEnumerable<int>>("default"));
            Assert.Equal("default", targets.GetOption<TestOption>(typeof(IEnumerable<>), "default"));
        }

        [Fact]
        public void OptionShouldBeInheritedByOverridingTargetContainer()
        {
            var targets = new TargetContainer();

            targets.SetOption<TestOption>("base");

            var overriding = new OverridingTargetContainer(targets);

            Assert.Equal("base", overriding.GetOption<TestOption>());
        }

        [Fact]
        public void OptionShouldBeOverridenByOverridingTargetContainer()
        {
            var targets = new TargetContainer();
            targets.SetOption<TestOption>("base");

            var overriding = new OverridingTargetContainer(targets);
            overriding.SetOption<TestOption>("overriden");

            Assert.Equal("overriden", overriding.GetOption<TestOption>());
        }

        [Fact]
        public void ShouldUseOptionForBaseClass()
        {
            var targets = new TargetContainer();
            // the underlying options container that's used to store the option value
            // is contravariant, so should return an option defined at the base class level :)
            targets.SetOption<TestOption>("for baseclass", typeof(BaseClass));

            Assert.Equal("for baseclass", targets.GetOption<TestOption, BaseClassChild>());
        }
    }
}