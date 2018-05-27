using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSoundPlayer : MonoBehaviour {
	public SwordTip tip;
	public float minVelocity = 40;
	public AudioClip[] clips;
	public float volume = 1;
	public bool isActive = true;

	void Update() {
		if (tip.velocity.magnitude > minVelocity) {
			if (!isActive) {
				isActive = true;
				AudioSource.PlayClipAtPoint(clips[Random.Range(0, clips.Length)], transform.position, volume);
			}
		} else {
			isActive = false;
		}
	}
}
