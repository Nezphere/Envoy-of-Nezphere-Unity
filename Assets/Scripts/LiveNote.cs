[System.Serializable]
public class LiveNote {
	public float x, y;
	public float startTime;
	public bool isPara, isSpecial;

	public LiveNote(UnityEngine.Vector2 pos, float startTime, bool isPara, bool isSpecial) {
		x = pos.x;
		y = pos.y;
		this.startTime = startTime;
		this.isPara = isPara;
		this.isSpecial = isSpecial;
	}

	public LiveNote(float x, float y, float startTime, bool isPara, bool isSpecial) {
		this.x = x;
		this.y = y;
		this.startTime = startTime;
		this.isPara = isPara;
		this.isSpecial = isSpecial;
	}
}

public delegate UnityEngine.Vector2 AxisTransformer2(float x, float y);