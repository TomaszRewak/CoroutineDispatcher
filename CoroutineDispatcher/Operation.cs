using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CoroutineDispatcher
{
	internal readonly struct Operation
	{
		private readonly Action _action;
		private readonly Func<ValueTask> _asyncAction;

		public Operation(Action action)
		{
			_action = action;
			_asyncAction = default;
		}

		public Operation(Func<ValueTask> asyncAction)
		{
			_action = default;
			_asyncAction = asyncAction;
		}

		public ValueTask Invoke()
		{
			if (_action != default)
			{
				_action();
				return new ValueTask();
			}
			else
			{
				return _asyncAction();
			}
		}
	}
}
