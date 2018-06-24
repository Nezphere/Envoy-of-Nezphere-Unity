using System.Text.RegularExpressions;

namespace LoveLivePractice.Api {
	[System.Serializable]
	public class ApiLiveMap {
		public string audiofile;
		public int speed;
		public ApiMapNote[] lane;

		public static string Transform(string json) {
			string result = Regex.Replace(json, @"\[\[([^\[\]]*)\],\[([^\[\]]*)\],\[([^\[\]]*)\],\[([^\[\]]*)\],\[([^\[\]]*)\],\[([^\[\]]*)\],\[([^\[\]]*)\],\[([^\[\]]*)\],\[([^\[\]]*)\]\]", @"[$1,$2,$3,$4,$5,$6,$7,$8,$9]");
			// Fix empty lanes
			result = Regex.Replace(result, @",+", @",");
			result = Regex.Replace(result, @"\[,", @"[");
			result = Regex.Replace(result, @"\,]", @"]");
			return result;
		}

		public LiveNote[] GetLiveNotes(AxisTransformer2 transformer) {
			var list = new System.Collections.Generic.List<LiveNote>();
			foreach (var note in lane) {
				list.Add(new LiveNote(
					transformer(note.lane, 0), 
					note.starttime / 1000f, 
					note.parallel, 
					note.longnote));
			}
			return list.ToArray();
		}
	}

	[System.Serializable]
	public class ApiMapNote : System.IComparable<ApiMapNote> {
		public int lane;
		public float starttime, endtime;
		public bool longnote, parallel, hold;

		public int CompareTo(ApiMapNote other) {
			return starttime.CompareTo(other.starttime);
		}
	}
}