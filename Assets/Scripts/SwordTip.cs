using UnityEngine;

public class SwordTip : MonoBehaviour {
	public enum Side {
		Left,
		Right,
	}

	public Side side;
	public float deltaTime;
	public Vector3 position, velocity, acceleration;
	Vector3 lastPosition, lastVelocity;

	const float interval = 0.05f;
	float lastTime;

	void LateUpdate() {
		var current = Time.time;
		//		if (current - lastTime < interval) return;
		
		deltaTime = current - lastTime;
		lastTime = current;
//		deltaTime = Time.deltaTime;

		position = transform.position;
		velocity = (position - lastPosition) / deltaTime;
		acceleration = (velocity - lastVelocity) / deltaTime;

		lastPosition = position;
		lastVelocity = velocity;

		Debug.DrawRay(position, velocity * 0.1f, Color.green);
		//Debug.DrawRay(position, acceleration, Color.red);
	}
}
