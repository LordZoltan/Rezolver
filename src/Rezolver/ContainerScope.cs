﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver
{
	/// <summary>
	/// Standard implementation of the <see cref="IContainerScope"/> interface.
	/// </summary>
	/// <seealso cref="Rezolver.IContainerScope" />
	public class ContainerScope : IContainerScope
	{
        /// <summary>
        /// Explicitly scoped objects can be a mixture of disposable and non-disposable objects
        /// </summary>
        //private ConcurrentDictionary<IResolveContext, Lazy<object>> _explicitlyScopedObjects;
        private ConcurrentDictionary<Type, Lazy<object>> _explicitlyScopedObjects;

        /// <summary>
        /// implicitly scoped objects will always be IDisposable
        /// </summary>
        private ConcurrentBag<IDisposable> _implicitlyScopedObjects
			= new ConcurrentBag<IDisposable>();
		private LockedList<IContainerScope> _childScopes
			= new LockedList<IContainerScope>();

		private bool _disposed = false;
		private bool _disposing = false;

		/// <summary>
		/// Gets a value indicating whether this <see cref="ContainerScope"/> is disposed.
		/// </summary>
		/// <value><c>true</c> if disposed; otherwise, <c>false</c>.</value>
		public bool Disposed { get { return _disposed; } }

		private readonly IContainer _container;
		/// <summary>
		/// The container that this scope is tied to.  All standard resolve operations
		/// should be made against this container.
		/// </summary>
		/// <value>The container.</value>
		public IContainer Container
		{
			get
			{
				return _container ?? Parent.Container;
			}
		}

		/// <summary>
		/// If this scope has a parent scope, this is it.
		/// </summary>
		/// <value>The parent.</value>
		public IContainerScope Parent
		{
			get;
		}

        private void InitScopeContainers()
        {
            //_explicitlyScopedObjects = new ConcurrentDictionary<IResolveContext, Lazy<object>>(ResolveContext.RequestedTypeComparer);
            _explicitlyScopedObjects = new ConcurrentDictionary<Type, Lazy<object>>();
            _implicitlyScopedObjects = new ConcurrentBag<IDisposable>();
            _childScopes = new LockedList<IContainerScope>();
        }

        private void FreeScopeContainers()
        {
            _explicitlyScopedObjects = null;
            _implicitlyScopedObjects = null;
            _childScopes = null;
        }

        private ContainerScope()
        {
            InitScopeContainers();
        }
		/// <summary>
		/// Creates a new container that is a child of another.
		/// 
		/// The <see cref="Container"/> will be inherited from the <paramref name="parentScope"/>
		/// by default, unless it's overriden by <paramref name="containerOverride"/>.
		/// </summary>
		/// <param name="parentScope">Required - the parent scope</param>
		/// <param name="containerOverride">Optional - the container which should be used for resolve
		/// operations executed against this scope (note - all the resolve methods are declared as extension
		/// methods which mirror those present on <see cref="IContainer"/>.</param>
		public ContainerScope(IContainerScope parentScope, IContainer containerOverride = null)
            : this()
		{
			parentScope.MustNotBeNull(nameof(parentScope));
			Parent = parentScope;
			if (containerOverride != null)
				_container = containerOverride;
		}

		/// <summary>
		/// Creates a new root scope tied to the given <paramref name="container"/>
		/// </summary>
		/// <param name="container"></param>
		public ContainerScope(IContainer container)
            : this()
		{
			container.MustNotBeNull(nameof(container));

			_container = container;
		}

		/// <summary>
		/// Called to create a child scope from this scope.  The implementation adds the
		/// new scope to a private collection so that it can dispose of the new child if
		/// it is not already disposed.
		/// </summary>
		public IContainerScope CreateScope()
		{
            if (Disposed) throw new ObjectDisposedException("ContainerScope", "This scope has been disposed");

            var newScope = new ContainerScope(this);
			_childScopes.Add(newScope);
			return newScope;
		}

		/// <summary>
		/// Called by child scopes when they are disposed to notify the parent that they
		/// will no longer need to be disposed of when the parent is disposed.
		/// </summary>
		/// <param name="child">The child.</param>
		/// <remarks>This is an infrastructure method and not something you would usually need to call.
		/// It's exposed for developers who are extending the container scoping functionality only.</remarks>
		public void ChildScopeDisposed(IContainerScope child)
		{
			if (!_disposing)
			{
				_childScopes.Remove(child);
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources.
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing)
		{
			//have to cope with recursion below because of the complexity of some of the disposable
			//scenarios we might have to deal with.
			if (Disposed || _disposing) return;

			if (disposing)
			{
				_disposing = true;
				try
				{
					using (var listLock = _childScopes.Lock())
					{
						//dispose all child scopes first
						foreach (var scope in _childScopes)
						{
							scope.Dispose();
						}
					}

					//note that explicitly scoped objects might not actually be IDisposable :)
                    
					var allExplicitObjects = _explicitlyScopedObjects.Skip(0).ToArray()
						.Select(l => l.Value.Value).OfType<IDisposable>();
					var allImplicitObjects = _implicitlyScopedObjects.Skip(0).ToArray();

                    //deref all used collections 
                    FreeScopeContainers();

					foreach (var obj in allExplicitObjects.Concat(allImplicitObjects))
					{
						obj.Dispose();
					}

					
				}
				finally
				{
					_disposed = true;
					_disposing = false;
				}
			}
		}

		object IContainerScope.Resolve(IResolveContext context, Func<IResolveContext, object> factory, ScopeBehaviour behaviour)
		{
			if (Disposed) throw new ObjectDisposedException("ContainerScope", "This scope has been disposed");

			if (behaviour == ScopeBehaviour.Explicit)
			{
                if (_explicitlyScopedObjects.TryGetValue(context.RequestedType, out Lazy<object> lazy))
                    return lazy.Value;

                //use a lazily evaluated object which is bound to this resolve context to ensure only one instance is created
                return _explicitlyScopedObjects.GetOrAdd(context.RequestedType, k => new Lazy<object>(() => factory(context))).Value;
			}
			else if (behaviour == ScopeBehaviour.Implicit)
			{
				var result = factory(context);
				if (result is IDisposable) _implicitlyScopedObjects.Add((IDisposable)result);
				return result;
			}
			return factory(context);
		}

		object IServiceProvider.GetService(Type serviceType)
		{
            if (Disposed) throw new ObjectDisposedException("ContainerScope", "This scope has been disposed");

            object toReturn = null;
			Container.TryResolve(new ResolveContext(this, serviceType), out toReturn);
			return toReturn;
		}
	}
}
