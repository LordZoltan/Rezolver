﻿// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver
{
	/// <summary>
	/// An <see cref="ITargetContainer"/> that stores multiple targets in a list.
	/// 
	/// This is not a type that you would typically use directly in your code, unless you are writing 
	/// custom logic/behaviour for <see cref="ITarget"/> registration.
	/// </summary>
	/// <remarks>
	/// This type is not thread-safe, nor does it perform any type checking on the targets
	/// that are added to it.
	/// </remarks>
	public class TargetListContainer : ITargetContainer, IList<ITarget>
	{
		private List<ITarget> _targets;

		/// <summary>
		/// Gets the type against which this list container is registered in its <see cref="ITargetContainer"/>.
		/// </summary>
		public Type RegisteredType { get; }

		/// <summary>
		/// Gets the default target for this list - which will always be the last target added to the list, or 
		/// <c>null</c> if no targets have been added yet.
		/// </summary>
		public ITarget DefaultTarget
		{
			get
			{
				if (_targets.Count == 0) return null;

				return _targets[_targets.Count - 1];
			}
		}

        /// <summary>
		/// Gets the number of targets which have been added to the list.
		/// </summary>
		/// <value>The count.</value>
		public int Count { get { return _targets.Count; } }

        bool ICollection<ITarget>.IsReadOnly => ((IList<ITarget>)_targets).IsReadOnly;

        ITarget IList<ITarget>.this[int index] { get => ((IList<ITarget>)_targets)[index]; set => ((IList<ITarget>)_targets)[index] = value; }

        private ITargetContainer Root { get; }
        private bool AllowMultiple { get; }
        private bool CanAdd => AllowMultiple || Count == 0;

        

        /// <summary>
        /// Initializes a new instance of the <see cref="TargetListContainer"/> class.
        /// </summary>
        /// <param name="root">The root target container in which this container is registered.</param>
        /// <param name="registeredType">Required - the type against which this list will be registered.</param>
        /// <param name="targets">Optional array of targets with which to initialise the list.</param>
        public TargetListContainer(ITargetContainer root, Type registeredType, params ITarget[] targets)
		{
            Root = root ?? throw new ArgumentNullException(nameof(root));
			RegisteredType = registeredType ?? throw new ArgumentNullException(nameof(registeredType));
            AllowMultiple = Root.GetOption(registeredType, Options.AllowMultiple.Default);

            if (AllowMultiple || targets?.Length <= 1)
                _targets = new List<ITarget>(targets ?? new ITarget[0]);
            else
                throw new ArgumentException($"Too many targets provided - only one target can be registered for the type { registeredType }", nameof(targets));
        }

		/// <summary>
		/// Registers the specified target into the list.  Note - the target is not checked to see
		/// if it supports this list's <see cref="RegisteredType"/>.
		/// </summary>
		/// <param name="target">The target.</param>
		/// <param name="registeredType">Ignored.</param>
		public virtual void Register(ITarget target, Type registeredType = null)
		{
			Add(target ?? throw new ArgumentNullException(nameof(target)));
		}

		/// <summary>
		/// Always returns the <see cref="DefaultTarget"/>
		/// </summary>
		/// <param name="type">Ignored.</param>
		public virtual ITarget Fetch(Type type)
		{
			return DefaultTarget;
		}

		/// <summary>
		/// Retrieves an enumerable of all targets that have been registered to this list.
		/// </summary>
		/// <param name="type">Ignored.</param>
		public virtual IEnumerable<ITarget> FetchAll(Type type)
		{
			return this.AsReadOnly();
		}

		/// <summary>
		/// Not supported.
		/// </summary>
		/// <param name="existing">Ignored</param>
		/// <param name="type">Ignored.</param>
		/// <exception cref="NotSupportedException">Always</exception>
		public virtual ITargetContainer CombineWith(ITargetContainer existing, Type type)
		{
            // clearly - we could actually do this - if the other container is a list, too, we
            // could merge its targets into this one and return this one.
			throw new NotSupportedException();
		}

        /// <summary>
        /// Not supported by this target container.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public ITargetContainer FetchContainer(Type type)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Not supported by this target container
        /// </summary>
        /// <param name="type"></param>
        /// <param name="container"></param>
        public void RegisterContainer(Type type, ITargetContainer container)
        {
            throw new NotSupportedException();
        }

        private void IfCanAdd(Action action)
        {
            if (CanAdd)
                action();
            else
                throw new InvalidOperationException($"Only one target can be registered for the type { RegisteredType }");
        }

        public int IndexOf(ITarget item)
        {
            return ((IList<ITarget>)_targets).IndexOf(item);
        }

        public void Insert(int index, ITarget item)
        {
            IfCanAdd(() => ((IList<ITarget>)_targets).Insert(index, item));
        }

        public void RemoveAt(int index)
        {
            ((IList<ITarget>)_targets).RemoveAt(index);
        }

        public void Add(ITarget item)
        {
            IfCanAdd(() => ((IList<ITarget>)_targets).Add(item));
        }

        public void Clear()
        {
            ((IList<ITarget>)_targets).Clear();
        }

        public bool Contains(ITarget item)
        {
            return ((IList<ITarget>)_targets).Contains(item);
        }

        public void CopyTo(ITarget[] array, int arrayIndex)
        {
            ((IList<ITarget>)_targets).CopyTo(array, arrayIndex);
        }

        public bool Remove(ITarget item)
        {
            return ((IList<ITarget>)_targets).Remove(item);
        }

        public IEnumerator<ITarget> GetEnumerator()
        {
            return ((IList<ITarget>)_targets).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IList<ITarget>)_targets).GetEnumerator();
        }
	}
}
