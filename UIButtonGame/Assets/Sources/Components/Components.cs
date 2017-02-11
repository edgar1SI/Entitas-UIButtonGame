using Entitas;
using Entitas.CodeGenerator;
using System.Collections.Generic;

/// <summary>
/// Points component. Used to store the global score data.
/// The SingleEntity attribute is used to make sure there is only one instance
/// of this component in the whole project. Requires Entitas.CodeGenerator.
/// </summary>
[Game, SingleEntity]
public sealed class PointsComponent : IComponent {
	public int currPoints;
	public int targetPoints;
}

/// <summary>
/// Points Entry component. Used to represent an event when points
/// need to be added. They will be procesed by another system.
/// </summary>
[Game]
public sealed class PointsEntryComponent : IComponent {
	public int points;
	public int buttonId;
}

/// <summary>
/// Button Off listener. Interface that implements the ButtonOffEvent.
/// The Pool attribute makes it so that the interface can be stored in
/// the pool as a component for easier access.
/// This interface is used by buttons to listen for button off events.
/// </summary>
[Game]
public interface ButtonOffListener {
	void ButtonOffEvent (int buttonID);
}

/// <summary>
/// Score listener. Same as other interfaces. This interface is used by
/// the score text to listen for score update events.
/// </summary>
[Game]
public interface ScoreListener {
	void UpdateScoreText (int currPoints, int maxPoints);
}
