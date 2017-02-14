using Entitas;
using UnityEngine;
using System.Collections.Generic;


/// <summary>
/// Notify button listeners system. This system is tasked with notifying the
/// button listeners in the pool whenever a PointsEntry event is created.
/// </summary>
public sealed class NotifyButtonListenersSystem : ReactiveSystem<GameEntity> {

	readonly IGroup<GameEntity> _listeners;

	// Set the context for this system
	public NotifyButtonListenersSystem (Contexts contexts) : base (contexts.game) {
		_listeners = contexts.game.GetGroup (GameMatcher.ButtonOffListener);
	}

	// Get triggered by a component
	protected override Collector<GameEntity> GetTrigger (IContext<GameEntity> context) {
		return context.CreateCollector (GameMatcher.PointsEntry, GroupEvent.Added);
	}

	// Filter the trigger component(s)
	protected override bool Filter (GameEntity entity) {
		return entity.hasPointsEntry;
	}

	protected override void Execute (List<GameEntity> entities) {

		foreach (var l in _listeners.GetEntities ()) {
			int _id = entities [0].pointsEntry.buttonId;
			l.buttonOffListener.value.ButtonOffEvent (_id);
		}
	}
}

public class PointsSystem : ReactiveSystem<GameEntity> {

	readonly GameContext _context;

	// Set the context
	public PointsSystem (Contexts contexts) : base (contexts.game) {
		_context = contexts.game;
	}

	protected override Collector<GameEntity> GetTrigger (IContext<GameEntity> context) {
		return context.CreateCollector (GameMatcher.PointsEntry, GroupEvent.Added);
	}

	protected override bool Filter (GameEntity entity) {
		return entity.hasPointsEntry;
	}

	protected override void Execute (List<GameEntity> entities)
	{
		int _curr = _context.points.currPoints;
		int _max = _context.points.targetPoints;

		foreach (var e in entities) {
			_context.ReplacePoints (_curr + e.pointsEntry.points, _max);
		}
	}
}

public class UpdateScoreTextSystem : ReactiveSystem<GameEntity> {

	readonly GameContext _context;
	readonly IGroup<GameEntity> _listeners;

	public UpdateScoreTextSystem (Contexts contexts) : base (contexts.game) {
		_context = contexts.game;
		_listeners = contexts.game.GetGroup (GameMatcher.ScoreListener);
	}

	protected override Collector<GameEntity> GetTrigger (IContext<GameEntity> context) {
		return context.CreateCollector (GameMatcher.Points, GroupEvent.AddedOrRemoved);
	}

	protected override bool Filter (GameEntity entity) {
		return entity.hasPoints;
	}

	protected override void Execute (List<GameEntity> entities)
	{
		foreach (var l in _listeners.GetEntities ()) {
			l.scoreListener.value.UpdateScoreText (_context.points.currPoints, _context.points.targetPoints);
		}
	}
}

public class VictoryCheckSystem : ReactiveSystem<GameEntity> {

	readonly GameContext _context;
	readonly IGroup<GameEntity> _listeners;

	public VictoryCheckSystem (Contexts contexts) : base (contexts.game) {
		_context = contexts.game;
		_listeners = contexts.game.GetGroup (GameMatcher.ButtonOffListener);
	}

	protected override Collector<GameEntity> GetTrigger (IContext<GameEntity> context) {
		return context.CreateCollector (GameMatcher.Points, GroupEvent.AddedOrRemoved);
	}

	protected override bool Filter (GameEntity entity) {
		return entity.hasPoints;
	}

	protected override void Execute (List<GameEntity> entities)
	{
		int _curr = _context.points.currPoints;
		int _max = _context.points.targetPoints;

		if (_curr > _max) {
			Debug.Log ("Ÿou lose!");
			DisableAllButtons ();
		} 
		else if (_curr == _max) {
			Debug.Log ("Victory!");
			DisableAllButtons ();
		}
	}

	void DisableAllButtons () {
		foreach (var l in _listeners.GetEntities ()) {
			l.buttonOffListener.value.ButtonOffEvent (-1);
		}
	}
}