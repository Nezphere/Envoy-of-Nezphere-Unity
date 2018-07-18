using UnityEngine;
using UnityEngine.EventSystems;

namespace VrInput {
	public class VrPointerEventData : PointerEventData {
		public GameObject Current;
		public VrPointer Controller;

		public VrPointerEventData(EventSystem e) : base(e) {
		}

		public override void Reset() {
			Current = null;
			Controller = null;
			base.Reset();
		}
	}
}
