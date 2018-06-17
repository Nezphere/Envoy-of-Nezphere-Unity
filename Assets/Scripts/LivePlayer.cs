using System.Collections.Generic;

using UnityEngine;

using Uif;
using LoveLivePractice.Api;

using Math = System.Math;

public class LivePlayer : MonoBehaviour {
	#region Statics

	public static LivePlayer Instance;
	public static double Time, LastTime, AccTime, DeltaTime;
	public static float Speed;

	static readonly Vector2[] startSlots = {
		new Vector2(-4f, 4f),
		new Vector2(-3f, 3f),
		new Vector2(-2f, 2f),
		new Vector2(-1f, 1f),

		new Vector2(0f, 0f),

		new Vector2(1f, 1f),
		new Vector2(2f, 2f),
		new Vector2(3f, 3f),
		new Vector2(4f, 4f),
	};
	static readonly Vector2[] slots = {
		new Vector2(-2f, 2f),
		new Vector2(-2f, 1f),
		new Vector2(-1f, 1f),
		new Vector2(-1f, 0f),

		new Vector2(0f, 0f),

		new Vector2(1f, 0f),
		new Vector2(1f, 1f),
		new Vector2(2f, 1f),
		new Vector2(2f, 2f),
	};

	public static Heading GetEndHeading(Heading heading) {
		return (Heading)(((int)heading + 4) % 8);
	}

	public static float HeadingToRotation(Heading heading) {
		return 45 * ((int)heading - 4);
	}

	public static Vector3 HeadingToVector(Heading Heading) {
		float rotation = (-HeadingToRotation(Heading) - 90) * Mathf.Deg2Rad;
		return new Vector3(Mathf.Cos(rotation), Mathf.Sin(rotation));
	}


	public const float BaseNormalScore = 100, BaseParaScore = 125;
	public const float TimePerfect = 0.08f, TimeGreat = 0.24f, TimeGood = 0.32f, TimeBad = 0.8f;

	public static float GetScoreOffsetAddon(double offset) {
		if (offset <= TimePerfect)
			return 1.0f;
		if (offset <= TimeGreat)
			return 0.88f;
		if (offset <= TimeGood)
			return 0.8f;
		if (offset <= TimeBad)
			return 0.4f;
		return 0;
	}

	public static float GetScoreComboAddon(int combo) {
		if (combo <= 50)
			return 1.0f;
		if (combo <= 100)
			return 1.1f;
		if (combo <= 200)
			return 1.15f;
		if (combo <= 400)
			return 1.2f;
		if (combo <= 600)
			return 1.25f;
		if (combo <= 800)
			return 1.3f;
		return 1.35f;
	}

	#endregion

	public VrPlayer player;
	public AudioSource source;
	public SwordTip leftTip, rightTip;

	[Header("Game Config")]
	public string liveName;
	public int seed, level = 1;
	public float startDelay = 4, timeOffset = 0.02f;
	public float bufferInterval = 5, cacheInterval = 1, recycleInterval = 3, recycleGraceInterval = 1;
	public bool shouldAutoPlay;

	[Header("Block")]
	public GameObject blockPrototype;
	public float bufferX = 100, bufferY = 100, bufferZ = 100;
	public EasingType bufferEasingTypeX, bufferEasingTypeY, bufferEasingTypeZ;
	public EasingPhase bufferEasingPhaseX, bufferEasingPhaseY, bufferEasingPhaseZ;
	public float cacheX = 100, cacheY = 100, cacheZ = 100;
	public EasingType cacheEasingTypeX, cacheEasingTypeY, cacheEasingTypeZ = EasingType.Cubic;
	public EasingPhase cacheEasingPhaseX, cacheEasingPhaseY, cacheEasingPhaseZ;
	public Vector3 blockPoint1, blockPoint2;

	[Header("Block Close")]
	public float closeDuration;
	public float shellStart;
	public EasingType closeEasingType;
	public EasingPhase closeEasingPhase;

