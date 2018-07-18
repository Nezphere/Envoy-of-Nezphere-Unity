using UnityEngine;
using UnityEngine.XR;

public sealed class VrPlayer : MonoBehaviour {
	public Transform headTrans, lHandTrans, rHandTrans;
	public float damping = 100, rotationDamping = 100;

	public static class WmrInput {
		public const string WMR_A_L_TRIGGER = "WMR_A_L_TRIGGER";
		public const string WMR_A_R_TRIGGER = "WMR_A_R_TRIGGER";
		public const string WMR_A_L_GRIP = "WMR_A_L_GRIP";
		public const string WMR_A_R_GRIP = "WMR_A_R_GRIP";
		public const string WMR_A_L_STICK_X = "WMR_A_L_STICK_X";
		public const string WMR_A_L_STICK_Y = "WMR_A_L_STICK_Y";
		public const string WMR_A_R_STICK_X = "WMR_A_R_STICK_X";
		public const string WMR_A_R_STICK_Y = "WMR_A_R_STICK_Y";
		public const string WMR_A_L_PAD_X = "WMR_A_L_PAD_X";
		public const string WMR_A_L_PAD_Y = "WMR_A_L_PAD_Y";
		public const string WMR_A_R_PAD_X = "WMR_A_R_PAD_X";
		public const string WMR_A_R_PAD_Y = "WMR_A_R_PAD_Y";

		public const string WMR_B_L_MENU = "WMR_B_L_MENU";
		public const string WMR_B_R_MENU = "WMR_B_R_MENU";
		public const string WMR_B_L_STICK_PRESS = "WMR_B_L_STICK_PRESS";
		public const string WMR_B_R_STICK_PRESS = "WMR_B_R_STICK_PRESS";
		public const string WMR_B_L_PAD_TOUCH = "WMR_B_L_PAD_TOUCH";
		public const string WMR_B_R_PAD_TOUCH = "WMR_B_R_PAD_TOUCH";
		public const string WMR_B_L_PAD_PRESS = "WMR_B_L_PAD_PRESS";
		public const string WMR_B_R_PAD_PRESS = "WMR_B_R_PAD_PRESS";
	}

	public enum WmrInputEnum {
		WMR_A_L_TRIGGER,
		WMR_A_R_TRIGGER,
		WMR_A_L_GRIP,
		WMR_A_R_GRIP,
		WMR_A_L_STICK_X,
		WMR_A_L_STICK_Y,
		WMR_A_R_STICK_X,
		WMR_A_R_STICK_Y,
		WMR_A_L_PAD_X,
		WMR_A_L_PAD_Y,
		WMR_A_R_PAD_X,
		WMR_A_R_PAD_Y,

		WMR_B_L_MENU,
		WMR_B_R_MENU,
		WMR_B_L_STICK_PRESS,
		WMR_B_R_STICK_PRESS,
		WMR_B_L_PAD_TOUCH,
		WMR_B_R_PAD_TOUCH,
		WMR_B_L_PAD_PRESS,
		WMR_B_R_PAD_PRESS,
	}

	void Start() {
		var position = Vector3.zero;
		position.x = PlayerPrefs.GetFloat("player_position_x");
		position.y = PlayerPrefs.GetFloat("player_position_y");
		position.z = PlayerPrefs.GetFloat("player_position_z");
		transform.position = position;
	}

	public void ManualUpdate() {
		var lHandPosition = InputTracking.GetLocalPosition(XRNode.LeftHand);
		var lHandRotation = InputTracking.GetLocalRotation(XRNode.LeftHand);
		lHandTrans.localPosition = Vector3.Lerp(lHandTrans.localPosition, lHandPosition, Time.deltaTime * damping);
		lHandTrans.localRotation = Quaternion.Slerp(lHandTrans.localRotation, lHandRotation, Time.deltaTime * rotationDamping);

		var rHandPosition = InputTracking.GetLocalPosition(XRNode.RightHand);
		var rHandRotation = InputTracking.GetLocalRotation(XRNode.RightHand);
		rHandTrans.localPosition = Vector3.Lerp(rHandTrans.localPosition, rHandPosition, Time.deltaTime * damping);
		rHandTrans.localRotation = Quaternion.Slerp(rHandTrans.localRotation, rHandRotation, Time.deltaTime * rotationDamping);

		var translation = Vector3.zero;
		translation.x = Input.GetAxis(WmrInput.WMR_A_L_STICK_X) + Input.GetAxis(WmrInput.WMR_A_R_STICK_X); 
		translation.y = Input.GetAxis(WmrInput.WMR_A_L_STICK_Y); 
		translation.z = Input.GetAxis(WmrInput.WMR_A_R_STICK_Y);

		if (translation.sqrMagnitude != 0) {
			transform.Translate(translation * 0.001f);
			PlayerPrefs.SetFloat("player_position_x", transform.position.x);
			PlayerPrefs.SetFloat("player_position_y", transform.position.y);
			PlayerPrefs.SetFloat("player_position_z", transform.position.z);
		}
	}

	//	void OnDrawGizmos() {
	//		var headPosition = InputTracking.GetLocalPosition(XRNode.CenterEye);
	//		var headRotation = InputTracking.GetLocalRotation(XRNode.CenterEye);
	//		DrawAnchor(headPosition, headRotation);
	//
	//		var lHandPosition = InputTracking.GetLocalPosition(XRNode.LeftHand);
	//		var lHandRotation = InputTracking.GetLocalRotation(XRNode.LeftHand);
	//		DrawAnchor(lHandPosition, lHandRotation);
	//
	//		var rHandPosition = InputTracking.GetLocalPosition(XRNode.RightHand);
	//		var rHandRotation = InputTracking.GetLocalRotation(XRNode.RightHand);
	//		DrawAnchor(rHandPosition, rHandRotation);
	//	}

	static void DrawAnchor(Vector3 position, Quaternion rotation, float length = 0.5f) {
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(position, 0.1f);

		Gizmos.color = Color.red;
		Gizmos.DrawRay(position, rotation * Vector3.forward * length);
	}
}