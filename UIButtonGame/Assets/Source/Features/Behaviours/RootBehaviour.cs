using Entitas;
using UnityEngine;
using System.Collections;

public class RootBehaviour : MonoBehaviour {

	// INTERNAL
	Systems _systems;

	// STATE DATA
	public int targetPoints;


	void Awake () {
		// Initialize pool(s)
		var _pools = Pools.sharedInstance;
		_pools.SetAllPools ();

		// Create systems
		_systems = CreateGameSystems (_pools.pool);

		// Set all data.
		SetData (_pools.pool);
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
		_systems.Cleanup ();
	}

	// The order systems are created in is the order in which they will execute, 
	// so it's important to keep your game logic well organized.
	Systems CreateGameSystems (Pool pool) {
		return new Feature ("Root Systems")

			// GAME LOOP
			.Add (pool.CreateSystem (new PointsSystem ()))
			.Add (pool.CreateSystem (new NotifyButtonListenersSystem ()))
			.Add (pool.CreateSystem (new VictoryCheckSystem ()))
			
			// UI		
			.Add (pool.CreateSystem (new UpdateScoreTextSystem()))
			;
	}

	// Function that is used to set the game's data. It can be replaced with Blueprints or some
	// form of serialization.
	void SetData (Pool pool)
	{
		pool.CreateEntity ().AddPoints (0, targetPoints);
	}
}
