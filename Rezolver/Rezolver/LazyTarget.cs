﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver
{
	public class LazyTarget : IRezolveTarget
	{
		private readonly Lazy<object> _lazyTarget;
		private IRezolveTarget _innerTarget;

		public LazyTarget(IRezolveTarget innerTarget)
		{
			innerTarget.MustNotBeNull("innerTarget");
			// TODO: Complete member initialization
			this._innerTarget = innerTarget;
			_lazyTarget = new Lazy<object>(() => _innerTarget.GetObject());
		}

		public bool SupportsType(Type type)
		{
			return _innerTarget.SupportsType(type);
		}

		public object GetObject()
		{
			return _lazyTarget.Value;
		}

		public Type DeclaredType
		{
			get { return _innerTarget.DeclaredType; }
		}
	}
}
