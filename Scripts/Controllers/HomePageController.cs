using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HomePageController : MonoBehaviour {

	private GameObject singlePlayerButton;

	void Awake() {
		// Attach single player button with required script.
		this.singlePlayerButton = GameObject.Find (UIConstants.Components.SinglePlayerButton.ToString ());
		this.singlePlayerButton.SetActive (true);
		this.singlePlayerButton.GetComponent<Button> ().onClick.AddListener (
			() => { 
				Utils.SceneUtils.loadSceneBySceneName(UIConstants.SceneNames.SinglePlayerOptionPage.ToString ());
			}
		);
	}

	void Start() {
	
	}

	void Update() {
	
	}
}

