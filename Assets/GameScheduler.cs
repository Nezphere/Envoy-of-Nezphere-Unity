using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Networking;

using Uif;

public class GameScheduler : MonoBehaviour {
	const string ApiUrl = "http://localhost:3000/v1";

	public RectTransform uiWelcomePanel, uiRankPanel, uiScorePanel;

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

	IEnumerator Start() {
		eventSystem.enabled = false;

		yield return Wait(2);

		uiWelcomePanel.gameObject.SetActive(true);
		yield return TweenManager.AddTween(panelShowTween.CreateTransition(step => {
			uiWelcomePanel.anchoredPosition = Vector2.LerpUnclamped(new Vector2(0, 5000), new Vector2(0, 0), step);
		}));

		yield return Wait(2);

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
}
