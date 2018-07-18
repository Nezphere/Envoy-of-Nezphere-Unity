using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SongPanelHintUiController : MonoBehaviour {
	public Text uiText;

	void Start() {
		uiText.text += Application.persistentDataPath;
	}
}
