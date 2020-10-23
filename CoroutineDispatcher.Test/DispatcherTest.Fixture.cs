using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace CoroutineDispatcher.Test
{
	public partial class DispatcherTest
	{
		private Dispatcher _dispatcher;
		private Thread _thread;
		private int _callNo;

		private void Dispatch(Action action)
		{
			_dispatcher.Dispatch(action);
		}

		private void Dispatch(DispatchPriority priority, Action action)
		{
			_dispatcher.Dispatch(priority, action);
		}

		private void Execute()
		{
			_thread = Thread.CurrentThread;
			_dispatcher.Execute();
		}

		private void AssertCall(int order)
		{
			_callNo += 1;
			Assert.AreEqual(order, _callNo);
			Assert.AreEqual(_thread, Thread.CurrentThread);
		}

		private void AssertTotalCalls(int calls)
		{
			Assert.AreEqual(calls, _callNo);
		}
	}
}
