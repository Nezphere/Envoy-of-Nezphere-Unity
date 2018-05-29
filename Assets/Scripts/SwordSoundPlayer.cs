using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordSoundPlayer : MonoBehaviour {
	public SwordTip tip;
	public float minSpeed = 40;
	public AudioClip[] clips;
	public float volume = 1;
	public bool isActive = true;

	void Update() {
		if (tip.speed > minSpeed) {
			if (!isActive) {
				isActive = true;
				AudioSource.PlayClipAtPoint(clips[Random.Range(0, clips.Length)], transform.position, volume);
			}
		} else {
			isActive = false;
		}
	}
}
