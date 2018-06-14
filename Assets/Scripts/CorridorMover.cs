using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CorridorConfig {
	public GameObject prototype;
	public float spacing;
	public int minCount, maxCount;

	[HideInInspector]
	public GameObject go;
	[HideInInspector]
	public Transform trans;
	[HideInInspector]
	public int sections;
}

public class CorridorMover : MonoBehaviour {
	const float ViewDistance = 100;

	public CorridorConfig[] configs;

	int selectedIdx, nextIdx, selectedCount, currentCount;
	double startTime, releaseTime;
	bool isInTransition;

	void Start() {
		for (int i = 0; i < configs.Length; i++) {
			var config = configs[i];
			configs[i].go = new GameObject(config.prototype.name);
			configs[i].trans = configs[i].go.transform;
			configs[i].trans.parent = transform;
			configs[i].trans.localPosition = Vector3.zero;

			configs[i].sections = (int)(ViewDistance / config.spacing + 0.5f) * 2;
			for (int j = 0; j < configs[i].sections; j++) {
				var go = Instantiate(config.prototype, configs[i].trans);
				go.transform.localPosition = new Vector3(0, 0, j * config.spacing);
			}

			configs[i].go.SetActive(false);
		}

		selectedIdx = Random.Range(0, configs.Length);
		selectedCount = Random.Range(configs[selectedIdx].minCount, configs[selectedIdx].maxCount);
		currentCount = 0;
		isInTransition = false;
		configs[selectedIdx].go.SetActive(true);
	}

	void Update() {
		double time = LivePlayer.AccTime;

		if (isInTransition) {
			if (time > releaseTime) {  // Release!
				configs[nextIdx].trans.SetParent(transform, true);
				configs[selectedIdx].go.SetActive(false);
				selectedIdx = nextIdx;
				selectedCount = Random.Range(configs[selectedIdx].minCount, configs[selectedIdx].maxCount);
				currentCount = 0;
				isInTransition = false;
				startTime = time;
			}
		} else {
			float duration = Mathf.Abs(configs[selectedIdx].spacing / LivePlayer.Speed);
			
			if (time < startTime || time - startTime > duration) {
				startTime = time;
				currentCount += 1;
				
				if (currentCount > selectedCount) {  // Need transition
					nextIdx = Random.Range(0, configs.Length);

					if (nextIdx == selectedIdx) {  // Same selected
						selectedCount = Random.Range(configs[nextIdx].minCount, configs[nextIdx].maxCount);
						currentCount = 0;
					} else {  // Different, need transition
						configs[nextIdx].go.SetActive(true);
						configs[nextIdx].trans.parent = configs[selectedIdx].trans;  // Append to its end

						var totalSpacing = configs[selectedIdx].sections * configs[selectedIdx].spacing;
						configs[nextIdx].trans.localPosition = new Vector3(0, 0, totalSpacing);
						releaseTime = startTime + totalSpacing / Mathf.Abs(LivePlayer.Speed);

						isInTransition = true;
					}
				}
			}
		}

		configs[selectedIdx].trans.localPosition = new Vector3(0, 0, -(float)(time - startTime) * LivePlayer.Speed);
	}
}
