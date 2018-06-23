using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VrUiTest : MonoBehaviour {
	void Update() {
//		if (Input.GetMouseButton(0)) {
		var screenPoint =
			new Vector2(
				Input.mousePosition.x / Display.main.systemWidth * UnityEngine.XR.XRSettings.eyeTextureWidth,
				Input.mousePosition.y / Display.main.systemHeight * UnityEngine.XR.XRSettings.eyeTextureHeight);
		var ray = Camera.main.ScreenPointToRay(screenPoint);
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit)) {
			Debug.DrawLine(hit.point, hit.point + Vector3.one, Color.magenta);
		}
		Debug.DrawRay(ray.origin, ray.direction * 1000, Input.GetMouseButton(0) ? Color.red : Color.white);
		Debug.Log(string.Format("{0} / {1} * {2}", Input.mousePosition, UnityEngine.XR.XRSettings.eyeTextureHeight, Display.main.systemHeight));
//		} 
	}
}
