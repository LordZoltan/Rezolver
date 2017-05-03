﻿using System;
using System.Collections.Generic;
using System.Text;
using Rezolver.Compilation;

namespace Rezolver
{
    /// <summary>
    /// This is the default <see cref="IMemberBindingBehaviour"/> which doesn't bind any members.  It's
    /// a singleton accessible only via the <see cref="MemberBindingBehaviour.BindNone"/> static property.
    /// </summary>
    public sealed class BindNoMembersBehaviour : IMemberBindingBehaviour
    {
        private static MemberBinding[] NoBindings = new MemberBinding[0];

        /// <summary>
        /// The one and only instance of the <see cref="BindNoMembersBehaviour"/>
        /// </summary>
        internal static BindNoMembersBehaviour Instance { get; } = new BindNoMembersBehaviour();
        private BindNoMembersBehaviour() { }

        /// <summary>
        /// Implementation of <see cref="IMemberBindingBehaviour.GetMemberBindings(ICompileContext, Type)"/>
        /// 
        /// Always returns an empty array.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public MemberBinding[] GetMemberBindings(ICompileContext context, Type type)
        {
            return NoBindings;
        }
    }
}