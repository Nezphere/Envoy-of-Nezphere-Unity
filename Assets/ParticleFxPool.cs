using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ParticleFxConfig {
	public string name;
	public int count = -1;
}

[System.Serializable]
public class ParticleFxPreset {
	public ParticleFxConfig[] configs;
}

public class ParticleFxPool : MonoBehaviour {
	public static readonly Dictionary<string, ParticleSystem> particlesByName = new Dictionary<string, ParticleSystem>();

	void Start() {
		foreach (var p in GetComponentsInChildren<ParticleSystem>()) {
			particlesByName.Add(p.name, p);
		}
	}

	public static void Emit(string name, int count, Vector3 position, Quaternion rotation) {
		var particle = particlesByName[name];
		particle.transform.position = position;
		particle.transform.rotation = rotation;
		if (count < 0) {
			particle.Emit((int)particle.emission.GetBurst(0).count.constant);
		} else {
			particle.Emit(count);
		}
	}

	public static void Emit(ParticleFxConfig config, Vector3 position, Quaternion rotation) {
		Emit(config.name, config.count, position, rotation);
	}

	public static void Emit(ParticleFxPreset preset, Vector3 position, Quaternion rotation) {
		foreach (var config in preset.configs) {
			Emit(config.name, config.count, position, rotation);
		}
	}
}
