﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Rezolver.Tests
{
	[TestClass]
	public class GenericConstructorTargetTests : TestsBase
	{
		#region diagnostic stuff
		private string GetTypeReportString(Type type)
		{
			var genericArgs = type.GetGenericArguments() ?? Type.EmptyTypes;
			return string.Format("{0}<{1}>:", type.Name, string.Join(", ", genericArgs.Select(typeParam => string.Format("({0}{1})", typeParam.IsGenericParameter ? "*" : "", typeParam.Name))));
		}

		private void WriteType(Type type, string typeType)
		{
			Console.WriteLine("{0} {1}", typeType, GetTypeReportString(type));
			var interfaces = type.GetInterfaces();
			if (interfaces.Length != 0)
			{
				Console.WriteLine("Interfaces for {0}: ", type);
				foreach (var i in interfaces)
				{
					WriteType(i, "Interface");
				}
			}
			if (type.BaseType != null && type.BaseType != typeof(object))
				WriteType(type.BaseType, "Base");
		}

		//[TestMethod]
		public void ShouldBuildTypeParameterMap()
		{
			var types = new[] { typeof(IBaseInterface<>), typeof(IDerivedInterface<,>), typeof(IFinalInterface<,,>),
				typeof(BaseInterfaceClass<>), typeof(DerivedInterfaceClass<,>), typeof(FinalInterfaceClass<,,>) };

			foreach (var type in types)
			{
				WriteType(type, "Type");
				Console.Write("----------------------------------------------");
				Console.WriteLine();
			}

		}
		#endregion

		public interface IBaseInterface<T1> { }
		public interface IDerivedInterface<Ta, Tb> : IBaseInterface<Tb> { }
		public interface IFinalInterface<Tx, Ty, Tz> : IDerivedInterface<Tz, Ty> { }
		public class BaseInterfaceClass<Ta1> : IBaseInterface<Ta1> { }
		public class DerivedInterfaceClass<Taa, Tab> : BaseInterfaceClass<Taa>, IDerivedInterface<Taa, Tab> { }
		public class FinalInterfaceClass<Tax, Tay, Taz> : DerivedInterfaceClass<Tay, Tax>, IFinalInterface<Tax, Tay, Tay> { }

		public interface IGeneric<T>
		{
			T Value { get; }
		}

		/// <summary>
		/// alternative IGeneric-like interface used to simplify the nested open generic scenario
		/// </summary>
		/// <typeparam name="T"></typeparam>
		public interface IGenericA<T>
		{
			T Value { get; }
		}

		public interface IGeneric2<T, U> : IGeneric<U>
		{
			T Value1 { get; }
			U Value2 { get; }
		}

		public class GenericNoCtor<T> : IGeneric<T>
		{
			public T Value { get; set; }
		}

		public class Generic<T> : IGeneric<T>
		{
			private T _value;

			public Generic(T value)
			{
				_value = value;
			}

			public T Value
			{
				get { return _value; }
			}
		}

		public class GenericA<T> : IGenericA<T>
		{
			private T _value;

			public GenericA(T value)
			{
				_value = value;
			}

			public T Value
			{
				get { return _value; }
			}
		}

		/// <summary>
		/// this is pretty hideous - but might be something that needs to be supported
		/// 
		/// pushes the discovery of type parameters by forcing unwrap another nested generic type parameter.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		public class GenericGeneric<T> : IGeneric<IGeneric<T>>
		{

			public IGeneric<T> Value
			{
				get;
				private set;
			}

			public GenericGeneric(IGeneric<T> value)
			{
				Value = value;
			}
		}

		public class Generic2<T, U> : IGeneric2<T, U>
		{
			public Generic2(T value1, U value2)
			{
				Value1 = value1;
				Value2 = value2;
			}

			public T Value1
			{
				get;
				private set;
			}

			public U Value2
			{
				get;
				private set;
			}

			//explicit implementation of IGeneric<U>
			U IGeneric<U>.Value
			{
				get { return Value2; }
			}
		}

		public class Generic2Reversed<T, U> : IGeneric2<U, T>
		{
			public Generic2Reversed(T value2, U value1)
			{
				Value1 = value1;
				Value2 = value2;
			}

			public U Value1
			{
				get;
				private set;
			}

			public T Value2
			{
				get;
				private set;
			}

			//explicit implementation of IGeneric<T>
			T IGeneric<T>.Value
			{
				get { return Value2; }
			}
		}

		[Obsolete("yet to be implemented", true)]
		public class DerivedGeneric<T> : Generic<T>
		{
			public DerivedGeneric(T value) : base(value) { }
		}

		public class HasGenericDependency
		{
			public Generic<int> Dependency { get; private set; }
			public HasGenericDependency(Generic<int> dependency)
			{
				Dependency = dependency;
			}
		}

		public class HasOpenGenericDependency<T>
		{
			public Generic<T> Dependency { get; private set; }
			public HasOpenGenericDependency(Generic<T> dependency)
			{
				Dependency = dependency;
			}
		}

		public class HasGenericInterfaceDependency
		{
			public IGeneric<int> Dependency { get; private set; }
			public HasGenericInterfaceDependency(IGeneric<int> dependency)
			{
				Dependency = dependency;
			}
		}

		public class HasOpenGenericInterfaceDependency<T>
		{
			public IGeneric<T> Dependency { get; private set; }
			public HasOpenGenericInterfaceDependency(IGeneric<T> dependency)
			{
				Dependency = dependency;
			}
		}

		[TestMethod]
		public void ShouldCreateGenericNoCtorClass()
		{
			IRezolveTarget t = GenericConstructorTarget.Auto(typeof(GenericNoCtor<>));
			Assert.IsNotNull(t);
			Assert.AreEqual(typeof(GenericNoCtor<>), t.DeclaredType);
			//try and build an instance 
			var instance = GetValueFromTarget<GenericNoCtor<int>>(t);
			Assert.IsNotNull(instance);
			Assert.AreEqual(default(int), instance.Value);
		}

		[TestMethod]
		public void ShouldResolveAGenericNoCtorClass()
		{
			//similar test to above, but we're testing that it works when you put the target inside the
			//default resolver.
			var rezolver = new DefaultRezolver(compiler: new RezolveTargetDelegateCompiler());
			rezolver.Register(GenericConstructorTarget.Auto(typeof(GenericNoCtor<>)));
			var instance = (GenericNoCtor<int>)rezolver.Resolve(typeof(GenericNoCtor<int>));
			Assert.IsNotNull(instance);
		}

		[TestMethod]
		public void ShouldCreateAGenericClass()
		{
			//use a rezolver mock for cross-referencing the int parameter
			var rezolverMock = new Mock<IRezolver>();
			rezolverMock.Setup(r => r.Fetch(typeof(int), It.IsAny<string>())).Returns((1).AsObjectTarget());
			rezolverMock.Setup(r => r.Compiler).Returns(new RezolveTargetDelegateCompiler());
			IRezolveTarget t = GenericConstructorTarget.Auto(typeof(Generic<>));
			Assert.IsNotNull(t);
			Assert.AreEqual(typeof(Generic<>), t.DeclaredType);
			var instance = GetValueFromTarget<Generic<int>>(t, rezolverMock.Object);
			Assert.IsNotNull(instance);
			Assert.AreEqual(1, instance.Value);
		}

		[TestMethod]
		public void ShouldRezolveAGenericClass()
		{
			//in this one, using DefaultRezolver, we're going to test a few parameter types
			IRezolver rezolver = new DefaultRezolver(compiler: new RezolveTargetDelegateCompiler());
			rezolver.Register(GenericConstructorTarget.Auto(typeof(Generic<>)));
			rezolver.Register((2).AsObjectTarget());
			rezolver.Register((3).AsObjectTarget(typeof(int?)));
			rezolver.Register("hello world".AsObjectTarget());
			var instance1 = (Generic<int>)rezolver.Resolve(typeof(Generic<int>));
			var instance2 = (Generic<string>)rezolver.Resolve(typeof(Generic<string>));
			var instance3 = (Generic<int?>)rezolver.Resolve(typeof(Generic<int?>));

			Assert.AreEqual(2, instance1.Value);
			Assert.AreEqual("hello world", instance2.Value);
			Assert.AreEqual(3, instance3.Value);
		}

		//now test that the target should work when used as the target of a dependency look up
		//just going to use the DefaultRezolver for this as it's far easier to setup the test.

		[TestMethod]
		public void ShouldRezolveAClosedGenericDependency()
		{
			IRezolver rezolver = new DefaultRezolver(compiler: new RezolveTargetDelegateCompiler());
			rezolver.Register(GenericConstructorTarget.Auto(typeof(Generic<>)));
			rezolver.Register((2).AsObjectTarget());
			rezolver.Register(ConstructorTarget.Auto<HasGenericDependency>());

			var result = (HasGenericDependency)rezolver.Resolve(typeof(HasGenericDependency));
			Assert.IsNotNull(result);
			Assert.IsNotNull(result.Dependency);
			Assert.AreEqual(2, result.Dependency.Value);
		}

		[TestMethod]
		public void ShouldRezolveNestedGenericDependency()
		{
			//this one is more complicated.  Passing a closed generic as a type argument to another
			//generic.
			//note that this isn't the most complicated it can get, however: that would be using
			//the type argument as a type argument to another open generic dependency.  That one is on it's way.
			IRezolver rezolver = new DefaultRezolver(compiler: new RezolveTargetDelegateCompiler());

			rezolver.Register(GenericConstructorTarget.Auto(typeof(Generic<>)));
			rezolver.Register(GenericConstructorTarget.Auto(typeof(GenericNoCtor<>)));

			var result = (Generic<GenericNoCtor<int>>)rezolver.Resolve(typeof(Generic<GenericNoCtor<int>>));
			Assert.IsNotNull(result);
			Assert.IsNotNull(result.Value);
			Assert.AreEqual(0, result.Value.Value);
		}


		//this one is the open generic nested dependency check

		[TestMethod]
		public void ShouldResolveNestedeOpenGenericDependency()
		{
			IRezolver rezolver = new DefaultRezolver(compiler: new RezolveTargetDelegateCompiler());

			rezolver.Register((10).AsObjectTarget());
			rezolver.Register(GenericConstructorTarget.Auto(typeof(Generic<>)));
			rezolver.Register(GenericConstructorTarget.Auto(typeof(HasOpenGenericDependency<>)));

			var result = (HasOpenGenericDependency<int>)rezolver.Resolve(typeof(HasOpenGenericDependency<int>));
			Assert.IsNotNull(result);
			Assert.IsNotNull(result.Dependency);
			Assert.AreEqual(10, result.Dependency.Value);
		}

		//now moving on to rezolving interface instead of the type directly

		[TestMethod]
		public void ShouldResolveGenericViaInterface()
		{
			IRezolver rezolver = new DefaultRezolver(compiler: new RezolveTargetDelegateCompiler());
			rezolver.Register((20).AsObjectTarget());
			rezolver.Register(GenericConstructorTarget.Auto(typeof(Generic<>)), typeof(IGeneric<>));

			var result = (IGeneric<int>)rezolver.Resolve(typeof(IGeneric<int>));
			Assert.IsInstanceOfType(result, typeof(Generic<int>));
			Assert.AreEqual(20, result.Value);
		}

		[TestMethod]
		public void ShouldRezolveGenericViaGenericInterface()
		{
			//first version of this test - where the nested generic interface is different
			//to the outer generic interface.  At the time of writing, making it the same causes
			//a circular dependency - see Bug #7

			IRezolver rezolver = CreateADefaultRezolver();
			//we need three dependencies registered - the inner T, an IGenericA<> and 
			//an IGeneric<IGenericA<T>>.
			rezolver.Register((25).AsObjectTarget());
			rezolver.Register(GenericConstructorTarget.Auto(typeof(GenericA<>)), typeof(IGenericA<>));
			//note here - using MakeGenericType is the only way to get a reference to a type like IFoo<IFoo<>> because
			//supply an open generic as a type parameter to a generic is not valid.
			rezolver.Register(GenericConstructorTarget.Auto(typeof(GenericGeneric<>)), typeof(IGeneric<>).MakeGenericType(typeof(IGenericA<>)));

			var result = (IGeneric<IGenericA<int>>)rezolver.Resolve(typeof(IGeneric<IGenericA<int>>));

			Assert.AreEqual(25, result.Value.Value);
		}

		[TestMethod]
		public void ShouldResolveClosedGenericViaInterfaceDependency()
		{
			IRezolver rezolver = new DefaultRezolver(compiler: new RezolveTargetDelegateCompiler());
			rezolver.Register((30).AsObjectTarget());
			rezolver.Register(GenericConstructorTarget.Auto(typeof(Generic<>)), typeof(IGeneric<>));
			rezolver.Register(ConstructorTarget.Auto<HasGenericInterfaceDependency>());
			var result = (HasGenericInterfaceDependency)rezolver.Resolve(typeof(HasGenericInterfaceDependency));
			Assert.IsNotNull(result.Dependency);
			Assert.AreEqual(30, result.Dependency.Value);
		}

		[TestMethod]
		public void ShouldResolveOpenGenericViaInterfaceDependency()
		{
			IRezolver rezolver = CreateADefaultRezolver();
			rezolver.Register((40).AsObjectTarget());
			rezolver.Register((50d).AsObjectTarget(typeof(double?))); //will that work?
			rezolver.Register("hello interface generics!".AsObjectTarget());
			//now register the IGeneric<T>
			rezolver.Register(GenericConstructorTarget.Auto(typeof(Generic<>)), typeof(IGeneric<>));
			rezolver.Register(GenericConstructorTarget.Auto(typeof(HasOpenGenericInterfaceDependency<>)));
			var resultWithInt = (HasOpenGenericInterfaceDependency<int>)rezolver.Resolve(typeof(HasOpenGenericInterfaceDependency<int>));
			var resultWithNullableDouble = (HasOpenGenericInterfaceDependency<double?>)rezolver.Resolve(typeof(HasOpenGenericInterfaceDependency<double?>));
			var resultWithString = (HasOpenGenericInterfaceDependency<string>)rezolver.Resolve(typeof(HasOpenGenericInterfaceDependency<string>));
			Assert.IsNotNull(resultWithInt);
			Assert.IsNotNull(resultWithNullableDouble);
			Assert.IsNotNull(resultWithString);
			Assert.IsNotNull(resultWithInt.Dependency);
			Assert.IsNotNull(resultWithNullableDouble.Dependency);
			Assert.IsNotNull(resultWithString.Dependency);
			Assert.AreEqual(40, resultWithInt.Dependency.Value);
			Assert.AreEqual(50, resultWithNullableDouble.Dependency.Value);
			Assert.AreEqual("hello interface generics!", resultWithString.Dependency.Value);
		}

		//now on to the multiple type parameters
		//first by direct type
		[TestMethod]
		public void ShouldResolveGenericTypeWith2Parameters()
		{
			var rezolver = CreateADefaultRezolver();
			rezolver.Register((60).AsObjectTarget());
			rezolver.Register("hello multiple".AsObjectTarget());
			rezolver.Register(GenericConstructorTarget.Auto(typeof(Generic2<,>)));
			var result = (Generic2<int, string>)rezolver.Resolve(typeof(Generic2<int, string>));
			Assert.AreEqual(60, result.Value1);
			Assert.AreEqual("hello multiple", result.Value2);

			var result2 = (Generic2<string, int>)rezolver.Resolve(typeof(Generic2<string, int>));
			Assert.AreEqual("hello multiple", result2.Value1);
			Assert.AreEqual(60, result2.Value2);
		}

		//TODO: resolve by base (DerivedGeneric<T>) and then probably using reversed parameters also.
		[TestMethod]
		public void ShouldResolveGenericTypeWith2ParametersByInterface()
		{
			var rezolver = CreateADefaultRezolver();
			rezolver.Register((70).AsObjectTarget());
			rezolver.Register("hello multiple interface".AsObjectTarget());
			rezolver.Register(GenericConstructorTarget.Auto(typeof(Generic2<,>)), typeof(IGeneric2<,>));
			var result = (IGeneric2<int, string>)rezolver.Resolve(typeof(IGeneric2<int, string>));
			Assert.AreEqual(70, result.Value1);
			Assert.AreEqual("hello multiple interface", result.Value2);

			var result2 = (IGeneric2<string, int>)rezolver.Resolve(typeof(IGeneric2<string, int>));
			Assert.AreEqual("hello multiple interface", result2.Value1);
			Assert.AreEqual(70, result2.Value2);
		}

		[TestMethod]
		public void ShouldResolveGenericTypeWith2ReverseParametersByInterface()
		{
			var rezolver = CreateADefaultRezolver();
			rezolver.Register((80).AsObjectTarget());
			rezolver.Register("hello reversed interface".AsObjectTarget());
			//the thing here being that the type parameters for IGeneric2 are swapped in Generic2Reversed,
			//so we're testing that the engine can identify that and map the parameters from IGeneric2
			//back to the type parameters that should be passed to Generic2Reversed
			rezolver.Register(GenericConstructorTarget.Auto(typeof(Generic2Reversed<,>)), typeof(IGeneric2<,>));
			var result = (IGeneric2<int, string>)rezolver.Resolve(typeof(IGeneric2<int, string>));
			Assert.IsInstanceOfType(result, typeof(Generic2Reversed<string, int>));
			Assert.AreEqual(80, result.Value1);
			Assert.AreEqual("hello reversed interface", result.Value2);
		}

		private class GenericImplementsNested<T> : IGeneric<IEnumerable<T>>
		{
			public IEnumerable<T> Value
			{
				get { throw new NotImplementedException(); }
			}
		}

		[TestMethod]
		public void ShouldMapSimpleParameter()
		{
			var mappings = MapGenericParameters(typeof(IGeneric<int>), typeof(Generic<>));

			Assert.IsTrue(new[] { typeof(int) }.SequenceEqual(mappings));
		}

		[TestMethod]
		public void ShouldMapParameterFromNestedInterface()
		{
			var mappings = MapGenericParameters(typeof(IGeneric<IEnumerable<int>>), typeof(GenericImplementsNested<>));
			Type[] expected = new[] { typeof(int) };
			Console.WriteLine("Expected: {0}", string.Join(", ", expected.Select(t => t.ToString())));
			Console.WriteLine("Result: {0}", string.Join(", ", mappings.Select(t => t.ToString())));

			Assert.IsTrue(expected.SequenceEqual(mappings));
		}

		private Type[] MapGenericParameters(Type requestedType, Type targetType)
		{
			var requestedTypeGenericDefinition = requestedType.GetGenericTypeDefinition();
			Type[] finalTypeArguments = targetType.GetGenericArguments();
			var mappedInterface = targetType.GetInterfaces().FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == requestedTypeGenericDefinition);
			if (mappedInterface != null)
			{
				var interfaceTypeParams = mappedInterface.GetGenericArguments();
				var typeParamPositions = targetType
					.GetGenericArguments()
					.Select(t =>{
						var mapping = DeepSearchTypeParameterMapping(null, mappedInterface, t);
							
						//if the mapping is not found, but one or more of the interface type parameters are generic, then 
						//it's possible that one of those has been passed the type parameter.
						//the problem with that, fromm our point of view, however, is how then 
						
						return new
						{
							DeclaredTypeParamPosition = t.GenericParameterPosition,
							Type = t,
							//the projection here allows us to get the index of the base interface's generic type parameter
							//It is required because using the GenericParameterPosition property simply returns the index of the 
							//type in our declared type, as the type is passed down into the interfaces from the open generic
							//but closes them over those very types.  Thus, the <T> from an open generic class Foo<T> is passed down
							//to IFoo<T> almost as if it were a proper type, and the <T> in IFoo<> is actually equal to the <T> from Foo<T>.
							MappedTo = mapping
						};
					}).OrderBy(r => r.MappedTo != null ? r.MappedTo[0] : int.MinValue).ToArray();

				var suppliedTypeArguments = requestedType.GetGenericArguments();
				Type suppliedArg = null;
				foreach (var typeParam in typeParamPositions.Where(p => p.MappedTo != null))
				{
					suppliedArg = suppliedTypeArguments[typeParam.MappedTo[0]];
					foreach(var index in typeParam.MappedTo.Skip(1))
					{
						suppliedArg = suppliedArg.GetGenericArguments()[index];
					}
					finalTypeArguments[typeParam.DeclaredTypeParamPosition] = suppliedArg;
				}
			}
			return finalTypeArguments;
		}

		/// <summary>
		/// returns a series of type parameter indexes from the baseType parameter which can be used to derive
		/// the concrete type parameter to be used in a target type, given a fully-closed generic type as the model
		/// </summary>
		/// <param name="previousTypeParameterPositions"></param>
		/// <param name="candidateTypeParameter"></param>
		/// <param name="targetTypeParameter"></param>
		/// <returns></returns>
		private int[] DeepSearchTypeParameterMapping(Stack<int> previousTypeParameterPositions, Type baseTypeParameter, Type targetTypeParameter)
		{
			if (baseTypeParameter == targetTypeParameter)
				return previousTypeParameterPositions.ToArray();
			if (previousTypeParameterPositions == null)
				previousTypeParameterPositions = new Stack<int>();
			if (baseTypeParameter.IsGenericType)
			{
				var args = baseTypeParameter.GetGenericArguments();
				int[] result = null;
				for (int f = 0; f < args.Length; f++)
				{
					previousTypeParameterPositions.Push(f);
					result = DeepSearchTypeParameterMapping(previousTypeParameterPositions, args[f], targetTypeParameter);
					previousTypeParameterPositions.Pop();
					if (result != null)
						return result;
				}
			}
			return null;
		}
	}
}
