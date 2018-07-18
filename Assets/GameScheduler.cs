using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Networking;

using VrInput;
using LoveLivePractice.Api;

public class GameScheduler : MonoBehaviour {
	//	#if UNITY_EDITOR
	//	public const string ApiUrl = "http://localhost:3000/v1";
	//	#else
	public const string ApiUrl = "https://api.nezphere.com/v1";
	//	#endif
	public static GameScheduler Instance;

	public RectTransform uiWelcomePanel;

	//	public Camera vrCamera, uiCamera;

	public EventSystem eventSystem;
	public LaserVrPointer pointer;
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
	public ApiLive live;
	public int selectedFileType;
	public string mapHash, mapName, mapVersion;

	[Header("Game Panels")]
	public RectTransform uiRankPanel, uiScorePanel, uiHintPanel;
	public RankPanelController rankPanelController;
	readonly List<RankInfo> ranks = new List<RankInfo>();
	readonly RankInfo rank = new RankInfo();
	public bool isSongFinished;
	public MusicPlayer musicPlayer;
	public LivePlayer livePlayer;

	[Header("Result Panel")]
	public RectTransform uiResultPanel;
	public ResultPanelUiController resultPanelUiController;
	public bool willRetry, willShowMenu;
	float acc;

	void Awake() {
		Instance = this;

		Application.logMessageReceived += (condition, stackTrace, type) => {
			if (type == LogType.Error || type == LogType.Exception) {
				uiErrorText.text = stackTrace;
			}
		};
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


		if (!string.IsNullOrEmpty(PlayerPrefs.GetString("friend_name", ""))) {
			uiLoginNameInput.text = PlayerPrefs.GetString("friend_name");
			uiLoginPassInput.text = PlayerPrefs.GetString("friend_pass");
			OnLoginButtonClicked();
		}

		isLoginFinished = false;
		eventSystem.enabled = true;
		pointer.SetActive(true);
		while (!isLoginFinished)
			yield return null;
		eventSystem.enabled = false;
		pointer.SetActive(false);

		yield return TweenManager.AddTween(panelHideTween.CreateTransition(step => {
			uiLoginPanel.anchoredPosition = Vector2.LerpUnclamped(new Vector2(0, 80), new Vector2(0, -5000), step);
		}));
		uiLoginPanel.gameObject.SetActive(false);

		SONG_SELECT:
		uiSongPanel.gameObject.SetActive(true);
		yield return TweenManager.AddTween(panelShowTween.CreateTransition(step => {
			uiSongPanel.anchoredPosition = Vector2.LerpUnclamped(new Vector2(0, 5000), new Vector2(0, 0), step);
		}));

		eventSystem.enabled = true;
		pointer.SetActive(true);

		isSongSelectFinished = false;
		selectedFileType = -1;
		while (!isSongSelectFinished)
			yield return null;
		eventSystem.enabled = false;
		pointer.SetActive(false);

		yield return TweenManager.AddTween(panelHideTween.CreateTransition(step => {
			uiSongPanel.anchoredPosition = Vector2.LerpUnclamped(new Vector2(0, 0), new Vector2(0, -5000), step);
		}));
		uiSongPanel.gameObject.SetActive(false);

//		uiCamera.enabled = false;

		uiRankPanel.gameObject.SetActive(true);
		uiScorePanel.gameObject.SetActive(true);
		uiHintPanel.gameObject.SetActive(true);
		rankPanelController.SetRank(new RankInfo[0]);
		yield return TweenManager.AddTween(panelShowTween.CreateTransition(step => {
			uiRankPanel.anchoredPosition = Vector2.LerpUnclamped(new Vector2(0, 5000), new Vector2(-400, 0), step);
			uiScorePanel.anchoredPosition3D = Vector3.LerpUnclamped(new Vector3(0, 5000, 0), new Vector3(0, 70, 100), step);
			uiHintPanel.anchoredPosition3D = Vector3.LerpUnclamped(new Vector3(0, 0, 5000), Vector3.zero, step);
		}));

		StartCoroutine(HandleCloseHintPanel());

		rank.name = GlobalStatic.Name;
		rank.rank = "--";
		rank.score = 0;
		ranks.Clear();
		ranks.Add(rank);
		rankPanelController.SetRank(ranks.ToArray());

		LiveNote[] notes = null;

		if (selectedFileType == 0) {
			notes = osuFile.GetLiveNotes((x, y) => new Vector2((x - 0.5f) * 4, y * 1.5f - 0.5f));
			musicPlayer.Play(System.IO.Path.Combine(osuFile.path, osuFile.AudioFilename));
		} else {  // selectedFileType == 1
			using (var mapAsset = new FResource<TextAsset>("maps/" + live.map_path.Replace(".json", ""))) {
				var map = JsonUtility.FromJson<ApiLiveMap>(mapAsset.asset.text);
				System.Array.Sort(map.lane);
				notes = map.GetLiveNotes((x, _) => LlpLiveStarter.slots[Mathf.RoundToInt(x)] * 0.75f, 2);
			}
			musicPlayer.Play(System.IO.Path.Combine(Application.persistentDataPath, live.bgm_path));
		}

		livePlayer.notes = notes;
		livePlayer.StartGame();

		SONG_START:
		uiErrorText.text = "";
		StartCoroutine(HandleRankInit());
		isSongFinished = false;
		while (!isSongFinished)
			yield return null;

		yield return TweenManager.AddTween(panelHideTween.CreateTransition(step => {
			uiRankPanel.anchoredPosition = Vector2.LerpUnclamped(new Vector2(-400, 0), new Vector2(0, 5000), step);
			uiScorePanel.anchoredPosition3D = Vector3.LerpUnclamped(new Vector3(0, 70, 100), new Vector3(0, 5000, 0), step);
		}));

		uiResultPanel.gameObject.SetActive(true);
		acc = (300f * livePlayer.perfect + 200f * livePlayer.great + 100f * livePlayer.good + 50f * livePlayer.bad) / (300f * livePlayer.total);
		resultPanelUiController.SetResults(mapName, mapVersion, acc.ToString("P0"),
			false, livePlayer.score, livePlayer.perfect, livePlayer.great, livePlayer.good, livePlayer.bad, livePlayer.miss,
			false, livePlayer.maxCombo, livePlayer.maxCombo >= livePlayer.total);
		
		yield return TweenManager.AddTween(panelShowTween.CreateTransition(step => {
			uiResultPanel.anchoredPosition = Vector2.LerpUnclamped(new Vector2(0, 5000), new Vector2(0, 0), step);
		}));

		if (GlobalStatic.IsLoggedIn) {
			StartCoroutine(HandleSubmitResult());
		}

		willRetry = willShowMenu = false;
		eventSystem.enabled = true;
		pointer.SetActive(true);
//		uiCamera.enabled = true;
		while (!willRetry && !willShowMenu)
			yield return null;
//		uiCamera.enabled = false;
		eventSystem.enabled = false;
		pointer.SetActive(false);

		yield return TweenManager.AddTween(panelHideTween.CreateTransition(step => {
			uiResultPanel.anchoredPosition = Vector2.LerpUnclamped(new Vector2(0, 0), new Vector2(0, 5000), step);
		}));

		if (willRetry) {
			livePlayer.StartGame();
			MusicPlayer.Restart();
			yield return TweenManager.AddTween(panelShowTween.CreateTransition(step => {
				uiRankPanel.anchoredPosition = Vector2.LerpUnclamped(new Vector2(0, 5000), new Vector2(-400, 0), step);
				uiScorePanel.anchoredPosition3D = Vector3.LerpUnclamped(new Vector3(0, 5000, 0), new Vector3(0, 70, 100), step);
			}));
			goto SONG_START;
		} else {
			goto SONG_SELECT;
		}
	}

