# CoroutineDispatcher

CoroutineDispatcher is a lightweight framework for running multiple tasks asynchronously on a single thread.

### Why?

With heavily multi-threaded applications it can be difficult to manage data synchronization and ownership. Performing all related operations on a single thread can help in achieving much more predictable behavior with a code that's much easier to maintain. In some cases it can even result in a better performance. 

Of course it doesn’t mean that one has to be limited to just a single thread. The CoroutineDispatcher can be used to spawn multiple independent task queues that can communicate with each other by dispatching tasks on one another. 

### Inspiration

The `CoroutineDispatcher` is strongly inspired by the `System.Windows.Dispatcher` used within the WPF framework. Just instead of using the default Windows message pump, it relays on a basic operation queue that is consumed within a relatively simple execution loop.

### What is it?

The heart of the `CoroutineDispatcher` is the `Dispatcher` class. You can think of it as a fancy priority queue that stores tasks to be run. By starting the `Dispatcher` you initialize an infinite loop that processes queued operations and waits for new ones once the list is empty.

Tasks can be scheduled beforehand or during execution (both from the thread on which the dispatcher is currently running as well as other threads).

On top of that simple concept the CoroutineDispatcher provides a handful of useful abstractions that make it easy to use within real world scenarios.

### How to use it?

First: install the package in your project.

`> dotnet add package CoroutineDispatcher`

Second: simply create a `Dispatcher`, queue (or not) some operations and start the execution.

```csharp
var dispatcher = new Dispatcher();

void F1() =>
dispatcher.Dispatch(DispatchPriority.Medium, () =>
{
	Console.Write(4)
});

void F2() =>
dispatcher.Dispatch(DispatchPriority.High, () =>
{
	Console.Write(3)
});

void F3() =>
dispatcher.Dispatch(DispatchPriority.Low, () =>
{
	Console.Write(5)
});

void F4() =>
dispatcher.Dispatch(() =>
{
	Console.Write(1);
	F1();
	F2();
	Console.Write(2);
});

F3();
F4();

dispatcher.Start(); // Prints "12345" and waits for new operations 
```

The `Dispatch` method is the simples of the task queueing operations. It’s a basic “fire and forget” used when the calling thread is not interested in the result of the task execution. 

An alternative to the `Dispatch` method is the `Invoke` method. It also queues the task on the dispatcher, but stops the calling thread until the execution of that task is over. It allows us not only to synchronize threads, but also easily acquire results of computations.

```csharp
var dispatcher = new Dispatcher();

void Add(int a, int b) =>
dispatcher.Invoke(() => {
	return a + b; // This executes on the dispatcher thread along with other operations (a and b are captured in a closure)
});

Task.Run(() => {
	int a = 2, b = 2; // This executes on a separate thread
	var sum = Add(a, b);
	Console.Write(sum); // And here we are on the background thread again
});

dispatcher.Start();
```

