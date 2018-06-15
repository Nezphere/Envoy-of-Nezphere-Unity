using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FlashFxConfig {
	public Color color;
	public float intensity, range;
	public Tween tween;
}

public class FlashFxPool : MonoBehaviour {
	public static FlashFxPool Instance;

	static readonly List<Light> lightList = new List<Light>();

	void Start() {
		Instance = this;
	}

	public static void Flash(FlashFxConfig config, Vector3 position) {
		Light light = null;
		foreach (var canditate in lightList) {
			if (!canditate.enabled) {
				light = canditate;
				light.enabled = true;
				break;
			}
		}

		if (light == null) {
			var go = new GameObject("FlashLight", typeof(Light));
			light = go.GetComponent<Light>();
			lightList.Add(light);
		}

		light.transform.parent = Instance.transform;
		light.transform.position = position;

		light.color = config.color;
		light.intensity = config.intensity;
		light.range = config.range;

		TweenManager.AddTween(new Tween(config.tween.duration, config.tween.easingType, config.tween.easingPhase, step => {
			light.intensity = Mathf.Lerp(config.intensity, 0, step);
		}, () => {
			light.enabled = false;
		}));
	}
}