	IEnumerator HandleCloseHintPanel() {
		yield return Wait(5);
		TweenManager.AddTween(new Tween(4, Uif.EasingType.Circ, Uif.EasingPhase.In, step => {
			uiHintPanel.anchoredPosition3D = Vector3.LerpUnclamped(Vector3.zero, new Vector3(0, 0, 10000), step);
		}, () => uiHintPanel.gameObject.SetActive(false)));
	}

	IEnumerator HandleSubmitResult() {
		if (string.IsNullOrEmpty(mapHash) || !GlobalStatic.IsLoggedIn) {
			yield break;
		}

		var form = new WWWForm();
		form.AddField("hash", mapHash);
		form.AddField("name", mapName);
		form.AddField("accuracy", acc.ToString());
		form.AddField("combo", livePlayer.combo);
		form.AddField("score", livePlayer.score);
		form.AddField("perfect", livePlayer.perfect);
		form.AddField("great", livePlayer.great);
		form.AddField("good", livePlayer.good);
		form.AddField("bad", livePlayer.bad);
		form.AddField("miss", livePlayer.miss);
		form.AddField("session", GlobalStatic.Session);

		using (var req = UnityWebRequest.Post(ApiUrl + "/trials/submit", form)) {
			yield return req.SendWebRequest();
			if (!req.isNetworkError && !req.isHttpError) {
				resultPanelUiController.SetRank(req.downloadHandler.text);
			} else {
				ApiErrorText(req);
			}
		}
	}

	public void SetScore(int score) {
		rank.score = score;
		ranks.Sort();
		rankPanelController.SetRank(ranks.ToArray());
	}

	IEnumerator HandleRankInit() {
		if (string.IsNullOrEmpty(mapHash)) {
			yield break;
		}

		var form = new WWWForm();
		form.AddField("hash", mapHash);
		form.AddField("count", 10);

		using (var req = UnityWebRequest.Post(ApiUrl + "/trials/ranking", form)) {
			yield return req.SendWebRequest();
			if (!req.isNetworkError && !req.isHttpError) {
				var rankRes = JsonUtility.FromJson<RankInfoResponse>(req.downloadHandler.text);
				ranks.AddRange(rankRes.ranking);
				ranks.Sort();
				rankPanelController.SetRank(ranks.ToArray());
			} else {
				ApiErrorText(req);
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
		string name = uiLoginNameInput.text, pass = uiLoginPassInput.text;
		form.AddField("name", name);
		form.AddField("pass", pass);

		using (var req = UnityWebRequest.Post(ApiUrl + "/friends/login", form)) {
			yield return req.SendWebRequest();
			if (req.isNetworkError || req.isHttpError) {
				uiErrorText.text = ApiErrorText(req);
			} else {
				uiErrorText.text = "";
				uiLoginResponseText.text = CapText("login successful");
				GlobalStatic.Session = req.downloadHandler.text;
				GlobalStatic.Name = name;
				PlayerPrefs.SetString("friend_name", name);
				PlayerPrefs.SetString("friend_pass", pass);
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
		selectedFileType = 0;
		osuFile = file;
		mapHash = file.BeatmapID;
		mapVersion = file.Version;
		mapName = file.TitleUnicode;
		isSongSelectFinished = true;
	}

	public void OnLlpSongSelected(ApiLive live) {
		selectedFileType = 1;
		this.live = live;
		mapHash = live.live_id;
		mapVersion = live.category.name;
		mapName = live.live_name;
		isSongSelectFinished = true;
	}

	public void OnRetryButtonClicked() {
		willRetry = true;
	}

	public void OnMenuButtonClicked() {
		willShowMenu = true;
	}
}

[System.Serializable]
public class RankInfoResponse {
	public RankInfo[] ranking;
}