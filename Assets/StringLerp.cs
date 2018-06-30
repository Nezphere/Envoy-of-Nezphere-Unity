using UnityEngine;

using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Uif {
	public sealed class StringLerp {
		[System.Serializable]
		public class Edit {
			public int operation;
			public int index;
			public string str;

			public Edit(int operation, int index, string str) {
				this.operation = operation;
				this.index = index;
				this.str = str;
			}
		}

		public const int MaxMatrixSize = 256 * 256;
		public static readonly Regex MathNumberRegex = new Regex(@"(-?\d{1,20}(?:\.\d{1,20})?)", RegexOptions.Multiline);
		public bool useNumeric, useDifference;

		public string[] sourceParts, targetParts, outputParts;
		public Edit[] edits;

		public readonly string Source, Target;

		public StringLerp(string source, string target) {
			Source = source ?? "";
			Target = target ?? "";

			useNumeric = (MathNumberRegex.Replace(Source, "0") == MathNumberRegex.Replace(Target, "0"));
			if (useNumeric) {
				sourceParts = MathNumberRegex.Split(Source);
				targetParts = MathNumberRegex.Split(Target);
				outputParts = (string[])targetParts.Clone();

				return;
			}

			var n = Source.Length * Target.Length;
			useDifference = (0 < n && n < MaxMatrixSize);
			if (useDifference) {
				edits = Difference(Target, Source, 2, 2, 3);

				return;
			}
		}

		public string Lerp(float t) {
			t = Mathf.Clamp01(t);

			if (useNumeric)
				return NumericLerp(t);
			else if (useDifference)
				return DifferenceLerp(t);
			else {
				var sourceLength = Mathf.RoundToInt(Source.Length * t);
				var targetLength = Mathf.RoundToInt(Target.Length * t);
				var head = Target.Substring(0, targetLength);
				var tail = Source.Substring(sourceLength, Source.Length);
				return head + tail;
			}
		}

		string NumericLerp(float t) {
			var i = 1;

			while (i < sourceParts.Length) {
				var sourcePart = sourceParts[i];
				var targetPart = targetParts[i];
				var part = Mathf.Lerp(float.Parse(sourcePart), float.Parse(targetPart), t);
				var sourcePoint = sourcePart.IndexOf(".");
				var targetPoint = targetPart.IndexOf(".");
				var point = Mathf.RoundToInt(
					            Mathf.Lerp(
						            sourcePoint >= 0 ? (sourcePart.Length - 1) - sourcePoint : 0,
						            targetPoint >= 0 ? (targetPart.Length - 1) - targetPoint : 0,
						            t));

				outputParts[i] = part.ToString("F" + point);
				i += 2;
			}

			return string.Join("", outputParts);
		}

		string DifferenceLerp(float t) {
			return PatchString(Mathf.RoundToInt((1 - t) * edits.Length), Target);
		}

		string PatchString(int step, string str) {
			for (var i = 0; i < step; ++i) {
				var edit = edits[i];

				var head = str.Substring(0, edit.index);
				var tail = str.Substring(edit.operation + edit.index);
				str = head + edit.str + tail;
			}
			return str;
		}

		#region Calculations

		static Edit[] Difference(string source, string target, int ins, int del, int sub) {
			return EditPath(CostMatrix(source, target, ins, del, sub), target);
		}

		const int INS = 0, SUB = 1;

		static Edit[] EditPath(int[] costs, string target) {
			/** Given a cost matrix and a target, create an edit list */
			var path = new List<Edit>();
			var j = target.Length;
			var n = j + 1;
			var i = costs.Length / n - 1;
			while (i > 0 || j > 0) {
				var sub = (i > 0 && j > 0) ? costs[n * (i - 1) + j - 1] : float.PositiveInfinity;
				var del = i > 0 ? costs[n * (i - 1) + j] : float.PositiveInfinity;
				var ins = j > 0 ? costs[n * i + j - 1] : float.PositiveInfinity;
				if (sub <= ins && sub <= del) {
					if (costs[n * i + j] != costs[n * (i - 1) + j - 1])
						path.Add(new Edit(SUB, i - 1, target[j - 1].ToString()));
					--i;
					--j;
				} else if (ins <= del) {
					path.Add(new Edit(INS, i, target[j - 1].ToString()));
					--j;
				} else {
					path.Add(new Edit(SUB, i - 1, string.Empty));
					--i;
				}
			}
			return path.ToArray();
		}

		static int[] CostMatrix(string source, string target, int ins = -1, int del = -1, int sub = -1) {
			ins = ins != -1 ? ins : 1;
			del = del != -1 ? del : ins;
			sub = sub != -1 ? sub : Mathf.Max(ins, del);

			var m = source.Length + 1;
			var n = target.Length + 1;
			var d = new int[m * n];
			int i, j;
			for (i = 1; i < m; ++i)
				d[n * i] = i;
			for (j = 1; j < n; ++j)
				d[j] = j;
			for (j = 1; j < n; ++j)
				for (i = 1; i < m; ++i)
					if (source[i - 1] == target[j - 1])
						d[n * i + j] = d[n * (i - 1) + j - 1];
					else
						d[n * i + j] = Mathf.Min(
							del + d[n * (i - 1) + j],
							Mathf.Min(
								ins + d[n * i + j - 1],
								sub + d[n * (i - 1) + j - 1]));

			return d;
		}

		#endregion
	}
}