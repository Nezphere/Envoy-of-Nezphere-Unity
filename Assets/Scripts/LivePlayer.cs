using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using Uif;
using LoveLivePractice.Api;

using Math = System.Math;
using Random = System.Random;

public class LivePlayer : MonoBehaviour {
	public static LivePlayer Instance;

	sealed class FResource<T> : System.IDisposable where T : Object {
		T _asset;

		public T asset {
			get {
				if (_asset != null)
					return _asset;
				throw new System.NullReferenceException();
			}
		}

		public FResource(string path) {
			_asset = Resources.Load<T>(path);
			if (_asset == null)
				throw new System.NullReferenceException(path);
		}

		public void Dispose() {
			Resources.UnloadAsset(_asset);
			_asset = null;
		}
	}


	public AudioSource source;

	[Header("Game")]
	public float gameStartDelay = 4;
	public string liveName;
	public double bufferInterval = 5, cacheInterval = 1;
	public int level = 1;

	ApiLive live;
	ApiLiveMap map;
	AudioClip bgm;

	double startTime;
	int index;

	Random rand;

	void OnValidate() {
		source = GetComponent<AudioSource>();
	}

	void Start() {
		Instance = this;

		using (var liveAsset = new FResource<TextAsset>("lives/" + liveName)) {
			live = JsonUtility.FromJson<ApiLiveResponse>(liveAsset.asset.text).content;
		}

		using (var mapAsset = new FResource<TextAsset>("maps/" + live.map_path.Replace(".json", ""))) {
			map = JsonUtility.FromJson<ApiLiveMap>(mapAsset.asset.text);
			System.Array.Sort(map.lane);
		}

		bgm = Resources.Load<AudioClip>("bgms/" + live.bgm_path.Replace(".mp3", ""));
		source.clip = bgm;
		rand = new Random(seed);

		hasStarted = true;

		StartGame();
	}

	bool hasStarted, isInPrelude;
	int counter = 0;

	void StartGame() {
		index = 0;
		startTime = AudioSettings.dspTime + gameStartDelay;
		source.PlayScheduled(startTime);
		isInPrelude = true;
		index = 0;
	}

	public static double time;

	void Update() {
		if (hasStarted && !isInPrelude && !source.isPlaying) {
			StartGame();
		}

		if (isInPrelude) {
			time = AudioSettings.dspTime - startTime;
			isInPrelude = time < 0;
		} else {
			time = source.time;
		}

		double bufferTime = time + bufferInterval;

		while (index < map.lane.Length && map.lane[index].starttime / 1000.0 <= bufferTime) {
			if (map.lane[index].parallel) {
//				InitBlock(map.lane[index], map.lane[index + 1]);
				if (counter % level == 0) {
					InitBlock(map.lane[index], map.lane[index + 1]);
				}
				index += 2;
			} else {
				if (counter % level == 0)
					InitBlock(map.lane[index]);
				index += 1;
			}

			counter += 1;
		}
			
		for (int i = 0; i < liveBlocks.Count; i++) {
			UpdateBlock(liveBlocks[i], time);
			if (liveBlocks[i].shouldDie) {
				Destroy(liveBlocks[i].gameObject);
				liveBlocks.RemoveAt(i);
				i -= 1;
			}
		}

//		if (Input.GetAxis(VrPlayer.WmrInput.WMR_A_L_TRIGGER) > 0.5f) {
//			level -= 1;
//		}
//		if (Input.GetAxis(VrPlayer.WmrInput.WMR_A_R_TRIGGER) > 0.5f) {
//			level += 1;
//		}

		if (Input.GetAxis(VrPlayer.WmrInput.WMR_A_L_GRIP) > 0.5f) {
//			source.pitch -= 0.001f;
		}

		if (Input.GetAxis(VrPlayer.WmrInput.WMR_A_R_GRIP) > 0.5f) {
//			source.pitch += 0.001f;
//			source.pitch = 1;
		}
	}

	[Header("Blocks")]
	public GameObject leftBlock;
	public GameObject rightBlock;
	public float bufferX = 100, bufferY = 100, bufferZ = 100;
	public EasingType bufferEasingTypeX, bufferEasingTypeY, bufferEasingTypeZ;
	public EasingPhase bufferEasingPhaseX, bufferEasingPhaseY, bufferEasingPhaseZ;
	public float cacheX = 100, cacheY = 100, cacheZ = 100;
	public EasingType cacheEasingTypeX, cacheEasingTypeY, cacheEasingTypeZ = EasingType.Cubic;
	public EasingPhase cacheEasingPhaseX, cacheEasingPhaseY, cacheEasingPhaseZ;

