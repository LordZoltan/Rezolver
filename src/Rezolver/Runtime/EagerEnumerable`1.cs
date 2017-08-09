﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver.Runtime
{
    /// <summary>
    /// Direct implementation of <see cref="IEnumerable{T}"/> for eagerly loaded enumerables when <see cref="Options.LazyEnumerables"/>
    /// has been disabled either globally, or for a specific enumerable's element type.
    /// 
    /// Rezolver uses this type instead of an array to prevent casting and modifying the contents of the enumerable.
    /// </summary>
    /// <remarks>See the remarks section on <see cref="LazyEnumerable{T}"/> for more about lazy and eager enumerables.</remarks>
    public class EagerEnumerable<T> : IEnumerable<T>
    {
        private readonly T[] _items;

        /// <summary>
        /// Constructs a new instance of <see cref="EagerEnumerable{T}"/>
        /// </summary>
        /// <param name="items"></param>
        public EagerEnumerable(T[] items)
        {
            _items = items;
        }

        /// <summary>
        /// Implementation of <see cref="IEnumerable{T}.GetEnumerator"/>
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator()
        {
            return (_items ?? (Enumerable.Empty<T>())).GetEnumerator();
        }


        /// <summary>
        /// Implementation of <see cref="IEnumerable.GetEnumerator"/>
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
