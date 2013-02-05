using System;
using System.Threading;

namespace SkyBiometry.Client.FC
{
	internal class AsyncResult : IAsyncResult
	{
		#region Private constants

		private const int StatePending = 0;
		private const int StateCompletedSynchronously = 1;
		private const int StateCompletedAsynchronously = 2;

		#endregion

		#region Private fields

		private readonly AsyncCallback _asyncCallback;
		private readonly object _asyncState;
		private readonly object _lock = new object();
		private int _completedState = StatePending;
		private ManualResetEvent _asyncWaitHandle;
		private Exception _exception;

		#endregion

		#region Public constructor

		public AsyncResult(AsyncCallback asyncCallback, Object asyncState)
		{
			_asyncCallback = asyncCallback;
			_asyncState = asyncState;
		}

		#endregion

		#region Public methods

		public void SetAsCompleted(Exception exception, bool completedSynchronously)
		{
			_exception = exception;
			if (Interlocked.Exchange(ref _completedState,
				completedSynchronously ? StateCompletedSynchronously : StateCompletedAsynchronously) != StatePending)
				throw new InvalidOperationException("AsyncResult is already marked as completed");
			if (_asyncWaitHandle != null) _asyncWaitHandle.Set();
			if (_asyncCallback != null) _asyncCallback(this);
		}

		public void EndInvoke()
		{
			lock (_lock)
			{
				if (!IsCompleted)
				{
					AsyncWaitHandle.WaitOne();
					AsyncWaitHandle.Dispose(); _asyncWaitHandle = null;
				}
			}
			if (_exception != null) throw _exception;
		}

		#endregion

		#region IAsyncResult interface

		public Object AsyncState
		{
			get
			{
				return _asyncState;
			}
		}

		public bool CompletedSynchronously
		{
			get
			{
				return _completedState == StateCompletedSynchronously;
			}
		}

		public WaitHandle AsyncWaitHandle
		{
			get
			{
				if (_asyncWaitHandle == null)
				{
					bool done = IsCompleted;
					var mre = new ManualResetEvent(done);
					if (Interlocked.CompareExchange(ref _asyncWaitHandle, mre, null) != null)
					{
						mre.Dispose();
					}
					else
					{
						if (!done && IsCompleted)
						{
							_asyncWaitHandle.Set();
						}
					}
				}
				return _asyncWaitHandle;
			}
		}

		public bool IsCompleted
		{
			get
			{
				return _completedState != StatePending;
			}
		}
		#endregion
	}

	internal sealed class AsyncResult<T> : AsyncResult
	{
		#region Private fields

		private T _result;

		#endregion

		#region Public constructor

		public AsyncResult(AsyncCallback asyncCallback, Object state) :
			base(asyncCallback, state)
		{
		}

		#endregion

		#region Public methods

		public void SetAsCompleted(T result, bool completedSynchronously)
		{
			_result = result;
			base.SetAsCompleted(null, completedSynchronously);
		}

		new public T EndInvoke()
		{
			base.EndInvoke();
			return _result;
		}

		#endregion
	}
}
