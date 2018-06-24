using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;

public class OsuLiveStarter : MonoBehaviour {
	public string beatmapId;

	void Start() {
		string path = Application.persistentDataPath;

		var dirs = Directory.GetDirectories(path);
		OsuFile osuFile = null;
		foreach (var dir in dirs) {
			print(dir);
			var files = Directory.GetFiles(dir);
			foreach (var file in files) {
				if (file.Substring(file.Length - 4, 4).ToLower() == ".osu") {
					print(file);
					var text = File.ReadAllText(file);
					osuFile = new OsuFile(text, dir);
					if (osuFile.BeatmapID == beatmapId)
						break;
				}
			}
		}

		GetComponent<Player>().Play(Path.Combine(osuFile.path, osuFile.AudioFilename));
//		var www = new WWW(GetLocalFileUrl(Path.Combine(osuFile.path, osuFile.AudioFilename)));
//		yield return www;
//		var bgm = www.GetAudioClip();

//		var notes = osuFile.GetLiveNotes((x, y) => new Vector2(x * 4, y * 2));

//		GetComponent<LivePlayer>().InitGame(notes, bgm);
	}

	static string GetLocalFileUrl(string path) {
		return "file:///" + path;
	}
}