	[Header("Block Fx")]
	public ParticleFxPreset closeParticleFxPreset;
	public FlashFxConfig closeFlashFxConfig;
	public ParticleFxPreset hitParticleFxPreset;
	public FlashFxConfig hitFlashFxConfig;
	public AudioFxConfig[] specialAudioConfigs, perfectAudioConfigs, greatAudioConfigs, goodAudioConfigs, badAudioConfigs;
	public string perfectText, greatText, goodText, badText;
	public Tween textTween;
	public float textEndZ;

	[Header("Game State")]
	public int score;
	public int combo, maxCombo;
	public Side leftSide = Side.Left, rightSide = Side.Right;
	public Heading leftHeading = Heading.Down, rightHeading = Heading.Down;

	ApiLive live;
	ApiLiveMap map;
	AudioClip bgm;

	System.Random rand;
	double startTime;
	int index, counter;
	bool hasStarted, isInPrelude;

	readonly LinkedList<LiveBlock> liveBlockList = new LinkedList<LiveBlock>(), deadBlockList = new LinkedList<LiveBlock>(), inactiveBlockList = new LinkedList<LiveBlock>();


	void Start() {
		Instance = this;

		using (var liveAsset = new FResource<TextAsset>("lives/" + liveName)) {
			live = JsonUtility.FromJson<ApiLiveResponse>(liveAsset.asset.text).content;
		}

		using (var mapAsset = new FResource<TextAsset>("maps/" + live.map_path.Replace(".json", ""))) {
			map = JsonUtility.FromJson<ApiLiveMap>(mapAsset.asset.text);

			System.Array.Sort(map.lane);
			foreach (var note in map.lane) {
				note.starttime /= 1000f;
				note.endtime /= 1000f;
			}
		}

		bgm = Resources.Load<AudioClip>("bgms/" + live.bgm_path.Replace(".mp3", ""));
		source.clip = bgm;

		StartGame();
	}

	void StartGame() {
		// Reset state
		rand = new System.Random(seed);
		leftSide = Side.Left;
		rightSide = Side.Right;
		leftHeading = Heading.Down;
		rightHeading = Heading.Down;

		index = 0;
		counter = 0;
		startTime = AudioSettings.dspTime + startDelay;
		LastTime = -startDelay;

		source.PlayScheduled(startTime);

		hasStarted = true;
		isInPrelude = true;
	}

	void Update() {
		player.ManualUpdate();
		leftTip.ManualUpdate();
		rightTip.ManualUpdate();

		if (hasStarted && !isInPrelude && !source.isPlaying) {  // Replay
			StartGame();
		}

		if (isInPrelude) {
			Time = AudioSettings.dspTime - startTime;
			isInPrelude = Time < 0;
		} else {
			Time = source.time;
		}

		// Update time
		Time += timeOffset;
		DeltaTime = Time - LastTime;
		LastTime = Time;
		AccTime += DeltaTime;

		Speed = bufferZ / bufferInterval;

		// Init new blocks
		double bufferTime = Time + bufferInterval;
		while (index < map.lane.Length && map.lane[index].starttime <= bufferTime) {
			if (map.lane[index].parallel) {
				if (counter % level == 0)
					InitBlock(map.lane[index], map.lane[index + 1]);
				index += 2;
			} else {
				if (counter % level == 0)
					InitBlock(map.lane[index]);
				index += 1;
			}

			counter += 1;
		}
			
		// Update live blocks
		var node = liveBlockList.First;
		while (node != null) {
			var next = node.Next;

			var block = node.Value;
			if (shouldAutoPlay && Time >= block.startTime) {
				block.hitSpeed = block.minDyingSpeed * 1.1f;
				block.hitVelocity = HeadingToVector(block.heading) * block.hitSpeed;
				block.shouldDie = true;
			}
			
			if (block.shouldDie) {
				if (!block.shouldDieSilently)
					KillBlock(block);
				
				deadBlockList.AddLast(block);
				liveBlockList.Remove(node);
			} else
				UpdateBlock(block);

			node = next;
		}

		// Wait dead blocks to be inactive
		node = deadBlockList.First;
		while (node != null) {
			var next = node.Next;

			var deadBlock = node.Value;
			if (deadBlock.startTime + recycleInterval + recycleGraceInterval < Time) {  // Ready to be revived
				deadBlock.gameObject.SetActive(false);
				inactiveBlockList.AddLast(deadBlock);
				deadBlockList.Remove(node);
			}

			node = next;
		}
	}

