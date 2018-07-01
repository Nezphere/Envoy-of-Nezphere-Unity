using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Uif;

[System.Serializable]
public class RankInfo : System.IComparable<RankInfo> {
	public string rank, name;
	public int score;

	public int CompareTo(RankInfo other) {
		return other.score.CompareTo(score);
	}
}

public class RankPanelController : MonoBehaviour {
	public Text[] rankCol, nameCol, scoreCol;
	public Tween textChangeTween;

	StringLerp[] rankColLerps = new StringLerp[0], nameColLerps = new StringLerp[0], scoreColLerps = new StringLerp[0];

	void Start() {
		textChangeTween.isDead = true;
		textChangeTween.doStayAlive = true;
		TweenManager.AddTween(textChangeTween.SetTransition(step => {
			for (int i = 0; i < rankCol.Length && i < rankColLerps.Length; i++) {
				if (rankColLerps[i] != null)
					rankCol[i].text = rankColLerps[i].Lerp(step); 
			}
			for (int i = 0; i < nameCol.Length && i < nameColLerps.Length; i++) {
				if (nameColLerps[i] != null)
					nameCol[i].text = nameColLerps[i].Lerp(step); 
			}
			for (int i = 0; i < scoreCol.Length && i < scoreColLerps.Length; i++) {
				if (scoreColLerps[i] != null)
					scoreCol[i].text = scoreColLerps[i].Lerp(step); 
			}
		}), false);
	}

	#if UNITY_EDITOR
	public RankInfo[] ranks;

	[ContextMenu("Test")]
	public void TestRank() {
		SetRank(ranks);
	}
	#endif

	public void SetRank(RankInfo[] ranks) {
		System.Array.Resize(ref rankColLerps, ranks.Length);
		System.Array.Resize(ref nameColLerps, ranks.Length);
		System.Array.Resize(ref scoreColLerps, ranks.Length);

		for (int i = 0; i < ranks.Length; i++) {
			rankCol[i].gameObject.SetActive(true);
			nameCol[i].gameObject.SetActive(true);
			scoreCol[i].gameObject.SetActive(true);
			string rank = ranks[i].rank, player = ranks[i].name, score = ranks[i].score.ToString();
			rankColLerps[i] = rankCol[i].text != rank ? new StringLerp(rankCol[i].text, rank) : null;
			nameColLerps[i] = nameCol[i].text != player ? new StringLerp(nameCol[i].text, player) : null;
			scoreColLerps[i] = scoreCol[i].text != score ? new StringLerp(scoreCol[i].text, score) : null;
		}

		for (int i = ranks.Length; i < rankCol.Length; i++) {
			rankCol[i].gameObject.SetActive(false);
			nameCol[i].gameObject.SetActive(false);
			scoreCol[i].gameObject.SetActive(false);
			rankCol[i].text = "";
			nameCol[i].text = "";
			scoreCol[i].text = "";
		}

		textChangeTween.isDead = false;
		textChangeTween.time = 0;
	}
}
