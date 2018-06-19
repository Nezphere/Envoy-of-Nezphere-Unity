using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class TabNavigation : MonoBehaviour {
	public bool findFirstSelectable = false;

	void Update() {
		if (Input.GetKeyDown(KeyCode.Tab)) {
			if (EventSystem.current != null) {
				GameObject selected = EventSystem.current.currentSelectedGameObject;

				//try and find the first selectable if there isn't one currently selected
				//only do it if the findFirstSelectable is true
				//you may not always want this feature and thus
				//it is disabled by default
				if (selected == null && findFirstSelectable) {
					Selectable found = (Selectable.allSelectables.Count > 0) ? Selectable.allSelectables[0] : null;

					if (found != null) {
						//simple reference so that selected isn't null and will proceed
						//past the next if statement
						selected = found.gameObject;
					}
				}

				if (selected != null) {
					var current = (Selectable)selected.GetComponent("Selectable");

					if (current != null) {
						var nextDown = current.FindSelectableOnDown();
						var nextUp = current.FindSelectableOnUp();
						var nextRight = current.FindSelectableOnRight();
						var nextLeft = current.FindSelectableOnLeft();

						if (nextDown != null) {
							nextDown.Select();
						} else if (nextRight != null) {
							nextRight.Select();
						} else if (nextUp != null) {
							nextUp.Select();
						} else if (nextLeft != null) {
							nextLeft.Select();
						}
					}
				}
			}
		}
	}
}