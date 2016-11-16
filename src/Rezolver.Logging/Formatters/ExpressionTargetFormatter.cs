﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Logging.Formatters
{
	[ObjectFormatter]
    public class ExpressionTargetFormatter : ObjectFormatter<ExpressionTarget>
    {
		public override string Format(ExpressionTarget obj, string format = null, ObjectFormatterCollection formatters = null)
		{
			if (obj.Expression != null)
				return string.Format("{{ Expression: {0} }}", obj.Expression);
			else
				return string.Format("{{ Expression Factory, Type = {0} }}", obj.DeclaredType);
		}
	}
}