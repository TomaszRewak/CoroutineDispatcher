using System;
using System.Threading;
using System.Threading.Tasks;

namespace CoroutineDispatcher
{
	public class Dispatcher
	{
		[ThreadStatic]
		private static Dispatcher _current = null;
		/// <summary>
		/// Returns a <see cref="Dispatcher"/> currently running on the calling thread.
		/// </summary>
		/// <remarks>
		/// Will return <c>null</c> if there is no <see cref="Dispatcher"/> running on the calling thread.
		/// </remarks>
		public static Dispatcher Current => _current;

		private readonly CoroutineSynchronizationContext _synchronizationContext = new CoroutineSynchronizationContext();

		/// <summary>
		/// Starts an infinite loop on the current thread to process all queued and scheduled operations.
		/// </summary>
		/// <seealso cref="Stop()"/>
		public void Start()
		{
			var oldDispatcher = _current;
			var oldSynchronizationContext = SynchronizationContext.Current;

			try
			{
				_current = this;
				SynchronizationContext.SetSynchronizationContext(_synchronizationContext);

				_synchronizationContext.Start();
			}
			finally
			{
				_current = oldDispatcher;
				SynchronizationContext.SetSynchronizationContext(oldSynchronizationContext);
			}
		}

		/// <summary>
		/// Processes all queued operations until the active operation queue is empty.
		/// </summary>
		/// <remarks>
		/// Performed execution will include all due scheduled operations, but will not wait for operations scheduled for after the active operation queue has been emptied nor the callbacks of awaited operations that did not finish yet.
		/// </remarks>
		/// <seealso cref="Stop()"/>
		public void Execute()
		{
			var oldDispatcher = _current;
			var oldSynchronizationContext = SynchronizationContext.Current;

			try
			{
				_current = this;
				SynchronizationContext.SetSynchronizationContext(_synchronizationContext);

				_synchronizationContext.Execute();
			}
			finally
			{
				_current = oldDispatcher;
				SynchronizationContext.SetSynchronizationContext(oldSynchronizationContext);
			}
		}

		/// <summary>
		/// Stops the current execution that was started be either calling the <see cref="Start()"/> or the <see cref="Execute()"/> method.
		/// </summary>
		/// <remarks>
		/// If called from within an operation currently executed by dispatcher, the operation itself will not be terminated, but no new operations will be acquired from the operation queue once the current operation finishes or yields. 
		/// </remarks>
		public void Stop()
		{
			_synchronizationContext.Stop();
		}

		/// <summary>
		/// Verifies if this <see cref="Dispatcher"/> is the <see cref="Current"/>
		/// </summary>
		/// <returns><c>true</c> if this instance of the <see cref="Dispatcher"/> is currently running on the calling thread.</returns>
		public bool CheckAccess()
		{
			return this == Current;
		}

		/// <summary>
		/// Creates a new <see cref="Dispatcher"/> and starts it on a new thread.
		/// </summary>
		public static Dispatcher Spawn()
		{
			var dispatcher = new Dispatcher();
			var thread = new Thread(() => {
				dispatcher.Start();
			});
			thread.Start();

			return dispatcher;
		}

		/// <summary>
		/// Adds the <paramref name="operation"/> to the operation queue and stops the current thread until it's executed. The operation will be queued with a default <see cref="DispatchPriority.Medium"/> priority.
		/// </summary>
		/// <param name="operation">Operation to be queued</param>
		/// <remarks>If called from within an operation currently executed by the <see cref="Current"/>, the provided <paramref name="operation"/> will be performed in place to avoid deadlocks.</remarks>
		public void Invoke(Action operation) => Invoke(DispatchPriority.Medium, operation);
		/// <summary>
		/// Adds the <paramref name="operation"/> to the operation queue and stops the current thread until it's executed.
		/// </summary>
		/// <param name="priority">Priority of the operation</param>
		/// <param name="operation">Operation to be queued</param>
		/// <remarks>If called from within an operation currently executed by the <see cref="Current"/>, the provided <paramref name="operation"/> will be performed in place to avoid deadlocks.</remarks>
		public void Invoke(DispatchPriority priority, Action operation)
		{
			if (CheckAccess())
				operation();
			else
				_synchronizationContext.Send(priority, operation);
		}

