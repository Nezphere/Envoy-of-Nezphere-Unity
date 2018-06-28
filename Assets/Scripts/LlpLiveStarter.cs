using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using LoveLivePractice.Api;

public class LlpLiveStarter : MonoBehaviour {
	static readonly Vector2[] startSlots = {
		new Vector2(-4f, 4f),
		new Vector2(-3f, 3f),
		new Vector2(-2f, 2f),
		new Vector2(-1f, 1f),

		new Vector2(0f, 0f),

		new Vector2(1f, 1f),
		new Vector2(2f, 2f),
		new Vector2(3f, 3f),
		new Vector2(4f, 4f),
	};
	static readonly Vector2[] slots = {
		new Vector2(-2f, 2f),
		new Vector2(-2f, 1f),
		new Vector2(-1f, 1f),
		new Vector2(-1f, 0f),

		new Vector2(0f, 0f),

		new Vector2(1f, 0f),
		new Vector2(1f, 1f),
		new Vector2(2f, 1f),
		new Vector2(2f, 2f),
	};

	public string liveName;

	void Start() {
		ApiLive live;
		using (var liveAsset = new FResource<TextAsset>("lives/" + liveName)) {
			live = JsonUtility.FromJson<ApiLiveResponse>(liveAsset.asset.text).content;
		}

		ApiLiveMap map;
		using (var mapAsset = new FResource<TextAsset>("maps/" + live.map_path.Replace(".json", ""))) {
			map = JsonUtility.FromJson<ApiLiveMap>(mapAsset.asset.text);
			System.Array.Sort(map.lane);
		}

		var bgm = Resources.Load<AudioClip>("bgms/" + live.bgm_path.Replace(".mp3", ""));

		var notes = map.GetLiveNotes((x, _) => slots[Mathf.RoundToInt(x)] * 0.5f);

		GetComponent<LivePlayer>().notes = notes;
		GetComponent<LivePlayer>().StartGame();
	}
}