	#region Init Block

	void InitBlock(ApiMapNote note1, ApiMapNote note2) {
		if (note1.lane > note2.lane) {
			var temp = note2;
			note2 = note1;
			note1 = temp;
		}

		if (leftSide == Side.Right) {
			UseLeft(Heading.Right, note1);
			leftSide = Side.Left;
		} else
			UseLeft(leftHeading, note1);

		if (rightSide == Side.Left) {
			UseRight(Heading.Left, note2);
			rightSide = Side.Right;
		} else
			UseRight(rightHeading, note2);
	}

	void InitBlock(ApiMapNote note) {
		int lane = note.lane;

		// 1 3     6 8
		//     [4]
		// 0 2     5 7
		if (lane < 4) {  // left side
			if (rightSide == Side.Left)
				UseRight(rightHeading, note);
			else if (leftSide == Side.Left)
				UseLeft(leftHeading, note);
			else {  // no hand on left, use left hand
				UseLeft(Heading.Right, note);
				leftSide = Side.Left;
			}
		} else if (lane > 4) {  // right side
			if (leftSide == Side.Right)
				UseLeft(leftHeading, note);
			else if (rightSide == Side.Right)
				UseRight(rightHeading, note);
			else {  // no hand on right, use right hand
				UseRight(Heading.Left, note);
				rightSide = Side.Right;
			}
		} else {  // middle
			if (leftSide == Side.Right) {  // left hand on right
				UseLeft(Heading.Right, note);
				leftSide = Side.Left;
			} else if (rightSide == Side.Left) {  // right hand on left
				UseRight(Heading.Left, note);
				rightSide = Side.Right;
			} else {  // no hand is inversed, choose at random
				if (rand.NextDouble() < 0.5) {
					UseLeft(Heading.Left, note);
					leftSide = Side.Right;
				} else {
					UseRight(Heading.Right, note);
					rightSide = Side.Left;
				}
			}
		}
	}

	public double NextGaussian() {
		const double mean = 0 , stdDev= 1;

		double u1 = 1.0 - rand.NextDouble();
		double u2 = 1.0 - rand.NextDouble();
		double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
		return mean + stdDev * randStdNormal;
	}

	public Heading GetDerivedHeading(Heading heading) {
		int offset = (int)NextGaussian();
		return (Heading)(((int)heading + 8 + offset) % 8);
	}

	void UseLeft(Heading heading, ApiMapNote note) {
		heading = GetDerivedHeading(heading);
		CreateBlock(Side.Left, heading, note);
		leftHeading = GetEndHeading(heading);
	}

	void UseRight(Heading heading, ApiMapNote note) {
		heading = GetDerivedHeading(heading);
		CreateBlock(Side.Right, heading, note);
		rightHeading = GetEndHeading(heading);
	}

	void CreateBlock(Side hand, Heading heading, ApiMapNote note) {
		LiveBlock block = null;

		var node = deadBlockList.First;
		while (node != null) {
			var next = node.Next;

			var deadBlock = node.Value;
			if (deadBlock.startTime + recycleInterval < Time) {  // Ready to be revived
				block = deadBlock;
				deadBlockList.Remove(node);
				break;
			}

			node = next;
		}

		if (block == null) {
			node = inactiveBlockList.First;
			if (node != null) {
				block = node.Value;
				block.gameObject.SetActive(true);
				inactiveBlockList.RemoveFirst();
			}
		}

		if (block == null) {
			var go = Instantiate(blockPrototype, transform);
			block = go.GetComponent<LiveBlock>();
		}
			
		block.Init(hand);
		block.heading = heading;

		block.createTime = Time;
		block.startTime = note.starttime;
		block.isParallel = note.parallel;

		var slot = slots[note.lane];
		var startSlot = startSlots[note.lane];
		block.startX = startSlot.x;
		block.startY = startSlot.y;
		block.x = slot.x * 0.5f;
		block.y = slot.y * 0.5f;
		block.transform.Rotate(HeadingToRotation(heading), 0, 0);

		liveBlockList.AddLast(block);
	}

