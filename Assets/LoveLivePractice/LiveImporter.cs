using File = System.IO.File;

using UnityEngine;

using LoveLivePractice.Api;

public class LiveImporter : MonoBehaviour {
	public string RemotePath = @"D:\LLP";
	public string LocalPath = @"D:\Unity\LLP-Core-Unity\Assets\Resources";

	public string liveName;
	public bool import;

	void OnValidate() {
		if (!import) return;
		import = false;

		var liveRemotePath = RemotePath + @"\lives\" + liveName;
		var liveLocalPath = LocalPath + @"\lives\" + liveName + ".json";

		string liveJson = File.ReadAllText(liveRemotePath);
		Debug.Log(liveJson);
		File.WriteAllText(liveLocalPath, liveJson);

		var live = JsonUtility.FromJson<ApiLiveResponse>(liveJson).content;
		Debug.Log(live.live_name);

		var mapRemotePath = RemotePath + @"\maps\" + live.map_path;
		var mapLocalPath = LocalPath + @"\maps\" + live.map_path;
		string mapJson = ApiLiveMap.Transform(File.ReadAllText(mapRemotePath));
		File.WriteAllText(mapLocalPath, mapJson);

		var bgmRemotePath = RemotePath + @"\bgms\" + live.bgm_path;
		var bgmLocalPath = LocalPath + @"\bgms\" + live.bgm_path + @".bytes";
		byte[] bgmBytes = File.ReadAllBytes(bgmRemotePath);
		File.WriteAllBytes(bgmLocalPath, bgmBytes);
	}
}
