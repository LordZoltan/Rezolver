﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Rezolver
{
	public class PropertyOrFieldBinding
	{
		public static readonly PropertyOrFieldBinding[] None = new PropertyOrFieldBinding[0];

		public MemberInfo Member { get; private set; }

		public IRezolveTarget Target { get; private set; }

		public Type MemberType
		{
			get
			{
				PropertyInfo p = Member as PropertyInfo;
				if (p != null)
					return p.PropertyType;
				else
					return ((FieldInfo)Member).FieldType;
			}
		}

		public PropertyOrFieldBinding(MemberInfo member, IRezolveTarget target)
		{
			Member = member;
			Target = target;
		}

		/// <summary>
		/// Static factory method that creates bindings for all publicly writable instance properties (and, optionally, fields) of the given type.
        /// Each property/field is bound to a <see cref="RezolvedTarget"/> instance - meaning that, at runtime, values for those properties or fields 
        /// will be resolved from the container by type.
		/// </summary>
		/// <param name="type">The type whose properties (and, optionally, publicly writable fields) are to be bound.</param>
        /// <param name="includeFields">If true, then publicly writable fields will be bound.</param>
		/// <returns></returns>
		public static PropertyOrFieldBinding[] DeriveAutoPropertyBinding(Type type, bool includeFields = false)
		{
			//note - the canwrite and GetSetMethod check in the predicate 
			var memberBindings = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
				.Where(p => p.CanWrite && p.GetSetMethod() != null)
				.Select(p => new PropertyOrFieldBinding(p, new RezolvedTarget(p.PropertyType)));
			if(includeFields)
			{
				memberBindings = memberBindings.Concat(type.GetFields(BindingFlags.Instance | BindingFlags.Public)
					.Select(m => new PropertyOrFieldBinding(m, new RezolvedTarget(m.FieldType))));
			}

			return memberBindings.ToArray();
		}

        /// <summary>
        /// Method for creating a Linq Expression MemberBinding object for the <see cref="Member"/>, to the expression produced by 
        /// the <see cref="Target"/> object's <see cref="IRezolveTarget.CreateExpression(CompileContext)"/> method.
        /// </summary>
        /// <param name="context">The <see cref="CompileContext"/> under which the generated expression will be compiled.</param>
        /// <returns></returns>
		public MemberBinding CreateMemberBinding(CompileContext context)
		{
			return Expression.Bind(Member, Target.CreateExpression(new CompileContext(context, MemberType, true)));
		}
	}
}