		/// <summary>
		/// Adds the <paramref name="operation"/> to the operation queue and stops the current thread until it's executed. The operation will be queued with a default <see cref="DispatchPriority.Medium"/> priority.
		/// </summary>
		/// <param name="operation">Operation to be queued</param>
		/// <returns>Result value of the performed operation</returns>
		/// <remarks>If called from within an operation currently executed by the <see cref="Current"/>, the provided <paramref name="operation"/> will be performed in place to avoid deadlocks.</remarks>
		public T Invoke<T>(Func<T> operation) => Invoke(DispatchPriority.Medium, operation);
		/// <summary>
		/// Adds the <paramref name="operation"/> to the operation queue and stops the current thread until it's executed.
		/// </summary>
		/// <param name="priority">Priority of the operation</param>
		/// <param name="operation">Operation to be queued</param>
		/// <returns>Result value of the performed operation</returns>
		/// <remarks>If called from within an operation currently executed by the <see cref="Current"/>, the provided <paramref name="operation"/> will be performed in place to avoid deadlocks.</remarks>
		public T Invoke<T>(DispatchPriority priority, Func<T> operation)
		{
			if (CheckAccess())
				return operation();
			else
				return InvokeAsync(priority, operation).Result;
		}

		/// <summary>
		/// Adds the <paramref name="operation"/> to the operation queue and returns a <see cref="Task"/> associated with the state of its execution. The operation will be queued with a default <see cref="DispatchPriority.Medium"/> priority.
		/// </summary>
		/// <param name="operation">Operation to be queued</param>
		public Task InvokeAsync(Action operation) => InvokeAsync(DispatchPriority.Medium, operation);
		/// <summary>
		/// Adds the <paramref name="operation"/> to the operation queue and returns a <see cref="Task"/> associated with the state of its execution.
		/// </summary>
		/// <param name="priority">Priority of the operation</param>
		/// <param name="operation">Operation to be queued</param>
		public Task InvokeAsync(DispatchPriority priority, Action operation)
		{
			return _synchronizationContext.Send(priority, operation);
		}

		/// <summary>
		/// Adds the asynchronous <paramref name="operation"/> to the operation queue and returns a <see cref="Task"/> associated with the state of its execution. The operation will be queued with a default <see cref="DispatchPriority.Medium"/> priority.
		/// </summary>
		/// <param name="operation">Operation to be queued</param>
		public Task InvokeAsync(Func<Task> operation) => InvokeAsync(DispatchPriority.Medium, operation);
		/// <summary>
		/// Adds the asynchronous <paramref name="operation"/> to the operation queue and returns a <see cref="Task"/> associated with the state of its execution.
		/// </summary>
		/// <param name="priority">Priority of the operation</param>
		/// <param name="operation">Operation to be queued</param>
		public async Task InvokeAsync(DispatchPriority priority, Func<Task> operation)
		{
			await await InvokeAsync<Task>(priority, operation);
		}

		/// <summary>
		/// Adds the <paramref name="operation"/> to the operation queue and returns a <see cref="Task"/> associated with the state of its execution. The operation will be queued with a default <see cref="DispatchPriority.Medium"/> priority.
		/// </summary>
		/// <param name="operation">Operation to be queued</param>
		public Task<T> InvokeAsync<T>(Func<T> operation) => InvokeAsync(DispatchPriority.Medium, operation);
		/// <summary>
		/// Adds the <paramref name="operation"/> to the operation queue and returns a <see cref="Task"/> associated with the state of its execution.
		/// </summary>
		/// <param name="priority">Priority of the operation</param>
		/// <param name="operation">Operation to be queued</param>
		public Task<T> InvokeAsync<T>(DispatchPriority priority, Func<T> operation)
		{
			return _synchronizationContext.Send(priority, operation);
		}

		/// <summary>
		/// Adds the asynchronous <paramref name="operation"/> to the operation queue and returns a <see cref="Task"/> associated with the state of its execution. The operation will be queued with a default <see cref="DispatchPriority.Medium"/> priority.
		/// </summary>
		/// <param name="operation">Operation to be queued</param>
		public Task<T> InvokeAsync<T>(Func<Task<T>> operation) => InvokeAsync(DispatchPriority.Medium, operation);
		/// <summary>
		/// Adds the asynchronous <paramref name="operation"/> to the operation queue and returns a <see cref="Task"/> associated with the state of its execution.
		/// </summary>
		/// <param name="priority">Priority of the operation</param>
		/// <param name="operation">Operation to be queued</param>
		public async Task<T> InvokeAsync<T>(DispatchPriority priority, Func<Task<T>> operation)
		{
			return await await InvokeAsync<Task<T>>(priority, operation);
		}

