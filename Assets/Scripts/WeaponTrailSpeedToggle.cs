using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponTrailSpeedToggle : MonoBehaviour {
	public WeaponTrail weaponTrail;
	public SwordTip tip;
	public float minVelocity = 10;

	public ParticleSystem particles;
	ParticleSystem.EmissionModule emission;
	public int rateOverDistance = 200, activeRateOverDistance = 400;

	public AudioClip clip;
	public float volume = 1;
	public Transform playPoint;

	void Awake() {
		emission = particles.GetComponent<ParticleSystem>().emission;
	}

	void Update() {
		if (tip.velocity.magnitude > minVelocity) {
			if (!weaponTrail.doEmit) {
				weaponTrail.trailIndex += 1;
				weaponTrail.doEmit = true;
				emission.rateOverTime = new ParticleSystem.MinMaxCurve(activeRateOverDistance);
//				AudioSource.PlayClipAtPoint(clip, playPoint.position, volume);
			}
			particles.Emit(10);
		} else {
			if (weaponTrail.doEmit) {
				weaponTrail.doEmit = false;
				emission.rateOverTime = new ParticleSystem.MinMaxCurve(rateOverDistance);
			}
		}
	}
}
