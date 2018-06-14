using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulidingMover : MonoBehaviour {
	public float maxZ = 100, minZ;
	public Vector3 rotation, rotationRand, initRotation, initRotationRand;

	double startTime;
	Vector3 curRotation;

	static float Rand() {
		return Random.value * 2f - 1f;
	}

	void Start() {
		transform.localEulerAngles = initRotation + new Vector3(Rand() * initRotationRand.x, Rand() * initRotationRand.y, Rand() * initRotationRand.z);
		curRotation = rotation + new Vector3(Rand() * rotationRand.x, Rand() * rotationRand.y, Rand() * rotationRand.z);
	}

	void Update() {
		double time = LivePlayer.AccTime;

		transform.Rotate(curRotation * (float)LivePlayer.DeltaTime);

		float duration = Mathf.Abs((maxZ - minZ) / LivePlayer.Speed);
		if (time < startTime || time - startTime > duration) {
			transform.localEulerAngles = initRotation + new Vector3(Rand() * initRotationRand.x, Rand() * initRotationRand.y, Rand() * initRotationRand.z);
			curRotation = rotation + new Vector3(Rand() * rotationRand.x, Rand() * rotationRand.y, Rand() * rotationRand.z);

			startTime = time;
		}
			
		var pos = transform.localPosition;
		pos.z = (float)(time - startTime) / duration * (maxZ - minZ) + minZ;
		transform.localPosition = pos;
	}
}
