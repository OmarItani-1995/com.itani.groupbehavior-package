using System;
using System.Collections.Generic;
using UnityEngine;

namespace GroupBehavior.Runtime
{
	/// <summary>
	/// Small utility: manages extra update callbacks.
	/// </summary>
	public class GroupScheduler
	{
		private readonly List<Action> _actions = new();

		public void Add(Action action)
		{
			if (action == null) throw new ArgumentNullException(nameof(action));
			if (_actions.Contains(action)) return;
			_actions.Add(action);
		}

		public void Remove(Action action)
		{
			if (action == null) return;
			_actions.Remove(action);
		}

		public void Tick()
		{
			for (int i = _actions.Count - 1; i >= 0; i--)
				_actions[i]?.Invoke();
		}
	}
}