using UnityEngine;

namespace VrInput {
	public class LaserVrPointer : VrPointer {
		#region Handle

		public override bool ButtonDown() {
			return Input.GetButtonDown(VrPlayer.WmrInput.WMR_B_R_PAD_PRESS);
		}

		public override bool ButtonUp() {
			return Input.GetButtonUp(VrPlayer.WmrInput.WMR_B_R_PAD_PRESS);
		}

		#endregion

		public float laserThickness = 0.002f;
		public float laserHitScale = 0.02f;
		public Color color;

		private GameObject hitPoint;
		private GameObject pointer;

		public void SetActive(bool active) {
			hitPoint.SetActive(active);
			pointer.SetActive(active);
			enabled = active;
		}

		protected override void Start() {
			base.Start();

			pointer = GameObject.CreatePrimitive(PrimitiveType.Cube);
			pointer.transform.SetParent(transform, false);
			pointer.transform.localScale = new Vector3(laserThickness, laserThickness, 100.0f);
			pointer.transform.localPosition = new Vector3(0.0f, 0.0f, 50.0f);

			hitPoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			hitPoint.transform.SetParent(transform, false);
			hitPoint.transform.localScale = new Vector3(laserHitScale, laserHitScale, laserHitScale);
			hitPoint.transform.localPosition = new Vector3(0.0f, 0.0f, 100.0f);

			hitPoint.SetActive(false);

			// remove the colliders on our primitives
			DestroyImmediate(hitPoint.GetComponent<SphereCollider>());
			DestroyImmediate(pointer.GetComponent<BoxCollider>());

			var newMaterial = new Material(Shader.Find("VRInput/LaserPointer"));

			newMaterial.SetColor("_Color", color);
			pointer.GetComponent<MeshRenderer>().material = newMaterial;
			hitPoint.GetComponent<MeshRenderer>().material = newMaterial;
		}

		protected override void Draw(bool isHit, float distance) {
			var ray = LastRay;

			if (ray.direction == Vector3.zero) {
				pointer.SetActive(false);
			} else {
				pointer.SetActive(true);
				pointer.transform.localScale = new Vector3(laserThickness, laserThickness, distance);
				pointer.transform.position = ray.origin;
				pointer.transform.transform.rotation = Quaternion.LookRotation(ray.direction);
				pointer.transform.Translate(new Vector3(0.0f, 0.0f, distance * 0.5f));
			}

			if (ray.direction == Vector3.zero) {
				hitPoint.SetActive(false);
			} else {
				if (isHit) {
					hitPoint.SetActive(true);
					hitPoint.transform.position = ray.origin;
					hitPoint.transform.transform.rotation = Quaternion.LookRotation(ray.direction);
					hitPoint.transform.Translate(new Vector3(0.0f, 0.0f, distance));
				} else {
					hitPoint.SetActive(false);
				}
			}
		}
	}
}
