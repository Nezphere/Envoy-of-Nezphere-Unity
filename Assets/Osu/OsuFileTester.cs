using UnityEngine;

using System.IO;

public class OsuFileTester : MonoBehaviour {
	public OsuFile osuFile;

	void Start() {
		string path = Application.persistentDataPath;

		var dirs = Directory.GetDirectories(path);
		foreach (var dir in dirs) {
			print(dir);
			var files = Directory.GetFiles(dir);
			foreach (var file in files) {
				if (file.Substring(file.Length - 4, 4).ToLower() == ".osu") {
					print(file);
					var text = File.ReadAllText(file);
					osuFile = new OsuFile(text, "");
				}
			}
		}
	}
}
