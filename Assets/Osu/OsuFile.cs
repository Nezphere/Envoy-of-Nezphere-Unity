[System.Serializable]
public class OsuFile {
	public string path;

	public string AudioFilename;
	public string Title;
	public string TitleUnicode;
	public string Artist;
	public string ArtistUnicode;
	public string Creator;
	public string Version;
	public string Source;
	public string Tags;
	public string BeatmapID;
	public string BeatmapSetID;

	public OsuHitObject[] HitObjects;

	public OsuFile(string text, string path) {
		this.path = path;

		var lines = text.Split(new [] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);

		for (int i = 0; i < lines.Length; i++) {
			string line = lines[i];

			if (line.Trim() == "[HitObjects]") {
				i += 1;
				ReadHitObjects(lines, ref i);
				break;
			}

			int index = line.IndexOf(':');
			if (index < 0) {
				continue;
			}

			string field = line.Substring(0, index).Trim(), value = line.Substring(index + 1).Trim();
//			UnityEngine.Debug.Log(field + " : " + value);
			switch (field) {
			case "AudioFilename":
				AudioFilename = value;
				break;
			case "Title":
				Title = value;
				break;
			case "TitleUnicode":
				TitleUnicode = value;
				break;
			case "Artist":
				Artist = value;
				break;
			case "ArtistUnicode":
				ArtistUnicode = value;
				break;
			case "Creator":
				Creator = value;
				break;
			case "Version":
				Version = value;
				break;
			case "Source":
				Source = value;
				break;
			case "Tags":
				Tags = value;
				break;
			case "BeatmapID":
				BeatmapID = value;
				break;
			case "BeatmapSetID":
				BeatmapSetID = value;
				break;
			}
		}
	}

	void ReadHitObjects(string[] lines, ref int i) {
		var list = new System.Collections.Generic.List<OsuHitObject>();
		bool flag = true;
		while (flag) {
			try {
				list.Add(new OsuHitObject(lines[i]));
			} catch {
				flag = false;
			}
			i += 1;
		}
		HitObjects = list.ToArray();
	}

	public LiveNote[] GetLiveNotes(AxisTransformer2 transformer) {
		var list = new System.Collections.Generic.List<LiveNote>();
		OsuHitObject lastObject = null;
		foreach (var hitObject in HitObjects) {
			bool isPara = lastObject != null && lastObject.time == hitObject.time;
			if (isPara) {
				list[list.Count - 1].isPara = true;
			}

			list.Add(new LiveNote(
				transformer(hitObject.x / 512f, hitObject.y / 384f), 
				hitObject.time / 1000f, 
				isPara, 
				(hitObject.hitSound != 0)));
			lastObject = hitObject;
		}
		return list.ToArray();
	}
}

[System.Serializable]
public class OsuHitObject {
	public int x, y;
	public int time;
	public int type;
	public int hitSound;

	public OsuHitObject(string line) {
		var segments = line.Split(new [] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
		x = int.Parse(segments[0]);
		y = int.Parse(segments[1]);
		time = int.Parse(segments[2]);
		type = int.Parse(segments[3]);
		hitSound = int.Parse(segments[4]);
	}
}