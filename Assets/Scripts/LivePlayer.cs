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

	public AudioSource source;

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

	[Header("Block Close")]
	public float closeDuration;
	public float shellStart;
	public EasingType closeEasingType;
	public EasingPhase closeEasingPhase;
	public ParticleFxPreset closeFxPreset;
	public FlashFxConfig closeFlashFxConfig;

	[Header("Block Sound")]
	public float volume;
	public AudioClip[] specialClips, perfectClips, greatClips, goodClips, badClips, missClips;

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

	void UpdateBlock(LiveBlock block) {
		const float damping = 100;

		if (!block.isClosed) {
			double closeTime = LivePlayer.Time - block.createTime;
			if (closeTime > closeDuration) {
				block.isClosed = true;
				ParticleFxPool.Emit(closeFxPreset, block.transform.position, block.transform.rotation);
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
	}

	void KillBlock(LiveBlock block) {
		double offset = Math.Abs(block.startTime - LivePlayer.Time);

		var clip = missClips[Random.Range(0, missClips.Length)];
		if (offset <= TimePerfect)
			clip = block.isParallel ? specialClips[Random.Range(0, specialClips.Length)] : perfectClips[Random.Range(0, perfectClips.Length)];
		else if (offset <= TimeGreat)
			clip = greatClips[Random.Range(0, greatClips.Length)];
		else if (offset <= TimeGood)
			clip = goodClips[Random.Range(0, goodClips.Length)];
		else if (offset <= TimeBad)
			clip = badClips[Random.Range(0, badClips.Length)];

		block.Die(clip, volume);

		if (offset <= TimeGood)
			AddCombo();
		else
			ClearCombo();
		
		if (offset > TimeBad)  // No score for you!
			return;

		float inc = block.isParallel ? BaseParaScore : BaseNormalScore;
		inc *= GetScoreOffsetAddon(offset) * GetScoreComboAddon(combo);
		AddScore((int)inc);
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
