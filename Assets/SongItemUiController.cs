using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class SongItemUiController : MonoBehaviour {
	public Text uiLeftText, uiMiddleText, uiRightText;
	public OsuFile osuFile;

	public void Init(OsuFile osuFile) {
		uiLeftText.text = string.Format(
			"<size=12>{0}</size>\n<size=10>{1}</size>", 
			osuFile.Version, 
			osuFile.Source);
		uiMiddleText.text = string.Format(
			"<size=12>{0}</size>\n<size=8>{1}</size>", 
			osuFile.TitleUnicode, 
			osuFile.ArtistUnicode);
		uiRightText.text = string.Format(
			"<size=14>{0}</size>\n<size=12>{1}</size>", 
			"--", 
			"--");

		if (GlobalStatic.IsLoggedIn) {
			StartCoroutine(InitHandler());
		}
	}

	IEnumerator InitHandler() {
		var form = new WWWForm();
		form.AddField("session", GlobalStatic.Session);
		form.AddField("hash", osuFile.BeatmapID);

		using (var req = UnityWebRequest.Post("http://localhost:3000/trials/high-score", form)) {
			yield return req.SendWebRequest();
			if (!req.isNetworkError && !req.isHttpError) {
				var segs = req.downloadHandler.text.Split(new [] { ':' }, System.StringSplitOptions.RemoveEmptyEntries);
				uiRightText.text = string.Format(
					"<size=14>{0}</size>\n<size=12>{1}</size>", 
					segs[0].ToUpper(), 
					segs[1]);
			}
		}
	}
}
