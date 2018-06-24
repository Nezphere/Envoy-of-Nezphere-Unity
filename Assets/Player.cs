using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NAudio.Wave;
using NAudio.MediaFoundation;

public class Player : MonoBehaviour {
	WaveOutEvent waveOut;
	AudioFileReader reader;

	public void Play(string path) {
		waveOut = new WaveOutEvent();
		MediaFoundationApi.Startup();
		reader = new AudioFileReader(path);
		waveOut.Init(reader);
		waveOut.Play();
	}

	void Update() {
		print(reader.CurrentTime.TotalMilliseconds);
	}

	void OnApplicationQuit() {
		waveOut.Stop();

		reader.Dispose();
		waveOut.Dispose();
	}
}
