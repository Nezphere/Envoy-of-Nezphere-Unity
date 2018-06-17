using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AudioFxConfig {
	public AudioClip clip;
	public float volume = 0.2f;

}

public class AudioFxPool : MonoBehaviour {
	public GameObject prototype;

	static readonly List<AudioSource> sourceList = new List<AudioSource>();

	public static AudioFxPool Instance;

	void Start() {
		Instance = this;
	}

	public static void Play(AudioFxConfig config, Vector3 position) {
		AudioSource source = null;
		foreach (var canditate in sourceList) {
			if (!canditate.isPlaying) {
				source = canditate;
				break;
			}
		}

		if (source == null) {
			var go = Instantiate(Instance.prototype, Instance.transform);
			source = go.GetComponent<AudioSource>();
			sourceList.Add(source);
		}

		source.transform.position = position;

		source.volume = config.volume;
		source.clip = config.clip;
		source.Play();
	}
}
