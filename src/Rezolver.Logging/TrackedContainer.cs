﻿// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Rezolver.Compilation;

namespace Rezolver.Logging
{
  /// <summary>
  /// Logging version of <see cref="Container"/>
  /// 
  /// All method calls are logged (start/end/result/exception)
  /// </summary>
  public class TrackedContainer : Container
  {
    private readonly int _id = TrackingUtils.NextID<TrackedContainer>();

    public override string ToString()
    {
      return $"(#{_id} {GetType().Name})";
    }

    protected internal ICallTracker Tracker { get; private set; }


    public TrackedContainer(ICallTracker logger, ITargetContainer builder = null, ICompilerConfigurationProvider compilerConfig = null) :
      base(targets: builder, compilerConfig: compilerConfig)
    {
      Tracker = logger;
    }

    public override bool CanResolve(ResolveContext context)
    {
      return Tracker.TrackCall(this, () => base.CanResolve(context), context);
    }

    public override IScopedContainer CreateLifetimeScope()
    {
      return Tracker.TrackCall(this, () => new TrackedOverridingScopedContainer(Tracker, null, this));
    }

    public override ICompiledTarget FetchCompiled(ResolveContext context)
    {
      return Tracker.TrackCall(this, () => base.FetchCompiled(context), new { context = context });
    }

    protected override object GetService(Type serviceType)
    {
      return Tracker.TrackCall(this, () => base.GetService(serviceType), new { serviceType = serviceType });
    }

    public override object Resolve(ResolveContext context)
    {
      return Tracker.TrackCall(this, () => base.Resolve(context), new { context = context });
    }

    public override bool TryResolve(ResolveContext context, out object result)
    {
      object tempResult = null;
      var @return = Tracker.TrackCall(this, () => base.TryResolve(context, out tempResult), new { context = context });
      result = tempResult;
      return @return;
    }

    protected override ICompiledTarget GetCompiledRezolveTarget(ResolveContext context)
    {
      return Tracker.TrackCall(this, () => base.GetCompiledRezolveTarget(context), new { context = context });
    }

    protected override ICompiledTarget GetFallbackCompiledRezolveTarget(ResolveContext context)
    {
      return Tracker.TrackCall(this, () => base.GetFallbackCompiledRezolveTarget(context), new { context = context });
    }
  }
}
