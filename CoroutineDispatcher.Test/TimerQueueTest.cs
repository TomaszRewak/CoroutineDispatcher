using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoroutineDispatcher.Test
{
	[TestClass]
	public partial class TimerQueueTest
	{
		[TestMethod]
		public void EmptyCollectionHasNothingToDequeue()
		{
			AssertDequeue(0);
		}

		[TestMethod]
		public void EnqueueingSingleOperationTwiceStoresItTwice()
		{
			Action operation = () => { };

			Enqueue(DateTime.MinValue, DispatchPriority.Medium, operation);
			Enqueue(DateTime.MinValue, DispatchPriority.Medium, operation);

			AssertDequeue(2);
		}

		[TestMethod]
		public void DequeuingOperationsRemovesThem()
		{
			Enqueue(DateTime.MinValue, DispatchPriority.Medium);
			Enqueue(DateTime.MinValue, DispatchPriority.Medium);

			AssertDequeue(2);
			AssertDequeue(0);
		}

		[TestMethod]
		public void OnlyPastOperationsAreDequeued()
		{
			Enqueue(DateTime.UtcNow.AddSeconds(-2), DispatchPriority.Medium, 1);
			Enqueue(DateTime.UtcNow.AddSeconds(-1), DispatchPriority.Medium, 2);
			Enqueue(DateTime.UtcNow.AddSeconds(1), DispatchPriority.Medium, 3);
			Enqueue(DateTime.UtcNow.AddSeconds(2), DispatchPriority.Medium, 4);

			AssertDequeue(new[] { 1, 2 });
		}

		[TestMethod]
		public void DequeuingIsNotImpactedByEnqueuingOrder()
		{
			Enqueue(DateTime.UtcNow.AddSeconds(-2), DispatchPriority.Medium, 1);
			Enqueue(DateTime.UtcNow.AddSeconds(1), DispatchPriority.Medium, 2);
			Enqueue(DateTime.UtcNow.AddSeconds(2), DispatchPriority.Medium, 3);
			Enqueue(DateTime.UtcNow.AddSeconds(-1), DispatchPriority.Medium, 4);

			AssertDequeue(new[] { 1, 4 });
		}
	}
}
