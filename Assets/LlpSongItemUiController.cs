using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

using LoveLivePractice.Api;

public class LlpSongItemUiController : MonoBehaviour {
	public Text uiLeftText, uiMiddleText, uiRightText;
	public ApiLive live;


	public void Init(ApiLive live) {
		this.live = live;

		uiLeftText.text = string.Format(
			"<size=12>{0}</size>\n<size=10>{1}</size>", 
			live.category.name, 
			"by " + live.upload_user.username);
		uiMiddleText.text = string.Format(
			"<size=12>{0}</size>\n<size=8>{1}</size>", live.live_name, live.artist);
		uiRightText.text = string.Format(
			"<size=14>{0}</size>\n<size=12>{1}</size>", 
			"--", 
			"--");

		if (GlobalStatic.IsLoggedIn) {
			StartCoroutine(InitHandler());
		}
	}

	IEnumerator InitHandler() {
		if (string.IsNullOrEmpty(live.live_id))
			yield break;

		var form = new WWWForm();
		form.AddField("session", GlobalStatic.Session);
		form.AddField("hash", live.live_id);

		using (var req = UnityWebRequest.Post(GameScheduler.ApiUrl + "/trials/high-score", form)) {
			yield return req.SendWebRequest();
			if (!req.isNetworkError && !req.isHttpError) {
				var segs = req.downloadHandler.text.Split(new [] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
				uiRightText.text = string.Format(
					"<size=14>{0}</size>\n<size=12>{1}</size>", 
					segs[0].ToUpper(), 
					segs[1]);
			}
		}
	}

	public void OnButtonClicked() {
		GameScheduler.Instance.OnLlpSongSelected(live);
	}
}
