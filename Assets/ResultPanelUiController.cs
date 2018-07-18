using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Uif;

public class ResultPanelUiController : MonoBehaviour {
	public Text uiTitleText, uiVersionText;
	public Text uiRankText, uiAccText;
	public Text uiScoreNewText, uiScoreText, uiPerfectText, uiGreatText, uiGoodText, uiBadText, uiMissText;
	public Text uiComboNewText, uiComboText, uiFullComboText;

	public Tween tween;

	[ContextMenu("Test")]
	void TestResults() {
		SetResults("ファティマ (TV Size)", "Hard", "49%", true, 4353244, 324, 23, 32, 54, 2, true, 3254, true);
	}

	public void SetResults(
		string title, string version, string acc,
		bool isScoreNew, int score, int perfect, int great, int good, int bad, int miss,
		bool isComboNew, int combo, bool isFullCombo) {
		acc = acc.Replace(" ", "");
		StringLerp titleLerp = new StringLerp("", title), versionLerp = new StringLerp("", version.ToUpper()),
		accLerp = new StringLerp("0<size=15>%</size>", acc.Insert(acc.Length - 1, "<size=15>") + "</size>"),
		scoreLerp = new StringLerp("0", score.ToString()), perfectLerp = new StringLerp("0", perfect.ToString()), 
		greatLerp = new StringLerp("0", great.ToString()), goodLerp = new StringLerp("0", good.ToString()), 
		badLerp = new StringLerp("0", bad.ToString()), missLerp = new StringLerp("0", miss.ToString()),
		comboLerp = new StringLerp("0", combo.ToString());

		uiScoreNewText.gameObject.SetActive(isScoreNew);
		uiComboNewText.gameObject.SetActive(isComboNew);
		uiFullComboText.gameObject.SetActive(isFullCombo);
		uiRankText.text = "--";

		TweenManager.AddTween(tween.CreateTransition(step => {
			uiTitleText.text = titleLerp.Lerp(step);
			uiVersionText.text = versionLerp.Lerp(step);
			uiAccText.text = accLerp.Lerp(step);
			uiScoreText.text = scoreLerp.Lerp(step);
			uiPerfectText.text = perfectLerp.Lerp(step);
			uiGreatText.text = greatLerp.Lerp(step);
			uiGoodText.text = goodLerp.Lerp(step);
			uiBadText.text = badLerp.Lerp(step);
			uiMissText.text = missLerp.Lerp(step);
			uiComboText.text = comboLerp.Lerp(step);
		}));
	}

	public void SetRank(string rank) {
		var rankLerp = new StringLerp("0<size=10>" + rank.Substring(rank.Length - 2).ToUpper() + "</size>", rank.ToUpper().Insert(rank.Length - 2, "<size=10>") + "</size>");

		TweenManager.AddTween(tween.CreateTransition(step => {
			uiRankText.text = rankLerp.Lerp(step);
		}));
	}
}
