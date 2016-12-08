using Entitas;
using UnityEngine;
using System.Collections.Generic;


/// <summary>
/// Notify button listeners system. This system is tasked with notifying the
/// button listeners in the pool whenever a PointsEntry event is created.
/// </summary>
public class NotifyButtonListenersSystem : ISetPool, IReactiveSystem {

	Pool _pool;
	Group _listeners;

	// OnEntityAdded is called whenever a new component is created. In this case, when
	// a PointsEntry component is created (basically every time a button is clicked).
	public TriggerOnEvent trigger { get { return Matcher.PointsEntry.OnEntityAdded (); } }

	public void SetPool (Pool pool) {
		_pool = pool;
		// We use GetGroup to find all the ButtonOffListeners in the pool. Basically anyone who
		// is interested in knowing when a button needs to be turned off.
		_listeners = _pool.GetGroup (Matcher.ButtonOffListener);
	}

	public void Execute (List<Entity> entities) {
		// We send the ID of the button that created the event to every listener in the group.
		foreach (var l in _listeners.GetEntities ()) {
			int _id = entities [0].pointsEntry.buttonId;
			l.buttonOffListener.value.ButtonOffEvent (_id);
		}
	}
}

/// <summary>
/// Points system. This system is tasked with processing the PointsEntry events.
/// It adds the points to the global variable and destroys the component once it's done.
/// </summary>
public class PointsSystem : ISetPool, IReactiveSystem, ICleanupSystem {

	Pool _pool;

	public void SetPool (Pool pool) { _pool = pool; } 

	public TriggerOnEvent trigger { get { return Matcher.PointsEntry.OnEntityAdded (); } }

	public void Execute (List<Entity> entities) {

		int _curr = _pool.points.currPoints;
		int _max = _pool.points.targetPoints;

		// To add or change a component's data, we use the Replace{ComponentName} method
		foreach (var e in entities) {
			_pool.ReplacePoints (_curr + e.pointsEntry.points, _max);
		}
	}

	// ICleanupSystem is implemented so that the event components (in this case PointsEntry)
	// are deleted after they have been registered, since we no longer need them.
	public void Cleanup() {
		Group _entries = _pool.GetGroup (Matcher.PointsEntry);
		foreach (var e in _entries.GetEntities()) {
			_pool.DestroyEntity (e);
		}
	}
}

/// <summary>
/// Update score text system. This system is tasked with notifying the score
/// listeners whenever the Points component is updated.
/// </summary>
public class UpdateScoreTextSystem : ISetPool, IReactiveSystem {

	Pool _pool;
	Group _listeners;

	public void SetPool (Pool pool) {
		_pool = pool;
		_listeners = _pool.GetGroup (Matcher.ScoreListener);
	} 

	public TriggerOnEvent trigger { get { return Matcher.Points.OnEntityAddedOrRemoved (); } }

	public void Execute (List<Entity> entities) {

		foreach (var l in _listeners.GetEntities()) {
			l.scoreListener.value.UpdateScoreText(_pool.points.currPoints, _pool.points.targetPoints);
		}
	}
}

/// <summary>
/// Victory check system. This system is in charge of the win/lose condition of the game.
/// It is called only when the Points component changes.
/// </summary>
public class VictoryCheckSystem : ISetPool, IReactiveSystem {
	Pool _pool;

	public void SetPool (Pool pool) {
		_pool = pool;
	} 
	
	public TriggerOnEvent trigger { get { return Matcher.Points.OnEntityAddedOrRemoved (); } }

	public void Execute (List<Entity> entities) {

		int _curr = _pool.points.currPoints;
		int _max = _pool.points.targetPoints;

		// If the current score is greater than the target score, then you lose
		if (_curr > _max) {
			Debug.Log ("You lose!");
			DisableAllButtons ();
		}
		// If the current score is equal to the target score, then you win
		else if (_curr == _max) {
			Debug.Log ("Victory!");
			DisableAllButtons ();
		}
	}

	// Hack function that disables all buttons once the game is over.
	// Probably shouldn't be here and could be done better :/
	public void DisableAllButtons ()
	{
		Group _listeners = _pool.GetGroup (Matcher.ButtonOffListener);

		foreach (var l in _listeners.GetEntities()) {
			l.buttonOffListener.value.ButtonOffEvent (-1);
		}
	}
}

