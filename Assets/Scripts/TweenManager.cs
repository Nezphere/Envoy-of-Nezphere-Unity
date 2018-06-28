using System.Collections.Generic;
using UnityEngine;

using Uif;

[System.Serializable]
public class Tween {
	public float duration;
	public EasingType easingType;
	public EasingPhase easingPhase;

	public System.Action<float> applyTransition;
	public System.Action endTransition;

	[HideInInspector]
	public float time;
	[HideInInspector]
	public bool doStayAlive, isDead;

	public Tween(float duration, EasingType easingType, EasingPhase easingPhase, System.Action<float> applyTransition, System.Action endTransition = null) {
		this.duration = duration;
		this.easingType = easingType;
		this.easingPhase = easingPhase;
		this.applyTransition = applyTransition;
		this.endTransition = endTransition;
	}

	public Tween SetTransition(System.Action<float> applyTransition, System.Action endTransition = null) {
		this.applyTransition = applyTransition;
		this.endTransition = endTransition;

		return this;
	}

	public Tween CreateTransition(System.Action<float> applyTransition, System.Action endTransition = null) {
		return new Tween(duration, easingType, easingPhase, applyTransition, endTransition);
	}
}

public class TweenManager : MonoBehaviour {
	static readonly LinkedList<Tween> tweenList = new LinkedList<Tween>();

	public static WaitForSeconds AddTween(Tween tween, bool doStartInstantly = true) {
		if (doStartInstantly) {
			tween.applyTransition(0);
		}
		tweenList.AddLast(tween);
		return new WaitForSeconds(tween.duration);
	}

	void Update() {
		float deltaTime = Time.deltaTime;

		var node = tweenList.First;
		while (node != null) {
			var next = node.Next;

			var tween = node.Value;
			if (!tween.isDead) {
				tween.time += deltaTime;
				
				if (tween.time < tween.duration) {
					tween.applyTransition(Easing.Ease(tween.easingType, tween.easingPhase, tween.time, tween.duration));
				} else {
					if (tween.endTransition == null)
						tween.applyTransition(1);
					else
						tween.endTransition();
					
					if (!tween.doStayAlive)
						tweenList.Remove(node);
					else
						tween.isDead = true;
				}
			}

			node = next;
		}
	}
}