	[Header("Random")]
	public int seed;
	public Side leftSide = Side.Left, rightSide = Side.Right;
	public Heading leftHeading = Heading.Down, rightHeading = Heading.Down;
	Vector2[] slots = {
		new Vector2(-3f, 1f),
		new Vector2(-2f, 1f),
		new Vector2(-2f, 0f),
		new Vector2(-1f, 0f),

		new Vector2(0f, 0f),

		new Vector2(1f, 0f),
		new Vector2(2f, 0f),
		new Vector2(2f, 1f),
		new Vector2(3f, 1f),
	};

	readonly List<LiveCube> liveBlocks = new List<LiveCube>();

	static Heading CalcEndHeading(Heading heading) {
		return (Heading)(((int)heading + 4) % 8);
	}

	public double NextGaussian() {
		const double mean = 0 , stdDev= 1;

		double u1 = 1.0 - rand.NextDouble();
		double u2 = 1.0 - rand.NextDouble();
		double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
		return mean + stdDev * randStdNormal;
	}

	Heading CalcDerivedHeading(Heading heading) {
//		return heading;
		int offset = (int)NextGaussian();
		return (Heading)(((int)heading + 8 + offset) % 8);
	}

	static float HeadingToRotation(Heading heading) {
		return 45 * ((int)heading - 4);
	}

	void CreateBlock(Side hand, Heading heading, int lane, double time, ApiMapNote note) {
		var go = Instantiate(hand == Side.Left ? leftBlock : rightBlock, transform);
		var block = go.GetComponent<LiveCube>();
		block.time = time;
		block.shouldDie = false;
//		block.note = note;
		block.heading = heading;

		var slot = slots[lane];
		block.x = slot.x * 0.4f;
		block.y = slot.y * 0.4f;
		block.transform.Rotate(HeadingToRotation(heading), 0, 0);
		liveBlocks.Add(block);
	}

	static bool IsLeft(Heading side) {
		return side == Heading.DownLeft || side == Heading.Left || side == Heading.UpLeft;
	}

	static bool IsRight(Heading side) {
		return side == Heading.DownRight || side == Heading.Right || side == Heading.UpRight;
	}

	void UseLeft(Heading heading, int lane, double time, ApiMapNote note) {
		heading = CalcDerivedHeading(heading);
		CreateBlock(Side.Left, heading, lane, time, note);
		leftHeading = CalcEndHeading(heading);
	}

	void UseRight(Heading heading, int lane, double time, ApiMapNote note) {
		heading = CalcDerivedHeading(heading);
		CreateBlock(Side.Right, heading, lane, time, note);
		rightHeading = CalcEndHeading(heading);
	}

	void InitBlock(ApiMapNote note1, ApiMapNote note2) {
		if (note1.lane > note2.lane) {
			var temp = note2;
			note2 = note1;
			note1 = temp;
		}

		var note1Lane = note1.lane;
		var note1Time = note1.starttime / 1000.0;

		var note2Lane = note2.lane;
		var note2Time = note2.starttime / 1000.0;

		if (note2Lane - note1Lane >= 6) {
			if (4 - note1Lane > note2Lane - 4) {
//				note2Lane -= 1;
			} else if (4 - note1Lane < note2Lane - 4) {
//				note1Lane += 1;
			} else {
//				if (rand.NextDouble() < 0.5) note2Lane -= 1;
//				else note1Lane += 1;
			}
		}

		if (leftSide == Side.Right) {
			UseLeft(Heading.Right, note1Lane, note1Time, note1);
			leftSide = Side.Left;
		} else
			UseLeft(leftHeading, note1Lane, note1Time, note1);

		if (rightSide == Side.Left) {
			UseRight(Heading.Left, note2Lane, note2Time, note2);
			rightSide = Side.Right;
		} else
			UseRight(rightHeading, note2Lane, note2Time, note2);
	}

	void InitBlock(ApiMapNote note) {
		var lane = note.lane;
		var time = note.starttime / 1000.0;

		// 1 3     6 8
		//     [4]
		// 0 2     5 7
		if (note.lane < 4) {  // left side
			if (rightSide == Side.Left)
				UseRight(rightHeading, lane, time, note);
			else if (leftSide == Side.Left)
				UseLeft(leftHeading, lane, time, note);
			else {  // no hand on left, use left hand
				UseLeft(Heading.Right, lane, time, note);
				leftSide = Side.Left;
			}
		} else if (note.lane > 4) {  // right side
			if (leftSide == Side.Right)
				UseLeft(leftHeading, lane, time, note);
			else if (rightSide == Side.Right)
				UseRight(rightHeading, lane, time, note);
			else {  // no hand on right, use right hand
				UseRight(Heading.Left, lane, time, note);
				rightSide = Side.Right;
			}
		} else {  // middle
			if (leftSide == Side.Right) {  // left hand on right
				UseLeft(Heading.Right, lane, time, note);
				leftSide = Side.Left;
			} else if (rightSide == Side.Left) {  // right hand on left
				UseRight(Heading.Left, lane, time, note);
				rightSide = Side.Right;
			} else {  // no hand is inversed, use right hand
				if (rand.NextDouble() < 0.5) {
					UseLeft(Heading.Left, lane, time, note);
					leftSide = Side.Right;
				} else {
					UseRight(Heading.Right, lane, time, note);
					rightSide = Side.Left;
				}
			}
		}
	}

