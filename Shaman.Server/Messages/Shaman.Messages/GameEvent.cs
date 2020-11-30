using System;

namespace Shaman.Messages
{
	[Serializable]
	public class  GameEvent
	{
		private Action _eventDelegate;

		public void Clear()
		{
			_eventDelegate = null;
		}

		public void Invoke()
		{
			_eventDelegate?.Invoke();
		}

		public void Subscribe(Action callback)
		{
			_eventDelegate += callback;
		}

		public void Unsubscribe(Action callback)
		{
			_eventDelegate -= callback;
		}

	}

	[Serializable]
	public class GameEvent<T0>
	{


		private Action<T0> _eventDelegate;



		public void Clear()
		{
			_eventDelegate = null;
		}

		public void Invoke(T0 param0)
		{
			if (_eventDelegate != null) _eventDelegate.Invoke(param0);
		}

		public void Subscribe(Action<T0> callback)
		{
			_eventDelegate += callback;
		}

		public void Unsubscribe(Action<T0> callback)
		{
			_eventDelegate -= callback;
		}


	}

	[Serializable]
	public class GameEvent<T0, T1>
	{


		private Action<T0, T1> _eventDelegate;



		public void Clear()
		{
			_eventDelegate = null;
		}

		public void Invoke(T0 param0, T1 param1)
		{
			if (_eventDelegate != null) _eventDelegate.Invoke(param0, param1);
		}

		public void Subscribe(Action<T0, T1> callback)
		{
			_eventDelegate += callback;
		}

		public void Unsubscribe(Action<T0, T1> callback)
		{
			_eventDelegate -= callback;
		}

	}

	[Serializable]
	public class GameEvent<T0, T1, T2>
	{


		private Action<T0, T1, T2> _eventDelegate;



		public void Clear()
		{
			_eventDelegate = null;
		}

		public void Invoke(T0 param0, T1 param1, T2 param2)
		{
			if (_eventDelegate != null) _eventDelegate.Invoke(param0, param1, param2);
		}

		public void Subscribe(Action<T0, T1, T2> callback)
		{
			_eventDelegate += callback;
		}

		public void Unsubscribe(Action<T0, T1, T2> callback)
		{
			_eventDelegate -= callback;
		}

	}

	[Serializable]
	public class GameEvent<T0, T1, T2, T3>
	{


		private Action<T0, T1, T2, T3> _eventDelegate;



		public void Clear()
		{
			_eventDelegate = null;
		}

		public void Invoke(T0 param0, T1 param1, T2 param2, T3 param3)
		{
			if (_eventDelegate != null) _eventDelegate.Invoke(param0, param1, param2, param3);
		}

		public void Subscribe(Action<T0, T1, T2, T3> callback)
		{
			_eventDelegate += callback;
		}

		public void Unsubscribe(Action<T0, T1, T2, T3> callback)
		{
			_eventDelegate -= callback;
		}

	}
	
	[Serializable]
	public class GameEvent<T0, T1, T2, T3, T4>
	{


		private Action<T0, T1, T2, T3, T4> _eventDelegate;



		public void Clear()
		{
			_eventDelegate = null;
		}

		public void Invoke(T0 param0, T1 param1, T2 param2, T3 param3, T4 param4)
		{
			if (_eventDelegate != null) _eventDelegate.Invoke(param0, param1, param2, param3, param4);
		}

		public void Subscribe(Action<T0, T1, T2, T3, T4> callback)
		{
			_eventDelegate += callback;
		}

		public void Unsubscribe(Action<T0, T1, T2, T3, T4> callback)
		{
			_eventDelegate -= callback;
		}

	}
	
	[Serializable]
	public class GameEvent<T0, T1, T2, T3, T4, T5>
	{


		private Action<T0, T1, T2, T3, T4, T5> _eventDelegate;



		public void Clear()
		{
			_eventDelegate = null;
		}

		public void Invoke(T0 param0, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5)
		{
			if (_eventDelegate != null) _eventDelegate.Invoke(param0, param1, param2, param3, param4, param5);
		}

		public void Subscribe(Action<T0, T1, T2, T3, T4, T5> callback)
		{
			_eventDelegate += callback;
		}

		public void Unsubscribe(Action<T0, T1, T2, T3, T4, T5> callback)
		{
			_eventDelegate -= callback;
		}

	}
}