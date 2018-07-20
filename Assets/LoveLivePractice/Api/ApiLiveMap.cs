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

		public LiveNote[] GetLiveNotes(AxisTransformer2 transformer, int level) {
			int counter = 0;
			var list = new System.Collections.Generic.List<LiveNote>();

			for (int i = 0; i < lane.Length; i++) {
				var note = lane[i];

				if (counter % level == 0) {
					list.Add(new LiveNote(
						transformer(note.lane, 0), 
						note.starttime / 1000f, 
						note.parallel, 
						note.longnote));

					if (note.parallel) {
						if (i > 0 && System.Math.Abs(lane[i - 1].starttime - note.starttime) < 1) {
							note = lane[i - 1];
							list.Add(new LiveNote(
								transformer(note.lane, 0), 
								note.starttime / 1000f, 
								note.parallel, 
								note.longnote));
						} else if (i < lane.Length - 1 && System.Math.Abs(lane[i + 1].starttime - note.starttime) < 1) {
							note = lane[i + 1];
							list.Add(new LiveNote(
								transformer(note.lane, 0), 
								note.starttime / 1000f, 
								note.parallel, 
								note.longnote));
						}
					}
				}

				counter += 1;
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