using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tester : MonoBehaviour {
	public GameObject prototype;
	NoteCube instance;

	void Update() {
		if (instance == null) {
			instance = Instantiate(prototype, transform.position, transform.rotation).GetComponentInChildren<NoteCube>();
		}
	}
}
