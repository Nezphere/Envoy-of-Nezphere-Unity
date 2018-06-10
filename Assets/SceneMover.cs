using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneMover : MonoBehaviour {
	const float ViewDistance = 200;

	public GameObject corridoProto;
	public float spacing;
	double startTime;

	void Start() {
		int count = (int)(ViewDistance / spacing + 0.5f);
		for (int i = -count / 2; i < count; i++) {
			var go = Instantiate(corridoProto, transform);
			go.transform.localPosition = new Vector3(0, 0, i * spacing);
		}
		startTime = LivePlayer.time;
	}

	void Update() {
		float speed = -LivePlayer.Instance.bufferZ / LivePlayer.Instance.bufferInterval;
		double time = LivePlayer.time;
		float duration = Mathf.Abs(spacing / speed);
		if (time < startTime || time - startTime > duration) {
			startTime = time;
		}
		var pos = transform.localPosition;
		pos.z = (float)(time - startTime) * speed;
		transform.localPosition = pos;
	}
}
