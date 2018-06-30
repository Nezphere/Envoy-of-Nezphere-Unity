using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Networking;

using Uif;

public class GameScheduler : MonoBehaviour {
	public const string ApiUrl = "http://localhost:3000/v1";
	public static GameScheduler Instance;

	public RectTransform uiWelcomePanel;

	public Camera vrCamera, uiCamera;

	public EventSystem eventSystem;
	public Text uiErrorText;

	public Tween panelShowTween, panelHideTween;

	[Header("Login Panel")]
	public RectTransform uiLoginPanel;
	public InputField uiLoginNameInput, uiLoginPassInput;
	public Text uiLoginResponseText;
	public bool isLoginFinished;

	[Header("Song Panel")]
	public RectTransform uiSongPanel;
	public bool isSongSelectFinished;
	public OsuFile osuFile;

	[Header("Game Panels")]
	public RectTransform uiRankPanel, uiScorePanel;
	public RankPanelController rankPanelController;
	readonly List<RankInfo> ranks = new List<RankInfo>();
	readonly RankInfo rank = new RankInfo();
	public bool isSongFinished;
	public MusicPlayer musicPlayer;
	public LivePlayer livePlayer;

	[Header("Result Panel")]
	public RectTransform uiResultPanel;
	public ResultPanelUiController resultPanelUiController;
	float acc;

	void Awake() {
		Instance = this;
	}

	IEnumerator Start() {
		eventSystem.enabled = false;

		yield return Wait(1);

		uiWelcomePanel.gameObject.SetActive(true);
		yield return TweenManager.AddTween(panelShowTween.CreateTransition(step => {
			uiWelcomePanel.anchoredPosition = Vector2.LerpUnclamped(new Vector2(0, 5000), new Vector2(0, 0), step);
		}));

		yield return Wait(1);

		yield return TweenManager.AddTween(panelHideTween.CreateTransition(step => {
			uiWelcomePanel.anchoredPosition = Vector2.LerpUnclamped(new Vector2(0, 0), new Vector2(0, -5000), step);
		}));
		uiWelcomePanel.gameObject.SetActive(false);

		uiLoginPanel.gameObject.SetActive(true);
		yield return TweenManager.AddTween(panelShowTween.CreateTransition(step => {
			uiLoginPanel.anchoredPosition = Vector2.LerpUnclamped(new Vector2(0, 5000), new Vector2(0, 80), step);
		}));

		eventSystem.enabled = true;
		while (!isLoginFinished)
			yield return null;
		eventSystem.enabled = false;

		yield return TweenManager.AddTween(panelHideTween.CreateTransition(step => {
			uiLoginPanel.anchoredPosition = Vector2.LerpUnclamped(new Vector2(0, 80), new Vector2(0, -5000), step);
		}));
		uiLoginPanel.gameObject.SetActive(false);

		uiSongPanel.gameObject.SetActive(true);
		yield return TweenManager.AddTween(panelShowTween.CreateTransition(step => {
			uiSongPanel.anchoredPosition = Vector2.LerpUnclamped(new Vector2(0, 5000), new Vector2(0, 0), step);
		}));

		eventSystem.enabled = true;
		while (!isSongSelectFinished)
			yield return null;
		eventSystem.enabled = false;

		yield return TweenManager.AddTween(panelHideTween.CreateTransition(step => {
			uiSongPanel.anchoredPosition = Vector2.LerpUnclamped(new Vector2(0, 0), new Vector2(0, -5000), step);
		}));
		uiSongPanel.gameObject.SetActive(false);

		uiCamera.enabled = false;

		uiRankPanel.gameObject.SetActive(true);
		uiScorePanel.gameObject.SetActive(true);
		rankPanelController.SetRank(new RankInfo[0]);
		yield return TweenManager.AddTween(panelShowTween.CreateTransition(step => {
			uiRankPanel.anchoredPosition = Vector2.LerpUnclamped(new Vector2(0, 5000), new Vector2(-400, 0), step);
			uiScorePanel.anchoredPosition3D = Vector3.LerpUnclamped(new Vector3(0, 5000, 0), new Vector3(0, 70, 100), step);
		}));

		rank.name = GlobalStatic.Name;
		rank.rank = "--";
		rank.score = 0;
		ranks.Clear();
		ranks.Add(rank);
		rankPanelController.SetRank(ranks.ToArray());

		StartCoroutine(HandleRankInit());
		print(osuFile.path);
		print(osuFile.AudioFilename);
		musicPlayer.Play(System.IO.Path.Combine(osuFile.path, osuFile.AudioFilename));
		var notes = osuFile.GetLiveNotes((x, y) => new Vector2((x - 0.5f) * 4, y * 1.5f - 0.5f));

		livePlayer.notes = notes;
		livePlayer.StartGame();

		while (!isSongFinished)
			yield return null;

		uiResultPanel.gameObject.SetActive(true);
		yield return TweenManager.AddTween(panelShowTween.CreateTransition(step => {
			uiResultPanel.anchoredPosition = Vector2.LerpUnclamped(new Vector2(0, 5000), new Vector2(0, 0), step);
		}));

		acc = (300f * livePlayer.perfect + 200f * livePlayer.great + 100f * livePlayer.good + 50f * livePlayer.bad) / (300f * livePlayer.total);
		resultPanelUiController.SetResults(osuFile.TitleUnicode, osuFile.Version, acc.ToString("P0"),
			false, livePlayer.score, livePlayer.perfect, livePlayer.great, livePlayer.good, livePlayer.bad, livePlayer.miss,
			false, livePlayer.maxCombo, livePlayer.maxCombo >= livePlayer.total);

		StartCoroutine(HandleSubmitResult());
	}

