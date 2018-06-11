using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PillarConfig {
	public GameObject proto;

	public float maxZ = 100, minZ;
	public int count;
	public float spacing;
	public float delay;

	[HideInInspector]
	public GameObject go;
	[HideInInspector]
	public Transform trans;
}

public class PillarMover : MonoBehaviour {
	public PillarConfig[] configs;
	public PillarConfig selectedConfig;

	double startTime;

	void Start() {
		foreach (var config in configs) {
			config.go = new GameObject(config.proto.name);
			config.trans = config.go.transform;
			config.trans.SetParent(transform, false);

			for (int i = 0; i < config.count; i++) {
				var trans = Instantiate(config.proto, config.trans).transform;
				trans.localPosition = new Vector3(0, 0, config.spacing * i);
			}
		}

		StartNew(LivePlayer.accTime);
	}

	void StartNew(double time) {
		startTime = time;

		foreach (var config in configs) {
			config.go.SetActive(false);
		}

		selectedConfig = configs[Random.Range(0, configs.Length)];
		selectedConfig.go.SetActive(true);

		var count = selectedConfig.trans.childCount;
		for (int i = 0; i < count; i++) {
			var child = selectedConfig.trans.GetChild(i);
			child.localPosition = new Vector3(0, 0, selectedConfig.spacing * i);
		}
	}

	void Update() {
		if (selectedConfig == null) {
			return;
		}

		float duration = Mathf.Abs((selectedConfig.maxZ - selectedConfig.minZ) / LivePlayer.speed);
		double time = LivePlayer.accTime;

		if (time < startTime || time - startTime > duration) {
			StartNew(time);
		}

		var pos = selectedConfig.trans.localPosition;
		pos.z = (float)(time - startTime) / duration * (selectedConfig.maxZ - selectedConfig.minZ) + selectedConfig.minZ;
		selectedConfig.trans.localPosition = pos;
	}
}