	#endregion

	static bool RayIntersectsTriangle(Vector3 orig, Vector3 dir, Vector3 vert0, Vector3 vert1, Vector3 vert2, out float t) {
		const float EPSILON = 0.0001f; 
		t = 0;

		var edge1 = vert1 - vert0;
		var edge2 = vert2 - vert0;

		var pvec = Vector3.Cross(dir, edge2);
		float det = Vector3.Dot(edge1, pvec);
		if (-EPSILON < det && det < EPSILON)
			return false;
		
		float inv_det = 1f / det;
		var tvec = orig - vert0;
		float u = Vector3.Dot(tvec, pvec) * inv_det;
		if (u < 0 || u > 1)
			return false;

		var qvec = Vector3.Cross(tvec, edge1);
		float v = Vector3.Dot(dir, qvec) * inv_det;
		if (v < 0 || u + v > 1)
			return false;

		t = Vector3.Dot(edge2, qvec) * inv_det;
		return true;
	}

	void UpdateBlock(LiveBlock block) {
		const float damping = 100;

		if (!block.isClosed) {
			double closeTime = LivePlayer.Time - block.createTime;
			if (closeTime > closeDuration) {
				block.isClosed = true;
				ParticleFxPool.Emit(closeParticleFxPreset, block.transform.position, block.transform.rotation);
				FlashFxPool.Flash(closeFlashFxConfig, block.transform.position);

				block.leftShellTrans.localPosition = Vector3.zero;
				block.rightShellTrans.localPosition = Vector3.zero;
			} else {
				block.leftShellTrans.localPosition = new Vector3(0, 0, Easing.Ease(closeEasingType, closeEasingPhase, shellStart, -shellStart, closeTime, closeDuration));
				block.rightShellTrans.localPosition = new Vector3(0, 0, Easing.Ease(closeEasingType, closeEasingPhase, -shellStart, shellStart, closeTime, closeDuration));
			}
		}

		if (Time <= block.startTime) {  // Not yet
			var x = Easing.Ease(bufferEasingTypeX, bufferEasingPhaseX, block.x, bufferX * block.startX, block.startTime - Time, bufferInterval);
			var y = Easing.Ease(bufferEasingTypeY, bufferEasingPhaseY, block.y, bufferY * block.startY, block.startTime - Time, bufferInterval);
			var z = Easing.Ease(bufferEasingTypeZ, bufferEasingPhaseZ, 0, bufferZ, block.startTime - Time, bufferInterval);
			block.transform.localPosition = Vector3.Lerp(block.transform.localPosition, new Vector3(x, y, z), UnityEngine.Time.deltaTime * damping);
		} else if (Time - block.startTime < cacheInterval) {  // Over due
			var x = Easing.Ease(cacheEasingTypeX, cacheEasingPhaseX, block.x, cacheX * block.x, Time - block.startTime, cacheInterval);
			var y = Easing.Ease(cacheEasingTypeY, cacheEasingPhaseY, block.y, cacheY * block.y, Time - block.startTime, cacheInterval);
			var z = Easing.Ease(cacheEasingTypeZ, cacheEasingPhaseZ, 0, cacheZ, Time - block.startTime, cacheInterval);
			block.transform.localPosition = Vector3.Lerp(block.transform.localPosition, new Vector3(x, y, z), UnityEngine.Time.deltaTime * damping);
		} else {  // Miss
			block.shouldDie = true;
			block.shouldDieSilently = true;
			ClearCombo();
		}

		if (block.isClosed && Time - block.startTime < cacheInterval) {  // Detect collision
			if (block.side == Side.Left) {  // Left hand
				if (Vector3.Dot(leftTip.velocity, -block.transform.up) > block.minDyingSpeed)
					DetectCollision(leftTip, block);
			} else {  // Right hand
				if (Vector3.Dot(rightTip.velocity, -block.transform.up) > block.minDyingSpeed)
					DetectCollision(rightTip, block);
			}
		}
	}