		/// <summary>
		/// Adds the <paramref name="operation"/> to the operation queue without blocking the current thread (fire and forget). The operation will be queued with a default <see cref="DispatchPriority.Medium"/> priority.
		/// </summary>
		/// <param name="operation">Operation to be queued</param>
		public void Dispatch(Action operation) => Dispatch(DispatchPriority.Medium, operation);
		/// <summary>
		/// Adds the <paramref name="operation"/> to the operation queue without blocking the current thread (fire and forget).
		/// </summary>
		/// <param name="priority">Priority of the operation</param>
		/// <param name="operation">Operation to be queued</param>
		public void Dispatch(DispatchPriority priority, Action operation)
		{
			_synchronizationContext.Post(priority, operation);
		}

		/// <summary>
		/// Adds the asynchronous <paramref name="operation"/> to the operation queue without blocking the current thread (fire and forget). The operation will be queued with a default <see cref="DispatchPriority.Medium"/> priority.
		/// </summary>
		/// <param name="operation">Operation to be queued</param>
		public void Dispatch(Func<Task> operation) => Dispatch(DispatchPriority.Medium, operation);
		/// <summary>
		/// Adds the asynchronous <paramref name="operation"/> to the operation queue without blocking the current thread (fire and forget).
		/// </summary>
		/// <param name="priority">Priority of the operation</param>
		/// <param name="operation">Operation to be queued</param>
		public void Dispatch(DispatchPriority priority, Func<Task> operation)
		{
			_synchronizationContext.Post(priority, () => operation());
		}

		/// <summary>
		/// Schedules the queuing of the <paramref name="operation"/> after the provided <paramref name="delay"/>. The operation will be queued with a default <see cref="DispatchPriority.Medium"/> priority.
		/// </summary>
		/// <param name="delay"></param>
		/// <param name="operation">Operation to be scheduled</param>
		/// <remarks>
		/// It is not guaranteed that the <paramref name="operation"/> will be executed exactly after the provided <paramref name="delay"/> (it's only guaranteed that it will be executed not sooner then that).
		/// </remarks>
		public void Schedule(TimeSpan delay, Action operation) => Schedule(delay, DispatchPriority.Medium, operation);
		/// <summary>
		/// Schedules the queuing of the <paramref name="operation"/> after the provided <paramref name="delay"/>.
		/// </summary>
		/// <param name="delay"></param>
		/// <param name="operation">Operation to be scheduled</param>
		/// <remarks>
		/// It is not guaranteed that the <paramref name="operation"/> will be executed exactly after the provided <paramref name="delay"/> (it's only guaranteed that it will be executed not sooner then that).
		/// </remarks>
		public void Schedule(TimeSpan delay, DispatchPriority priority, Action operation)
		{
			_synchronizationContext.PostDelayed(DateTime.UtcNow + delay, priority, operation);
		}

		/// <summary>
		/// Schedules the queuing of the asynchronous <paramref name="operation"/> after the provided <paramref name="delay"/>. The operation will be queued with a default <see cref="DispatchPriority.Medium"/> priority.
		/// </summary>
		/// <param name="delay"></param>
		/// <param name="operation">Operation to be scheduled</param>
		/// <remarks>
		/// It is not guaranteed that the <paramref name="operation"/> will be executed exactly after the provided <paramref name="delay"/> (it's only guaranteed that it will be executed not sooner then that).
		/// </remarks>
		public void Schedule(TimeSpan delay, Func<Task> operation) => Schedule(delay, DispatchPriority.Medium, operation);
		/// <summary>
		/// Schedules the queuing of the <paramref name="operation"/> after the provided <paramref name="delay"/>.
		/// </summary>
		/// <param name="delay"></param>
		/// <param name="operation">Operation to be scheduled</param>
		/// <remarks>
		/// It is not guaranteed that the <paramref name="operation"/> will be executed exactly after the provided <paramref name="delay"/> (it's only guaranteed that it will be executed not sooner then that).
		/// </remarks>
		public void Schedule(TimeSpan delay, DispatchPriority priority, Func<Task> operation)
		{
			_synchronizationContext.PostDelayed(DateTime.UtcNow + delay, priority, () => operation());
		}

		/// <summary>
		/// When awaited, will yield the execution of the current dispatcher to other queued operations with at least <paramref name="minPriority"/>.
		/// </summary>
		/// <param name="minPriority">The priority of operations to be executed</param>
		/// <exception cref="DispatcherException">Throws the <see cref="DispatcherException"/> if no dispatcher is currently running on the calling thread.</exception>
		public static YieldTask Yield(DispatchPriority minPriority = DispatchPriority.Low)
		{
			return new YieldTask(minPriority);
		}
	}
}
