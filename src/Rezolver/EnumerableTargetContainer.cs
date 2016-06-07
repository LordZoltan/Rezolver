﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver
{
	internal class EnumerableTargetContainer : GenericTargetContainer
	{
		ITargetContainer _parent;
		public EnumerableTargetContainer(ITargetContainer parent) : base(typeof(IEnumerable<>))
		{
			_parent = parent;
		}

		public override ITarget Fetch(Type type)
		{
			if (!TypeHelpers.IsGenericType(type))
				throw new ArgumentException("Only IEnumerable<T> is supported by this container", nameof(type));
			Type genericType = type.GetGenericTypeDefinition();
			if (genericType != typeof(IEnumerable<>))
				throw new ArgumentException("Only IEnumerable<T> is supported by this container", nameof(type));
			//we allow for specific IEnumerable<T> registrations
			var result = base.Fetch(type);

			if (result != null)
				return result;

			var enumerableType = TypeHelpers.GetGenericArguments(type)[0];

			var targets = _parent.FetchAll(enumerableType);
			return new ListTarget(enumerableType, targets, true);
		}
	}

	public static class EnumerableTargetBuilderExtensions
	{

		public static void EnableEnumerableResolving(this Builder builder)
		{
			builder.MustNotBeNull(nameof(builder));
			builder.RegisterContainer(typeof(IEnumerable<>), new EnumerableTargetContainer(builder));
		}
	}
}
