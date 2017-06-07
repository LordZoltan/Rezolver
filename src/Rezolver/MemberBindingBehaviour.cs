﻿using Rezolver.Targets;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rezolver
{
    /// <summary>
    /// Static accessor for the common member binding behaviours provided by Rezolver.
    /// </summary>
    /// <remarks>The <see cref="ConstructorTarget"/> and <see cref="GenericConstructorTarget"/> classes can be provided
    /// with an <see cref="IMemberBindingBehaviour"/> when created.  If one is not set, then instead they will attempt to 
    /// get their behaviour via the options API (see <see cref="OptionsTargetContainerExtensions"/>) from the 
    /// <see cref="ITargetContainer"/> in which they have been registered.
    /// 
    /// The default behaviour, when no option is explicitly set for a particular type, is the <see cref="Default"/>.</remarks>
    public static class MemberBindingBehaviour
    {
        /// <summary>
        /// A behaviour that binds all publicly writeable properties and fields on an object after construction.
        /// </summary>
        /// <remarks>The implementation is an instance of the <see cref="BindAllMembersBehaviour"/></remarks>
        public static IMemberBindingBehaviour BindAll { get; } = new BindAllMembersBehaviour();

        /// <summary>
        /// A behaviour that doesn't bind any properties or fields on an object.
        /// 
        /// This is also the 
        /// </summary>
        /// <remarks>The implementation is an instance of the <see cref="BindNoMembersBehaviour"/></remarks>
        public static IMemberBindingBehaviour BindNone { get; } = new BindNoMembersBehaviour();

        /// <summary>
        /// A behaviour which binds only publicly writeable properties on an object after construction.
        /// </summary>
        public static IMemberBindingBehaviour BindProperties { get; } = new BindPublicPropertiesBehaviour();

        /// <summary>
        /// A behaviour which binds only public fields on an object after construction.
        /// </summary>
        public static IMemberBindingBehaviour BindFields { get; } = new BindPublicFieldsBehaviour();
    }
}
