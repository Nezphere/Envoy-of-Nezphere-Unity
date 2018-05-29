using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class SwordTrail : MonoBehaviour {
	struct Point {
		public Vector3 basePos, tipPos;
		public float startTime;
		public int index;

		public Point(Vector3 basePos, Vector3 tipPos, float startTime, int index) {
			this.basePos = basePos;
			this.tipPos = tipPos;
			this.startTime = startTime;
			this.index = index;
		}
	}

	public bool doEmit = true, isActive = true, isDoubleFace;
	public Material trailMaterial;
	public Gradient trailColor;
	public float lifeTime = 1, minDistance = 0.01f;
	public int trailIndex;

	public Transform baseTrans, tipTrans;

	GameObject trailGo;
	Mesh trailMesh;

	readonly List<Point> pointList = new List<Point>();

	void Start() {
		trailGo = new GameObject(name + "Trail", typeof(MeshFilter), typeof(MeshRenderer));
		trailGo.transform.position = Vector3.zero;
		trailGo.transform.rotation = Quaternion.identity;
		trailGo.transform.localScale = Vector3.one;
		trailGo.GetComponent<Renderer>().sharedMaterial = trailMaterial;

		trailMesh = new Mesh();
		trailGo.GetComponent<MeshFilter>().mesh = trailMesh;
	}

	void OnDisable() {
		Destroy(trailGo);
	}

	void Update() {
		if (!isActive)
			return;

		float time = Time.time;

		if (doEmit) {
			var point = new Point(baseTrans.position, tipTrans.position, time, trailIndex);

			if (pointList.Count < 2) {
				pointList.Add(point);
			} else {
				var lastPoint = pointList[pointList.Count - 2];
				if (Vector3.Distance(point.basePos, lastPoint.basePos) > minDistance || Vector3.Distance(point.tipPos, lastPoint.tipPos) > minDistance) {
					pointList.Add(point);
				} else {
					point.startTime = pointList[pointList.Count - 1].startTime;
					pointList[pointList.Count - 1] = point;
				}
			}
		}

		if (pointList.Count > 1) {
			int pointCount = pointList.Count;

			var vertices = new Vector3[pointCount << 1];
			var uv = new Vector2[vertices.Length];
			var colors = new Color[vertices.Length];
			var triangles = new int[(pointCount - 1) * 6];

			for (int i = 0; i < pointCount; i++) {
				var point = pointList[i];

				vertices[i << 1] = point.basePos;
				vertices[i << 1 | 1] = point.tipPos;

				float step = Mathf.Clamp01((time - point.startTime) / lifeTime);

				var color = trailColor.Evaluate(step);
				colors[i << 1] = color;
				colors[i << 1 | 1] = color;
			
				uv[i << 1] = new Vector2(step, 0);
				uv[i << 1 | 1] = new Vector2(step, 1);

				if (i > 0 && pointList[i - 1].index == point.index) {
					triangles[(i - 1) * 6 + 0] = (i * 2) - 2;
					triangles[(i - 1) * 6 + 1] = (i * 2) - 1;
					triangles[(i - 1) * 6 + 2] = (i * 2);

					triangles[(i - 1) * 6 + 3] = (i * 2) + 1;
					triangles[(i - 1) * 6 + 4] = (i * 2);
					triangles[(i - 1) * 6 + 5] = (i * 2) - 1;
				}
			}

			trailMesh.Clear();
			trailMesh.vertices = vertices;
			trailMesh.colors = colors;
			trailMesh.uv = uv;
			trailMesh.triangles = triangles;
		} else {
			trailMesh.Clear();
		}

		while (pointList.Count > 0 && time - pointList[0].startTime > lifeTime) {
			pointList.RemoveAt(0);
		}
	}
}
