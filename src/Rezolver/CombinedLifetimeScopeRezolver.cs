using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Collections.Concurrent;

namespace Rezolver
{
    //TODO: reimplement this as a combined rezolver - there's no need to override caching rezolver any more.
    public class CombinedLifetimeScopeRezolver : CombinedRezolver, ILifetimeScopeRezolver
    {
        private ConcurrentBag<ILifetimeScopeRezolver> _children;
        private ConcurrentDictionary<RezolveContext, ConcurrentBag<object>> _objects;

        public ILifetimeScopeRezolver ParentScope
        {
            get
            {
                return _parentScope;
            }
        }

        private bool _disposed;
        private readonly ILifetimeScopeRezolver _parentScope;
        public CombinedLifetimeScopeRezolver(IRezolver inner, IRezolverBuilder builder = null, IRezolveTargetCompiler compiler = null)
            : base(inner, builder, compiler)
        {
            _children = new ConcurrentBag<ILifetimeScopeRezolver>();
            _objects = new ConcurrentDictionary<RezolveContext, ConcurrentBag<object>>();
            _disposed = false;
        }

        public CombinedLifetimeScopeRezolver(ILifetimeScopeRezolver parentScope, IRezolver rezolver = null)
            : this(rezolver ?? (IRezolver)parentScope)
        {
            _parentScope = parentScope;
        }

        private void TrackObject(object obj, RezolveContext context)
        {
            if (obj == null)
                return;
            ConcurrentBag<object> instances = _objects.GetOrAdd(
                new RezolveContext(null, context.RequestedType, context.Name), 
                c => new ConcurrentBag<object>());

            //bit slow this, but hopefully there won't be loads of them...
            //note that there'll be a memory overhead with this, certainly in portable,
            //as under the hood the internal implementation realises the enumerable as an array
            //before returning its enumerator.
            if (!instances.Any(o => object.ReferenceEquals(o, obj)))
                instances.Add(obj);
        }

        public override ILifetimeScopeRezolver CreateLifetimeScope()
        {
            var toReturn = new CombinedLifetimeScopeRezolver(this);
            _children.Add(toReturn);
            return toReturn;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                ILifetimeScopeRezolver child = null;
                while(_children.TryTake(out child))
                {
                    child.Dispose();
                }

                object obj = null;
                IDisposable disposableObj = null;
                foreach (var kvp in _objects)
                {
                    while(kvp.Value.TryTake(out obj))
                    {
                        disposableObj = obj as IDisposable;
                        if (disposableObj != null)
                            disposableObj.Dispose();
                    }
                }
                _objects.Clear();
            }
            _disposed = true;
        }

        public void AddToScope(object obj, RezolveContext context = null)
        {
            obj.MustNotBeNull("obj");
            TrackObject(obj, context ?? new RezolveContext(null, obj.GetType()));
        }

        public IEnumerable<object> GetFromScope(RezolveContext context)
        {
            context.MustNotBeNull("context");
            ConcurrentBag<object> instances = null;
            if (_objects.TryGetValue(context, out instances))
            {
                //important to return a read-only collection here to avoid modification
                return new ReadOnlyCollection<object>(instances.ToArray());
            }
            else
                return Enumerable.Empty<object>();
        }
    }


}