	IEnumerator HandleSubmitResult() {
		var form = new WWWForm();
		form.AddField("hash", osuFile.BeatmapID);
		form.AddField("name", osuFile.TitleUnicode);
		form.AddField("accuracy", acc.ToString());
		form.AddField("combo", livePlayer.combo);
		form.AddField("score", livePlayer.score);
		form.AddField("perfect", livePlayer.perfect);
		form.AddField("great", livePlayer.great);
		form.AddField("good", livePlayer.good);
		form.AddField("bad", livePlayer.bad);
		form.AddField("miss", livePlayer.miss);

		using (var req = UnityWebRequest.Post(ApiUrl + "/trials/submit", form)) {
			yield return req.SendWebRequest();
			if (!req.isNetworkError && !req.isHttpError) {
				resultPanelUiController.SetRank(req.downloadHandler.text);
			}
		}
	}

	public void SetScore(int score) {
		rank.score = score;
		ranks.Sort();
		rankPanelController.SetRank(ranks.ToArray());
	}

	IEnumerator HandleRankInit() {
		var form = new WWWForm();
		form.AddField("hash", osuFile.BeatmapID);
		form.AddField("count", 10);

		using (var req = UnityWebRequest.Post(ApiUrl + "/trials/ranking", form)) {
			yield return req.SendWebRequest();
			if (!req.isNetworkError && !req.isHttpError) {
				var rankRes = JsonUtility.FromJson<RankInfoResponse>(req.downloadHandler.text);
				ranks.AddRange(rankRes.ranking);
				ranks.Sort();
				rankPanelController.SetRank(ranks.ToArray());
			}
		}
	}

	static WaitForSeconds Wait(float time) {
		return new WaitForSeconds(time);
	}

	public void OnLoginButtonClicked() {
		StartCoroutine(HandleLogin());
	}

	IEnumerator HandleLogin() {
		eventSystem.enabled = false;

		var form = new WWWForm();
		form.AddField("name", uiLoginNameInput.text);
		form.AddField("pass", uiLoginPassInput.text);

		using (var req = UnityWebRequest.Post(ApiUrl + "/friends/login", form)) {
			yield return req.SendWebRequest();
			if (req.isNetworkError || req.isHttpError) {
				uiErrorText.text = ApiErrorText(req);
			} else {
				uiErrorText.text = "";
				uiLoginResponseText.text = CapText("login successful");
				GlobalStatic.Session = req.downloadHandler.text;
				GlobalStatic.Name = uiLoginNameInput.text;
				isLoginFinished = true;
			}
		}

		eventSystem.enabled = true;
	}

	public void OnRegisterButtonClicked() {
		StartCoroutine(HandleRegister());
	}

	public void OnLoginCancelButtonClicked() {
		isLoginFinished = true;
		GlobalStatic.Name = "--";
	}

	IEnumerator HandleRegister() {
		eventSystem.enabled = false;

		var form = new WWWForm();
		form.AddField("name", uiLoginNameInput.text);
		form.AddField("pass", uiLoginPassInput.text);

		using (var req = UnityWebRequest.Post(ApiUrl + "/friends/register", form)) {
			yield return req.SendWebRequest();
			if (req.isNetworkError || req.isHttpError) {
				uiErrorText.text = ApiErrorText(req);
			} else {
				uiErrorText.text = "";
				uiLoginResponseText.text = CapText("register successful");
			}
		}

		eventSystem.enabled = true;
	}

	static string ApiErrorText(UnityWebRequest req) {
		return CapText(req.error) + "\n\n" + req.downloadHandler.text;
	}

	static string CapText(string text) {
		return "| " + text.ToUpper() + " |";
	}

	public void OnOsuSongSelected(OsuFile file) {
		osuFile = file;
		isSongSelectFinished = true;
	}
}

[System.Serializable]
public class RankInfoResponse {
	public RankInfo[] ranking;
}