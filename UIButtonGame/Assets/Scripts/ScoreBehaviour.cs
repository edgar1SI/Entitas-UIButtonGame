using Entitas;
using UnityEngine;
using UnityEngine.UI;

public class ScoreBehaviour : MonoBehaviour, ScoreListener {

	public Text scoreText;

	// Use this for initialization
	void Start () {
		// This script's listener is added to the pool at the start of the game
		Contexts.sharedInstance.game.CreateEntity ().AddScoreListener (this);
	}

	// This function is called only when the Points component is updated.
	public void UpdateScoreText (int currPoints, int maxPoints) {
		scoreText.text = "Score: " + currPoints.ToString () + "/" + maxPoints.ToString ();
	}
}
