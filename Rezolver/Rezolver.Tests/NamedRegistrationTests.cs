﻿using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Rezolver.Tests
{
	[TestClass]
	public class NamedRegistrationTests
	{
		[TestMethod]
		public void ShouldSupportNamedRegistration()
		{
			IRezolveTarget target = new ObjectTarget("hello world");
			IRezolverScope scope = new RezolverScope();
			scope.Register(target, path: "name");
			var target2 = scope.Fetch(typeof (string), "name");
			Assert.AreEqual(target, target2);

		}

		[TestMethod]
		public void ShouldSupportTwoNamedRegistrations()
		{
			IRezolveTarget target1 = new ObjectTarget("hello world");
			IRezolveTarget target2 = new ObjectTarget("hello universe");
			IRezolverScope scope = new RezolverScope();
			scope.Register(target1, path: "string 1");
			scope.Register(target2, path: "string 2");
			var target1B = scope.Fetch(typeof (string), name: "string 1");
			var target2B = scope.Fetch(typeof (string), name: "string 2");

			Assert.AreEqual(target1, target1B);
			Assert.AreEqual(target2, target2B);
			
		}

		[TestMethod]
		public void ShouldSupportHierarchicalNaming()
		{
			IRezolverScope scope = new RezolverScope();
			INamedRezolverScope childScope = scope.GetNamedScope("parent.child", create: true);
			Assert.IsNotNull(childScope);
			Assert.AreEqual("child", childScope.Name);
			INamedRezolverScope parentScope = scope.GetNamedScope("parent", create: false);
			Assert.AreEqual("parent", parentScope.Name);
		}
	}
}
