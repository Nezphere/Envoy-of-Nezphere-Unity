using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NAudio.Wave;
using NAudio.Wave.SampleProviders;

public class MusicPlayer : MonoBehaviour {
	static MusicPlayer instance;

	public static float LiveTime, DeltaTime, AccTime, LastTime;
	public static bool IsPlaying;

	public LivePlayer livePlayer;
	public float offset;

	static WaveOutEvent waveOut;
	static AudioFileReader reader;
	static long lastReaderPosition;

	void Awake() {
		instance = this;

		waveOut = new WaveOutEvent();
	}

	void Update() {
		if (!(IsPlaying = (waveOut.PlaybackState == PlaybackState.Playing))) {
			LiveTime = LastTime = offset;
			DeltaTime = Time.deltaTime;
			AccTime += DeltaTime;
		} else {
			if (reader.Position != lastReaderPosition) {
				float time = (float)reader.Position / waveOut.OutputWaveFormat.AverageBytesPerSecond;
				LiveTime = offset + time;
				if (LiveTime > LastTime) {
					DeltaTime = LiveTime - LastTime;
					AccTime += DeltaTime;
				} else {
					DeltaTime = 0;
				}
				LastTime = LiveTime;
				lastReaderPosition = reader.Position;
			} else {
				DeltaTime = Time.deltaTime;
				LiveTime += DeltaTime;
				AccTime += DeltaTime;

				LastTime = LiveTime;
			}

		}
	
		livePlayer.ManualUpdate();
	}

	public void Play(string path) {
		reader = new AudioFileReader(path);
		reader.Volume = 0.2f;
		waveOut.Init(reader);
		waveOut.Play();
	}

	public static void Restart() {
		reader.Position = 0;
		lastReaderPosition = 0;
		waveOut.Play();
	}

	void OnApplicationQuit() {
		waveOut.Stop();
	}
}
