using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CoroutineDispatcher.Test
{
	public partial class DispatcherTest
	{
		private Dispatcher _dispatcher;
		private Thread _mainThread;
		private int _callNo;

		[TestInitialize]
		public void Setup()
		{
			_dispatcher = new Dispatcher();
			_mainThread = Thread.CurrentThread;
		}

		private void Dispatch(Action action) => _dispatcher.Dispatch(action);
		private void Dispatch(DispatchPriority priority, Action action) => _dispatcher.Dispatch(priority, action);
		private void Dispatch(Func<Task> action) => _dispatcher.Dispatch(action);
		private void Dispatch(DispatchPriority priority, Func<Task> action) => _dispatcher.Dispatch(priority, action);

		private void Execute() => _dispatcher.Execute();
		private void Start() => _dispatcher.Start();
		private void Stop() => _dispatcher.Stop();

		private void AssertCall(int order)
		{
			Trace.WriteLine($"Main thread call: {order}");
			Assert.AreEqual(order, Interlocked.Increment(ref _callNo));
			Assert.AreEqual(_mainThread, Thread.CurrentThread);
		}

		private void AssertSecondThreadCall(int order)
		{
			Trace.WriteLine($"Second thread call: {order}");
			Assert.AreEqual(order, Interlocked.Increment(ref _callNo));
			Assert.AreNotEqual(_mainThread, Thread.CurrentThread);
		}

		private void AssertTotalCalls(int calls)
		{
			Assert.AreEqual(calls, _callNo);
		}
	}
}
