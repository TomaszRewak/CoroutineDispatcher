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
		public void EnqueueingSingleOperationTwiceStoresItTwice()
		{
			Action operation = () => { };

			Enqueue(operation);
			Enqueue(operation);

			AssertDequeue();
			AssertDequeue();
			AssertFailDequeue();
		}
	}
}
