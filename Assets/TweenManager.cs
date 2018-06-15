using System.Collections.Generic;
using UnityEngine;

using Uif;

[System.Serializable]
public class Tween {
	public float duration, time;
	public EasingType easingType;
	public EasingPhase easingPhase;

	public System.Action<float> applyTransition;
	public System.Action endTransition;


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
}

public class TweenManager : MonoBehaviour {
	static readonly LinkedList<Tween> tweenList = new LinkedList<Tween>();

	public static void AddTween(Tween tween, bool doStartInstantly = true) {
		if (doStartInstantly) {
			tween.applyTransition(0);
		}
		tweenList.AddLast(tween);
	}

	void Update() {
		float deltaTime = Time.deltaTime;

		var node = tweenList.First;
		while (node != null) {
			var next = node.Next;

			var tween = node.Value;
			tween.time += deltaTime;

			if (tween.time < tween.duration) {
				tween.applyTransition(Easing.Ease(tween.easingType, tween.easingPhase, tween.time, tween.duration));
			} else {
				if (tween.endTransition == null)
					tween.applyTransition(1);
				else
					tween.endTransition();

				tweenList.Remove(node);
			}

			node = next;
		}
	}
}
