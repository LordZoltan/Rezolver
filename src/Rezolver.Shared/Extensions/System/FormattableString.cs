﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#if NET451 || NETSTANDARD1_1
//SHIM FOR FormattableStringFactory and FormattableString for Net451 builds
//Courtesy of http://stackoverflow.com/a/32077216/157701
namespace System.Runtime.CompilerServices
{
	internal class FormattableStringFactory
	{
		public static FormattableString Create(string messageFormat, params object[] args)
		{
			return new FormattableString(messageFormat, args);
		}
	}
}

namespace System
{
	internal class FormattableString : IFormattable
	{
		private readonly string messageFormat;
		private readonly object[] args;

		public FormattableString(string messageFormat, object[] args)
		{
			this.messageFormat = messageFormat;
			this.args = args;
		}

		public override string ToString()
		{
			return string.Format(messageFormat, args);
		}

		public string ToString(string format, IFormatProvider formatProvider)
		{
			return string.Format(formatProvider, format ?? messageFormat, args);
		}

		public string ToString(IFormatProvider formatProvider)
		{
			return string.Format(formatProvider, messageFormat, args);
		}
	}
}
#endif