using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiveCube : MonoBehaviour {
	public Side side;
	public float minDyingSpeed = 12;
	public bool isDead;

	public float velocityScaling = 1, randomForce = 1, randomTorque = 1;

	public AudioClip[] clips;
	public float volume = 1;

	[Header("Live")]
	public double time;
	public float x, y;
	public bool shouldDie;
	// For debug
	public Heading heading;

	void OnTriggerEnter(Collider other) {
		if (isDead)
			return;

		var tip = other.GetComponentInChildren<SwordTip>();
		if (tip == null || tip.side != side)
			return;

		var down = -transform.up;
		float speed = Vector3.Dot(tip.velocity, down);

		if (speed > minDyingSpeed) {  // Die
			isDead = true;

			AudioSource.PlayClipAtPoint(clips[Random.Range(0, clips.Length)], transform.position, volume * 0.5f * speed / minDyingSpeed);

			foreach (var rigid in GetComponentsInChildren<Rigidbody>()) {
				rigid.isKinematic = false;
				rigid.velocity = tip.velocity * velocityScaling;
				rigid.AddRelativeForce(Random.insideUnitSphere * randomForce, ForceMode.Impulse);
				rigid.AddTorque(Random.insideUnitSphere * randomTorque, ForceMode.Impulse);
				rigid.transform.parent = null;
				Destroy(rigid.gameObject, 1);
			}

			foreach (var particles in GetComponentsInChildren<ParticleSystem>()) {
				particles.Play();
			}
		}
	}

	void OnTriggerStay(Collider other) {
		OnTriggerEnter(other);
	}
}
