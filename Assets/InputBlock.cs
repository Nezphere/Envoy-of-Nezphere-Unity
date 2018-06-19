using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class InputBlock : MonoBehaviour {
	public Text uiTargetText;
	public float minDyingSpeed = 20;
	public Text[] uiTexts;
	public bool[] isEnableds;
	public string[] texts;
	public Vector3[] collisionPoints;

	void Start() {
		RebuildLayout();
	}

	[ContextMenu("RebuildLayout")]
	public void RebuildLayout() {
		for (int i = 0; i < 8; i++) {
			uiTexts[i].text = isEnableds[i] ? texts[i] : "";
			uiTexts[i].transform.rotation = Quaternion.identity;
		}
	}

	void OnTriggerEnter(Collider other) {
		print(other.name);

		var tip = other.GetComponentInChildren<SwordTip>();
		if (tip == null)
			return;

		if (tip.speed < minDyingSpeed)
			return;

		Heading heading = Heading.Down;
		float speed, maxSpeed = 0;

		if (isEnableds[0] && (speed = Vector3.Dot(tip.velocity, transform.up)) > maxSpeed) {
			heading = Heading.Down;
			maxSpeed = speed;
		}
		if (isEnableds[4] && (speed = Vector3.Dot(tip.velocity, -transform.up)) > maxSpeed) {
			heading = Heading.Up;
			maxSpeed = speed;
		}
		if (isEnableds[2] && (speed = Vector3.Dot(tip.velocity, transform.right)) > maxSpeed) {
			heading = Heading.Left;
			maxSpeed = speed;
		}
		if (isEnableds[6] && (speed = Vector3.Dot(tip.velocity, -transform.right)) > maxSpeed) {
			heading = Heading.Right;
			maxSpeed = speed;
		}
		if (isEnableds[1] && (speed = Vector3.Dot(tip.velocity, (transform.up + transform.right).normalized)) > maxSpeed) {
			heading = Heading.DownLeft;
			maxSpeed = speed;
		}
		if (isEnableds[7] && (speed = Vector3.Dot(tip.velocity, (transform.up - transform.right).normalized)) > maxSpeed) {
			heading = Heading.DownRight;
			maxSpeed = speed;
		}
		if (isEnableds[3] && (speed = Vector3.Dot(tip.velocity, (-transform.up + transform.right).normalized)) > maxSpeed) {
			heading = Heading.UpLeft;
			maxSpeed = speed;
		}
		if (isEnableds[5] && (Vector3.Dot(tip.velocity, (-transform.up - transform.right).normalized)) > maxSpeed) {
			heading = Heading.UpRight;
		}

		uiTargetText.text += texts[(int)heading];

		Debug.Log(heading);
	}
}
