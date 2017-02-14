﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.Types
{
  public class GenericDecoratingHandler<T> : IHandler<T>
  {
    private readonly IHandler<T> _decorated;

    public GenericDecoratingHandler(IHandler<T> decorated)
    {
      _decorated = decorated;
    }

    public string Handle(T t)
    {
      return $"({ _decorated.Handle(t) }) Decorated";
    }
  }
}
