using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponTrailSpeedToggle : MonoBehaviour {
	public MeleeWeaponTrail weaponTrail;
	public SwordTip tip;
	public float minVelocity = 10;

	void Update () {
		weaponTrail.Emit = tip.velocity.sqrMagnitude > minVelocity;
	}
}