	const float damping = 100;

	void UpdateBlock(LiveCube block, double time) {
		if (block.gameObject.activeSelf) {
			if (time <= block.time) {  // Not yet
				var x = Easing.Ease(bufferEasingTypeX, bufferEasingPhaseX, block.x, bufferX * block.x, block.time - time, bufferInterval);
				var y = Easing.Ease(bufferEasingTypeY, bufferEasingPhaseY, block.y, bufferY, block.time - time, bufferInterval);
				var z = Easing.Ease(bufferEasingTypeZ, bufferEasingPhaseZ, 0, bufferZ, block.time - time, bufferInterval);
				block.transform.localPosition = Vector3.Lerp(block.transform.localPosition, new Vector3(x, y, z), Time.deltaTime * damping);
			} else if (time - block.time < cacheInterval) {  // Over due
				var x = Easing.Ease(cacheEasingTypeX, cacheEasingPhaseX, block.x, cacheX * block.x, time - block.time, cacheInterval);
				var y = Easing.Ease(cacheEasingTypeY, cacheEasingPhaseY, block.y, cacheY, time - block.time, cacheInterval);
				var z = Easing.Ease(cacheEasingTypeZ, cacheEasingPhaseZ, 0, cacheZ, time - block.time, cacheInterval);
				block.transform.localPosition = Vector3.Lerp(block.transform.localPosition, new Vector3(x, y, z), Time.deltaTime * damping);
			} else {
				block.shouldDie = true;
//				RecordMiss();
			}
		} else {
			block.shouldDie = true;
		}
	}

	//	[Header("Score")]
	//	public Text uiScoreText, uiComboText;
	//	public ScoreEffect seScoreText, seComboText;
	//
	//	public GameObject blueScore, redScore;
	//	public int combo, maxCombo, total;
	//	public Transform scoreCanvas;
	//
	//	const float baseNormalScore = 100, baseParaScore = 125;
	//
	//	const float timePerfect = 0.08f, timeGreat = 0.24f, timeGood = 0.32f, timeBad = 0.8f;
	//
	//	float OffsetAddon(double offset) {
	//		if (offset <= timePerfect)
	//			return 1.0f;
	//		if (offset <= timeGreat)
	//			return 0.88f;
	//		if (offset <= timeGood)
	//			return 0.8f;
	//		if (offset <= timeBad)
	//			return 0.4f;
	//		return 0;
	//	}
	//
	//	float ComboAddon() {
	//		if (combo <= 50)
	//			return 1.0f;
	//		if (combo <= 100)
	//			return 1.1f;
	//		if (combo <= 200)
	//			return 1.15f;
	//		if (combo <= 400)
	//			return 1.2f;
	//		if (combo <= 600)
	//			return 1.25f;
	//		if (combo <= 800)
	//			return 1.3f;
	//		return 1.35f;
	//	}
	//
	//	void RefreshCombo() {
	//		uiComboText.text = combo.ToString("N0");
	//		seScoreText.Again();
	//		seComboText.Again();
	//	}
	//
	//	public void RecordHit(int lane, bool para, bool left, double offset) {
	//		if (offset > timeGood)
	//			RecordMiss();
	//		if (offset > timeBad)
	//			return;
	//
	//		combo += 1;
	//		RefreshCombo();
	//
	//		float score = para ? baseParaScore : baseNormalScore;
	//		score *= OffsetAddon(offset) * ComboAddon();
	//		int iScore = (int)score;
	//
	//		total += iScore;
	//		uiScoreText.text = total.ToString("N0");
	//
	//		var go = Instantiate(left ? redScore : blueScore, scoreCanvas);
	//		go.transform.localPosition = new Vector3(50 * (lane - 4), 0, -20);
	//		go.GetComponent<Text>().text = "+" + iScore.ToString("N0");
	//	}
	//
	//	public void RecordMiss() {
	//		if (combo > maxCombo)
	//			maxCombo = combo;
	//
	//		combo = 0;
	//		RefreshCombo();
	//	}
}
