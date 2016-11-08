﻿using Rezolver.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests
{
	public class RezolverLoggerTests
	{
		private class TestRezolverBase : IContainer
		{
			public virtual ITargetContainer Targets
			{
				get
				{
					return null;
				}
			}

			public virtual ITargetCompiler Compiler
			{
				get
				{
					return null;
				}
			}

			public virtual bool CanResolve(RezolveContext context)
			{
				return false;
			}

			public virtual IScopedContainer CreateLifetimeScope()
			{
				return null;
			}

			public virtual ICompiledTarget FetchCompiled(RezolveContext context)
			{
				return null;
			}

			public virtual object GetService(Type serviceType)
			{
				return null;
			}

			public virtual object Resolve(RezolveContext context)
			{
				return null;
			}

			public virtual bool TryResolve(RezolveContext context, out object result)
			{
				result = null;
				return false;
			}
		}

		private class TestLogger : ICallTracker
		{
			private int _lastReqID = 0;

			public int RequestCount
			{
				get
				{
					return _lastReqID;
				}
			}
			public void CallEnd(int reqId)
			{
				Console.WriteLine($"CallEnd called for reqId {reqId}");
			}

			public void CallResult<TResult>(int reqId, TResult result)
			{
				Console.WriteLine($"CallResult called for reqId {reqId}, result: {(result == null ? "null" : result.ToString())}");
			}

			public int CallStart(object callee, object arguments, [CallerMemberName] string method = null)
			{
				int thisReqId = ++_lastReqID;
				Console.WriteLine($"CallStart called on object {callee} for method {method}. Arguments? {(arguments != null ? "Yes" : "No")}.  ReqId: {thisReqId}");
				return thisReqId;
			}

			public void Exception(int reqId, Exception ex)
			{
				Console.WriteLine($"Exception called for reqId {reqId}.  Exception: { ex?.Message }");
			}

			public TrackedCall GetCall(int callID)
			{
				throw new NotImplementedException();
			}

			public TrackedCallMessage Message(int callID, string message, MessageType messageType = MessageType.Information)
			{
				throw new NotImplementedException();
			}
		}

		private class RequiresInt
		{
			public int IntValue { get; private set; }
			public RequiresInt(int intValue)
			{
				IntValue = intValue;
			}
		}

		[Fact]
		public void ShouldRecordRezolverCall()
		{
			var logger = new TestLogger();
			var loggingRezolver = new TrackedContainer(logger);

			try
			{
				loggingRezolver.Resolve(typeof(int));
			}
			catch (InvalidOperationException) { /* expected - no registrations */ }

			Assert.NotEqual(0, logger.RequestCount);
		}

		[Fact]
		public void ShouldWorkWithDefaultRezolver()
		{
			//could use a more elaborate test here, but the most important thing is that the rezolve operation 
			//works and that nothing throws an exception.  I'm happy with that as a test.  For now.

			//If bugs do start to show up 
			var logger = new TestLogger();
			var loggingRezolver = new TrackedContainer(logger);

			loggingRezolver.RegisterObject(10);
			loggingRezolver.RegisterType<RequiresInt>();

			var result = loggingRezolver.Resolve<RequiresInt>();

			using (var scope = loggingRezolver.CreateLifetimeScope())
			{
				var result2 = scope.Resolve<RequiresInt>();
			}
		}
	}
}
