using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CoroutineDispatcher
{
	internal sealed class CoroutineSynchronizationContext : SynchronizationContext
	{
		public CoroutineSynchronizationContext()
		{
		}

		public override void Post(SendOrPostCallback d, object state)
		{
			base.Post(d, state);
		}

		public override void Send(SendOrPostCallback d, object state)
		{
			base.Send(d, state);
		}

		public override void OperationStarted()
		{
			base.OperationStarted();
		}

		public override void OperationCompleted()
		{
			base.OperationCompleted();
		}

		public override int Wait(IntPtr[] waitHandles, bool waitAll, int millisecondsTimeout)
		{
			return base.Wait(waitHandles, waitAll, millisecondsTimeout);
		}
	}
}
