﻿// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rezolver.Compilation;

namespace Rezolver
{
	/// <summary>
	/// Describes a type which discovers and creates property/field bindings, typically for use when creating a new instance.
	/// </summary>
	/// <remarks>The default implementation of this is <see cref="DefaultMemberBindingBehaviour"/>, which always binds all publicly
	/// writable instance properties and publicly accessible instance fields to auto-generated <see cref="RezolvedTarget"/> targets.</remarks>
	public interface IMemberBindingBehaviour
	{
		/// <summary>
		/// Retrieves the property and/or field bindings for the given <paramref name="type"/> based on the given <paramref name="context"/>.
		/// </summary>
		/// <param name="context">The current compilation context (will be used to look up <see cref="ITarget"/> references from its
		/// implementation of <see cref="ITargetContainer"/></param>
		/// <param name="type">The type whose writable members are to be probed.</param>
		/// <returns>Zero or more bindings for the members of the <paramref name="type"/>.</returns>
		MemberBinding[] GetMemberBindings(ICompileContext context, Type type);
	}

}