using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GlobalStatic {
	public static bool IsLoggedIn { get { return !string.IsNullOrEmpty(Session); } }

	public static string Session = "", Name = "--";
}
