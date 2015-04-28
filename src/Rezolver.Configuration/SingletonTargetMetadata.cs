﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver.Configuration
{
	public class SingletonTargetMetadata : RezolveTargetMetadataBase, Rezolver.Configuration.ISingletonTargetMetadata
	{
		/// <summary>
		/// If true, then the singleton object should be scope-compatible, i.e. with a lifetime limited
		/// to the lifetime of an external scope rather than to the AppDomain's lifetime.
		/// </summary>
		public bool Scoped { get; private set; }

		/// <summary>
		/// Metadata for the inner target that is turned into a scoped singleton
		/// </summary>
		public IRezolveTargetMetadata Inner { get; private set; }

		/// <summary>
		/// Gets the declared type of the object that will be created by an IRezolveTarget created by
		/// this metadata.  Note - this isn't always known, or always fixed, since configuration systems
		/// will allow developers to avoid being specific about the types that are to be built.
		/// </summary>
		/// <value>The type of the declared.</value>
		public override ITypeReference DeclaredType
		{
			get { return Inner.DeclaredType; }
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SingletonTargetMetadata"/> class.
		/// </summary>
		/// <param name="inner">The inner target that will be turned into a singleton.</param>
		/// <param name="scoped">if set to <c>true</c> [scoped].</param>
		/// <exception cref="System.ArgumentNullException">inner</exception>
		public SingletonTargetMetadata(IRezolveTargetMetadata inner, bool scoped = false)
			: base(RezolveTargetMetadataType.Singleton)
		{
			if (inner == null) throw new ArgumentNullException("inner");
			Inner = inner;
			Scoped = scoped;
		}

		/// <summary>
		/// Creates a new instance (clone) of this metadata that's bound to the target types (although, typically, you'll only ever pick the first target type that
		/// is in the array).  Only ever called if <see cref="DeclaredType" /> is unbound
		/// </summary>
		/// <param name="targetTypes">The target types.</param>
		/// <returns>IRezolveTargetMetadata.</returns>
		protected override IRezolveTargetMetadata BindBase(ITypeReference[] targetTypes)
		{
			return new SingletonTargetMetadata(Inner.Bind(targetTypes), Scoped);
		}

		/// <summary>
		/// Called by <see cref="CreateRezolveTarget" /> to create the rezolve target that will be registered into the <see cref="IRezolverBuilder" />
		/// currently being built (available on the <paramref name="context" />)
		/// If an error occurs, you indicate that by adding to the <paramref name="context" />'s errors collection, and return null.
		/// You can also throw an exception, which will be caught and added to the errors collection for you.
		/// </summary>
		/// <param name="targetTypes">The target types.</param>
		/// <param name="context">The context.</param>
		/// <param name="entry">The entry.</param>
		/// <returns>IRezolveTarget.</returns>
		protected override IRezolveTarget CreateRezolveTargetBase(Type[] targetTypes, ConfigurationAdapterContext context, IConfigurationEntry entry)
		{
			return Scoped ? (IRezolveTarget)new ScopedSingletonTarget(Inner.CreateRezolveTarget(targetTypes, context, entry))
				: new SingletonTarget(Inner.CreateRezolveTarget(targetTypes, context, entry));
		}
	}
}