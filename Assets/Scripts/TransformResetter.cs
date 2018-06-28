using UnityEngine;

public class TransformResetter : MonoBehaviour {
	public Vector3 position, size;
	public Quaternion rotation;
	public bool trigger;

	void OnValidate() {
		position = transform.localPosition;
		rotation = transform.localRotation;
		size = transform.localScale;
	}

	public void ResetTransform() {
		transform.localPosition = position;
		transform.localRotation = rotation;
		transform.localScale = size;
	}
}
