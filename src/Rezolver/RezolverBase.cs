﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver
{
	public abstract class RezolverBase : IRezolver
	{
		private static readonly
			Dictionary<Type, ICompiledRezolveTarget>
			MissingTargets = new Dictionary<Type, ICompiledRezolveTarget>();
		


		protected static ICompiledRezolveTarget GetMissingTarget(Type target)
		{
			ICompiledRezolveTarget result = null;

			if (MissingTargets.TryGetValue(target, out result))
				return result;

			return MissingTargets[target] = new MissingCompiledTarget(target);
		}

		protected static bool IsMissingTarget(ICompiledRezolveTarget target)
		{
			return target is MissingCompiledTarget;
		}

		protected class MissingCompiledTarget : ICompiledRezolveTarget
		{
			private readonly Type _type;

			public MissingCompiledTarget(Type type)
			{
				_type = type;
			}

			public object GetObject(RezolveContext context)
			{
				throw new InvalidOperationException(String.Format("Could not resolve type {0}", _type));
			}
		}

		protected RezolverBase()
		{
		}

		public abstract IRezolveTargetCompiler Compiler { get; }

		public abstract IRezolverBuilder Builder { get; }

		public virtual object Resolve(RezolveContext context)
		{
			return GetCompiledRezolveTarget(context).GetObject(context);
		}

		public virtual bool TryResolve(RezolveContext context, out object result)
		{
			var target = GetCompiledRezolveTarget(context);
			if (!IsMissingTarget(target))
			{
				result = target.GetObject(context);
				return true;
			}
			else
			{
				result = null;
				return false;
			}
		}

		public virtual ILifetimeScopeRezolver CreateLifetimeScope()
		{
			return new LifetimeScopeRezolver(this);
		}

		public ICompiledRezolveTarget FetchCompiled(RezolveContext context)
		{
			//note that this rezolver is fixed as the rezolver in the compile context - regardless of the
			//one passed in.  This is important.
			//note also that this rezolver is only passed as scope if the context doesn't already have one.
			return GetCompiledRezolveTarget(context.CreateNew(this, context.Scope == null ? this as ILifetimeScopeRezolver : context.Scope));
		}

		public virtual bool CanResolve(RezolveContext context)
		{
			//TODO: Change this to refer to the cache (once I've figured out how to do it based on the new compiler)
			return Builder.Fetch(context.RequestedType, context.Name) != null;
		}

		protected virtual ICompiledRezolveTarget GetCompiledRezolveTarget(RezolveContext context)
		{
			IRezolveTarget target = Builder.Fetch(context.RequestedType, context.Name);

			if (target != null)
			{
				//note that if a name was passed we're grabbing the best matching named builder to use for resolving
				//dependencies.
				return Compiler.CompileTarget(target,
					new CompileContext(this, 
						context.RequestedType, 
						dependencyBuilder: context.Name != null ? Builder.GetBestNamedBuilder(context.Name) : null));
			}

			return GetFallbackCompiledRezolveTarget(context);
		}

		protected virtual ICompiledRezolveTarget GetFallbackCompiledRezolveTarget(RezolveContext context)
		{
			return GetMissingTarget(context.RequestedType);
		}

		object IServiceProvider.GetService(Type serviceType)
		{
			//IServiceProvider should return null if not found - so we use TryResolve.
			object toReturn = null;
			this.TryResolve(serviceType, out toReturn);
			return toReturn;
		}
	}
}