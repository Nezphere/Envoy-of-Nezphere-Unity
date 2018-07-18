using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using LoveLivePractice.Api;

public class LlpSongItemImporter : MonoBehaviour {
	public string liveName;
	public LlpSongItemUiController uiController;

	void Start() {
		ApiLive live;
		using (var liveAsset = new FResource<TextAsset>("lives/" + liveName)) {
			live = JsonUtility.FromJson<ApiLiveResponse>(liveAsset.asset.text).content;
		}

		using (var bgmAsset = new FResource<TextAsset>("bgms/" + live.bgm_path)) {
			var bgmPath = System.IO.Path.Combine(Application.persistentDataPath, live.bgm_path);
			if (!System.IO.File.Exists(bgmPath)) {
				System.IO.File.WriteAllBytes(bgmPath, bgmAsset.asset.bytes);	
			} 
		}

		uiController.Init(live);
	}
}