(Please excuse the amount of lambdas used in those examples - it's just easier to create simple and compact snippets this way)

The `CoroutineDispatcher` (of course) also provides support for dispatching asynchronous operations.

```csharp
var dispatcher1 = new Dispatcher();
var dispatcher2 = Dispatcher.Spawn(); // Creates and starts a dispatcher on a new thread

dispatcher1.Dispatch(async () =>
{
	var r = 2.0;
	// While we await the result, the dispatcher1 is free to execute other pending operations
	var v = await dispatcher2.InvokeAsync(() => Math.PI * r * r);
	// The continuation of the awaited operation is properly dispatched back on the dispatcher1
	Console.Write(v);
});
dispatcher1.Dispatch(() => { });

dispatcher1.Start();
```

You can even intentionally yield execution back to the dispatcher if you are worried that other tasks might get starved during an execution of a long running operation.

```csharp
var dispatcher = new Dispatcher();

dispatcher.Dispatch(async () =>
{
	Console.Write(1);
	dispatcher.Dispatch(DispatchPriority.Low, () => Console.Write(4));
	dispatcher.Dispatch(DispatchPriority.High, () => Console.Write(2));
	Thread.Sleep(1000);

	// Will allow for execution of pending tasks with at least medium priority before continuing
	await Dispatcher.Yield(DispatchPriority.Medium);

	Thread.Sleep(1000);
	Console.Write(3);
});

dispatcher.Start(); // Will print "1234"
```

The last but not least is the task scheduling. It can be used run a task after a specified delay.

```csharp
var dispatcher = new Dispatcher();

dispatcher.Schedule(TimeSpan.FromSeconds(30), () => Console.Write("At least 30 seconds have passed"));

dispatcher.Start();
```

Once you are done with a Dispatcher you can simply call the `Stop()` method. It will not terminate the currently running task, but will prevent the dispatcher from consuming any new ones. But don’t worry - queued tasks are not lost. Calling `Start()` again will resume the processing from where it was left off.

And that's it from the most essential basics. Maybe not much - but for many use cases - more then enough.

At the end, just as a hint, I want to share that I've found the following pattern (also used in some of the above) to be the most handy when working with tasks management in systems with multiple dispatchers.

```csharp
internal sealed class Consumer
{
	private readonly Dispatcher _dispatcher = Dispatcher.Spawn();

	public void Consume(string item) =>
	_dispatcher.Dispatch(async () =>
	{
		...
	});

	public Task<int> GetCount() =>
	_dispatcher.InvokeAsync(() =>
	{
		...
	});

	private async Task<bool> CheckOnTheServer(string item)
	{
		...
	}
}
```

So instead of making it a responsibility of the caller to know where to dispatch an operation - in this pattern we annotate the public interface of a component owning the resources with correct task management operations to make sure that the work is always performed on a correct thread.

But we can also reduce the first example in this README to just this:

```csharp
var dispatcher = new Dispatcher();

dispatcher.Dispatch(() =>
{
	Console.Write(1);
	dispatcher.Dispatch(DispatchPriority.High, () => Console.Write(3));
	dispatcher.Dispatch(DispatchPriority.Low, () => Console.Write(5));
	Console.Write(2);
});
dispatcher.Dispatch(DispatchPriority.Medium, () => Console.Write(4));

dispatcher.Start(); // Prints "12345" and waits for new operations 
```

### The `CoroutineDispatcher.Dispatcher` class

##### `Dispatcher Dispatcher.Current { get; }`

Returns a `Dispatcher` currently running on the calling thread. By using it you don't have to pass around the reference to your dispatcher.

##### `void Dispatcher.Start()`

Starts an infinite loop on the current thread to process all queued and scheduled operations. Once the queue of operations is empty, waits for new ones to arrive.

##### `void Dispatcher.Stop()`

Stops the current execution. If called from within an operation currently executed by the dispatcher, the operation itself will not be terminated, but no new operations will be acquired from the operation queue once the current operation finishes or yields.

##### `void Dispatcher.Execute()`

Processes all queued operations until the active operation queue is empty. Performed execution will include all due scheduled operations, but will not wait for operations scheduled for after the active operation queue has been emptied nor the callbacks of awaited operations that did not finish yet.

##### `bool Dispatcher.CheckAccess()`

Returns `true` if this instance of the `Dispatcher` is currently running on the calling thread.

##### `static Dispatcher Dispatcher.Spawn()`

Creates a new `Dispatcher` and starts it on a new thread.

##### `void Dispatcher.Dispatch(...)`
###### `void Dispatch([DispatchPriority priority = DispatchPriority.Medium,] Action operation)`
###### `void Dispatch([DispatchPriority priority = DispatchPriority.Medium,] Func<Task> operation)`

Adds the `operation` to the operation queue without blocking of the current thread (fire and forget).

```
═════════ dispatcher2.Dispatch(...) ═════════

dispatcher1      dispatcher2      thread_pool
     │                │                ║     
     ∩                │                ║     
     ║                ∩                ║     
     ╟───────┐        ║                ║     
     ║       │        U                ║     
     U       └────────╖                ║     
     ∩                ║       ┌────────╢     
     ║                U       │        ║     
     ║                ∩       │        ║     
     ║           ┌────╢       │        ║     
     U           │    U       │        ║     
     │           │    ╓───────┘        ║     
     ∩           │    ║                ║     
     ║           │    U                ║     
     U           └────╖                ║     
     │                ║                ║       
```

##### `... Dispatcher.Invoke(...)`
###### `void Invoke([DispatchPriority priority = DispatchPriority.Medium,] Action operation)`
###### `T Invoke<T>([DispatchPriority priority = DispatchPriority.Medium,] Func<T> operation)`

Adds the `operation` to the operation queue and stops the calling thread until it's executed.

If called from within an operation currently executed by the `Dispatcher.Current`, the provided `operation` will be performed in place to avoid deadlocks.

```
══════════ dispatcher2.Invoke(...) ══════════

dispatcher1      dispatcher2      thread_pool
     │                │                ║     
     ∩                │                ║     
     ║                ∩                ║     
     ╙───────┐        ║                ║     
     .       │        U                ║     
     .       └────────╖                ║     
     .                ║       ┌────────╜     
     .                ║       │        .     
     ╓────────────────╜       │        .     
     ║                ∩       │        .     
     U                U       │        .     
     │                ╓───────┘        .     
     ∩                ║                .     
     ║                ╙────────────────╖     
     U                │                ║            
```

##### `... Dispatcher.InvokeAsync(...)`
###### `Task InvokeAsync([DispatchPriority priority = DispatchPriority.Medium,] Action operation)`
###### `Task InvokeAsync([DispatchPriority priority = DispatchPriority.Medium,] Func<Task> operation)`
###### `Task<T> InvokeAsync<T>([DispatchPriority priority = DispatchPriority.Medium,] Func<T> operation)`
###### `Task<T> InvokeAsync<T>([DispatchPriority priority = DispatchPriority.Medium,] Func<Task<T>> operation)`

Adds the `operation` to the operation queue and returns a `Task` associated with the state of its execution.

```
═════ await dispatcher2.InvokeAsync(...) ═════

dispatcher1      dispatcher2      thread_pool
     │                │               ║ │    
     ∩                │               ║ │    
     ║                ∩               ║ │    
     ╙───────┐        ║               ║ │    
     │       │        U               ║ │    
     ∩       └────────╖               ║ │    
     ║                ║       ┌───────╜ │    
     ║                ║       │       │ │    
     ║       ┌────────╜       │       │ │    
     U       │        ∩       │       │ │    
     ╓───────┘        U       │       │ │    
     ║                ╓───────┘       │ │    
     U                ║               │ │    
     ∩                ╙───────────────┼ ╖    
     U                │               │ ║   
```

##### `void Schedule(...)`
###### `void Schedule(TimeSpan delay, [DispatchPriority priority = DispatchPriority.Medium,] Action operation)`
###### `void Schedule(TimeSpan delay, [DispatchPriority priority = DispatchPriority.Medium,] Func<Task> operation)`

Schedules the execution of the `operation` after the provided `delay`. 

It is not guaranteed that the `operation` will be executed exactly after the provided `delay` (it's only guaranteed that it will be queued not sooner then that).

##### `static YieldTask Dispatcher.Yield(DispatchPriority priority = DispatchPriority.Low)`

When awaited, will yield the execution of the current dispatcher to other queued operations with at least given `priority`.

```
════════ await Dispatcher.Yield(...) ════════

                  dispatcher                 
                      │                      
                      │                      
                      ∩                      
                      ║                      
                 ┌────╜                      
                 │    ∩                      
                 │    ║                      
                 │    U                      
                 │    ∩                      
                 │    ║                      
                 │    U                      
                 └────╖                      
                      ║                      
                      U                      
                      │                      
```

# Contributions

I'm open to tickets and contributions.
