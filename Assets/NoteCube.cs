using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteCube : MonoBehaviour {
	public SwordTip.Side side;
	public float minDyingSpeed = 12;
	public bool isDead;

	public AudioClip[] clips;
	public float volume = 1;

	void OnTriggerEnter(Collider other) {
		if (isDead)
			return;

		var tip = other.GetComponentInChildren<SwordTip>();
		if (tip == null)// || tip.side != side)
			return;

		var down = -transform.up;
		float speed = Vector3.Dot(tip.velocity, down);

		if (speed > minDyingSpeed) {  // Die
			isDead = true;

			AudioSource.PlayClipAtPoint(clips[Random.Range(0, clips.Length)], transform.position, volume);

			foreach (var rigid in GetComponentsInChildren<Rigidbody>()) {
				rigid.isKinematic = false;
			}

			foreach (var particles in GetComponentsInChildren<ParticleSystem>()) {
				particles.Play();
			}

			Destroy(this, 0.2f);
		}
	}

	void OnTriggerStay(Collider other) {
		OnTriggerEnter(other);
	}
}