	/**
	 * tip  - lastTip
	 *  |   \     |
	 * base - lastBase
	 */
	void DetectCollision(SwordTip tip, LiveBlock block) {
		Vector3 tipPosition = tip.tipPosition, lastTipPosition = tip.lastTipPosition, basePosition = tip.basePosition, lastBasePosition = tip.lastBasePosition;
//		Debug.DrawLine(tipPosition, lastTipPosition, Color.magenta);
//		Debug.DrawLine(tipPosition, lastBasePosition, Color.magenta);
//		Debug.DrawLine(tipPosition, basePosition, Color.magenta);
//		Debug.DrawLine(lastBasePosition, lastTipPosition, Color.magenta);
//		Debug.DrawLine(lastBasePosition, basePosition, Color.magenta);

		float t;
		Vector3 blockTopLeft = block.transform.TransformPoint(blockPoint1), blockTipRight = block.transform.TransformPoint(blockPoint2);
		Debug.DrawLine(blockTopLeft, blockTipRight, Color.cyan);

		if (RayIntersectsTriangle(blockTopLeft, blockTipRight - blockTopLeft, tipPosition, lastTipPosition, lastBasePosition, out t) && 0 <= t && t <= 1)
			block.Collide(tip);
		else if (RayIntersectsTriangle(blockTopLeft, blockTipRight - blockTopLeft, tipPosition, lastBasePosition, basePosition, out t) && 0 <= t && t <= 1)
			block.Collide(tip);
	}

	void KillBlock(LiveBlock block) {
		double offset = Math.Abs(block.startTime - LivePlayer.Time);

		if (offset <= TimeGood)
			AddCombo();
		else
			ClearCombo();

		float inc = block.isParallel ? BaseParaScore : BaseNormalScore;
		inc *= GetScoreOffsetAddon(offset) * GetScoreComboAddon(combo);
		score += (int)inc;

		AudioFxConfig audioConfig;
		string text;
		if (offset <= TimePerfect) {
			text = perfectText;
			audioConfig = block.isParallel ? specialAudioConfigs[Random.Range(0, specialAudioConfigs.Length)] : perfectAudioConfigs[Random.Range(0, perfectAudioConfigs.Length)];
		} else if (offset <= TimeGreat) {
			text = greatText;
			audioConfig = greatAudioConfigs[Random.Range(0, greatAudioConfigs.Length)];
		} else if (offset <= TimeGood) {
			text = goodText;
			audioConfig = goodAudioConfigs[Random.Range(0, goodAudioConfigs.Length)];
		} else {  // if (offset <= TimeBad)
			text = badText;
			audioConfig = badAudioConfigs[Random.Range(0, badAudioConfigs.Length)];
		}

		var blockPosition = block.transform.position;
		hitParticleFxPreset.configs[0].count = score;  // 0th must be CubeCoin
		ParticleFxPool.Emit(hitParticleFxPreset, blockPosition, block.transform.rotation);
		FlashFxPool.Flash(hitFlashFxConfig, blockPosition);
		AudioFxPool.Play(audioConfig, blockPosition);

		block.Die();

		block.canvas.rotation = Quaternion.identity;
		block.uiScoreText.text = string.Format("+{0:N0}", inc);
		block.uiJudgeText.text = text;
		if (combo > 10)
			block.uiComboText.text = string.Format("{0:N0}", combo);

		TweenManager.AddTween(new Tween(recycleInterval, textTween.easingType, textTween.easingPhase, step => {
			float inverse = 1f - step;
			float sizeFloat = Mathf.LerpUnclamped(1, 0.5f, step);
			var size = new Vector3(sizeFloat, sizeFloat, sizeFloat);
			block.uiScoreText.color = new Color(1, 1, 1, inverse);
			block.uiJudgeText.color = new Color(1, 1, 1, inverse);
			block.uiComboText.color = new Color(1, 1, 1, inverse);
			block.uiScoreText.transform.localScale = size;
			block.uiJudgeText.transform.localScale = size;
			block.uiComboText.transform.localScale = size;
		}));
	}

	void AddCombo() {
		combo += 1;
		if (combo > maxCombo)
			maxCombo = combo;
	}

	void ClearCombo() {
		combo = 0;
	}

	void AddScore(int inc) {
		score += inc;
	}
}
