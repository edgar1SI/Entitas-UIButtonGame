using Entitas;
using UnityEngine;
using UnityEngine.UI;

public class ButtonClickBehaviour : MonoBehaviour, ButtonOffListener {

	// STATE DATA
	public int _thisID;
	public int _points;

	// REFERENCE
	public Text _pointsText;
	public Button _buttonRef;


	// This script's listener is added to the pool at the start of the game
	void Start () {
		_pointsText.text = _points.ToString ();
		//GameContext.CreateEntity ().AddButtonOffListener (this);
		Contexts.sharedInstance.game.CreateEntity ().AddButtonOffListener (this);
	}

	// Function that is called when the player clicks the button
	public void ButtonClick ()
	{
		//GameContext.CreateEntity ().AddPointsEntry (_points, _thisID);
		Contexts.sharedInstance.game.CreateEntity ().AddPointsEntry (_points, _thisID);
	}

	// Contract that belongs to ButtonOffListener interface, declared up top...
	public void ButtonOffEvent (int buttonID)
	{
		// Instantly deactivate the button
		if (buttonID == -1) {
			_buttonRef.interactable = false;
			return;
		}

		// If the button off event ID matches this button's ID, turn button OFF
		if (buttonID == _thisID) {
			_buttonRef.interactable = false;
		}
		// Else the event doesn't match the button id, turn button ON
		else {
			_buttonRef.interactable = true;
		}
	}
}
