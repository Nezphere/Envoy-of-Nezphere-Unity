using UnityEngine;

public class SwordTip : MonoBehaviour {
	const int CacheCount = 4, CacheMod = 3;

	public Transform baseTrans;

	public Side side;
	public float deltaTime, speed;
	public Vector3 position, lastPosition, velocity, acceleration;
	public Vector3 tipPosition, lastTipPosition, basePosition, lastBasePosition;

	readonly Vector3[] positions = new Vector3[CacheCount], basePositions = new Vector3[CacheCount];
	int index;

	float lastTime;

	public void ManualUpdate() {
		var current = Time.time;

		deltaTime = current - lastTime;
		lastTime = current;

		lastPosition = position;
		position = transform.position;

		velocity = (position - lastPosition) / deltaTime;
		speed = velocity.magnitude;

		tipPosition = positions[index & CacheMod] = position;
		lastTipPosition = positions[(index - CacheMod) & CacheMod];

		basePosition = basePositions[index & CacheMod] = baseTrans.position;
		lastBasePosition = basePositions[(index - CacheMod) & CacheMod];

		index += 1;

//		Debug.DrawRay(position, velocity * 0.1f, Color.green);
	}
}
