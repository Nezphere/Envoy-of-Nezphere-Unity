using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.EventSystems;

public class LoginUiController : MonoBehaviour {
	public InputField uiNameInput, uiPassInput;
	public Text uiErrorText, uiResponseText;
	public EventSystem eventSystem;

	public void OnLoginButtonClicked() {
		StartCoroutine(HandleLogin());
	}

	IEnumerator HandleLogin() {
		eventSystem.enabled = false;

		var form = new WWWForm();
		form.AddField("name", uiNameInput.text);
		form.AddField("pass", uiPassInput.text);

		using (var req = UnityWebRequest.Post("http://localhost:3000/login", form)) {
			yield return req.SendWebRequest();
			if (req.isNetworkError || req.isHttpError) {
				uiErrorText.text = string.Format("| {0} |\n{1}", req.error, req.downloadHandler.text);
				eventSystem.enabled = true;
			} else {
				uiErrorText.text = "";
				uiResponseText.text = string.Format("| {0} |", req.downloadHandler.text);
			}
		}
	}

	public void OnRegisterButtonClicked() {
		StartCoroutine(HandleRegister());
	}

	IEnumerator HandleRegister() {
		eventSystem.enabled = false;

		var form = new WWWForm();
		form.AddField("name", uiNameInput.text);
		form.AddField("pass", uiPassInput.text);

		using (var req = UnityWebRequest.Post("http://localhost:3000/register", form)) {
			yield return req.SendWebRequest();
			if (req.isNetworkError || req.isHttpError) {
				uiErrorText.text = string.Format("| {0} |\n{1}", req.error, req.downloadHandler.text);
			} else {
				uiErrorText.text = "";
				uiResponseText.text = string.Format("| {0} |", req.downloadHandler.text);
			}
		}

		eventSystem.enabled = true;
	}
}
