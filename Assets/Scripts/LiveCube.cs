using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Uif;

public class LiveCube : MonoBehaviour {
	public Side side;
	public float minDyingSpeed = 12;

	public float velocityScaling = 1, randomForce = 1, randomTorque = 1;

	public AudioClip[] clips;
	public float volume = 1;

	public ParticleFxPreset closeFxPreset, hitFxPreset;

	[Header("Close")]
	public double startTime;
	public float closeDuration;
	public Transform leftShellTrans, rightShellTrans;
	public float leftShellStart, rightShellStart;
	public EasingType closeEasingType;
	public EasingPhase closeEasingPhase;
	public bool isClosed;

	[Header("Live")]
	public double time;
	public float startX, startY, x, y;
	public bool shouldDie;
	// For debug
	public Heading heading;

	public double closeTime;

	void Update() {
		if (isClosed) {
			return;
		}

		closeTime = LivePlayer.time - startTime;
		if (closeTime > closeDuration) {
			isClosed = true;
			leftShellTrans.localPosition = Vector3.zero;
			rightShellTrans.localPosition = Vector3.zero;
			ParticleFxPool.Emit(closeFxPreset, transform.position, transform.rotation);

			return;
		}

		leftShellTrans.localPosition = new Vector3(0, 0, Easing.Ease(closeEasingType, closeEasingPhase, leftShellStart, -leftShellStart, closeTime, closeDuration));
		rightShellTrans.localPosition = new Vector3(0, 0, Easing.Ease(closeEasingType, closeEasingPhase, rightShellStart, -rightShellStart, closeTime, closeDuration));
	}

	void OnTriggerEnter(Collider other) {
		if (shouldDie)
			return;

		var tip = other.GetComponentInChildren<SwordTip>();
		if (tip == null || tip.side != side)
			return;

		var down = -transform.up;
		float speed = Vector3.Dot(tip.velocity, down);

		if (speed > minDyingSpeed) {  // Die
			ParticleFxPool.Emit(hitFxPreset, transform.position, transform.rotation);

			AudioSource.PlayClipAtPoint(clips[Random.Range(0, clips.Length)], transform.position, volume * 0.5f * speed / minDyingSpeed);

			foreach (var rigid in GetComponentsInChildren<Rigidbody>()) {
				rigid.isKinematic = false;
				rigid.velocity = tip.velocity * velocityScaling;
				rigid.AddRelativeForce(Random.insideUnitSphere * randomForce, ForceMode.Impulse);
				rigid.AddTorque(Random.insideUnitSphere * randomTorque, ForceMode.Impulse);
				rigid.transform.parent = null;
				Destroy(rigid.gameObject, 1);
			}

			shouldDie = true;
		}
	}

	void OnTriggerStay(Collider other) {
		OnTriggerEnter(other);
	}
}
