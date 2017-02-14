using Entitas;
using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour {

	// INTERNAL
	Systems _systems;

	// STATE DATA
	public int targetPoints;


	void Awake () {
		// Initialize pool(s)
		var _contexts = Contexts.sharedInstance;
		_contexts.SetAllContexts ();

		// Create systems
		_systems = CreateSystems (_contexts);

		// Set all data.
		SetData (_contexts);
	}

	void Start ()
	{
		// Once systems have been created, they must be initialized.
		_systems.Initialize ();
	}

	void Update () {
		// Systems that implement Execute must be called every frame.
		_systems.Execute ();
		// Systems that implement Cleanup must be called every frame.
		//_systems.Cleanup ();
	}

	// The order systems are created in is the order in which they will execute, 
	// so it's important to keep your game logic well organized.
	Systems CreateSystems (Contexts contexts) {
		return new Feature ("Systems")
			.Add (new PointsSystem(contexts))
			.Add (new NotifyButtonListenersSystem(contexts))
			.Add (new VictoryCheckSystem(contexts))
			.Add (new UpdateScoreTextSystem(contexts))
			;
	}

	// Function that is used to set the game's data. It can be replaced with Blueprints or some
	// form of serialization.
	void SetData (Contexts contexts)
	{
		contexts.game.CreateEntity ().AddPoints (0, targetPoints);
	}
}
