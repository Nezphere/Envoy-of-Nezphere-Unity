using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using Uif;
using LoveLivePractice.Api;

using Math = System.Math;

public class LivePlayer : MonoBehaviour {
	#region Statics

	public static LivePlayer Instance;
	public static float Speed;

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
	public const float TimePerfect = 0.05f, TimeGreat = 0.15f, TimeGood = 0.2f, TimeBad = 0.5f;

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
		if (combo <= 25)
			return 1.0f;
		if (combo <= 50)
			return 1.1f;
		if (combo <= 100)
			return 1.15f;
		if (combo <= 200)
			return 1.2f;
		if (combo <= 300)
			return 1.25f;
		if (combo <= 400)
			return 1.3f;
		return 1.35f;
	}

	#endregion

	public VrPlayer player;
	public MusicPlayer musicPlayer;
	public SwordTip leftTip, rightTip;

	[Header("Score")]
	public Text uiScoreText, uiComboText, uiIncText;
	public Tween uiScoreTextTween, uiComboTextTween, uiIncTextTween;

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
	public FlashFxConfig badFlashFxConfig;
	public Material perfectMaterial, greatMaterial, goodMaterial, badMaterial;

	[Header("Game State")]
	public int score;
	public int combo, maxCombo;
	public Side leftSide = Side.Left, rightSide = Side.Right;
	public Heading leftHeading = Heading.Down, rightHeading = Heading.Down;

	[HideInInspector]
	public LiveNote[] notes;

	System.Random rand;
	double startTime;
	int index, counter;
	bool hasStarted, isInPrelude;

	readonly LinkedList<LiveBlock> liveBlockList = new LinkedList<LiveBlock>(), deadBlockList = new LinkedList<LiveBlock>(), inactiveBlockList = new LinkedList<LiveBlock>();

	void Start() {
		Instance = this;

		uiScoreTextTween.isDead = true;
		uiScoreTextTween.doStayAlive = true;
		uiScoreTextTween.SetTransition(step => {
			float size = 1.5f + (1f - step) * 1f;
			uiScoreText.transform.localScale = new Vector3(size, size, size);
		});
		TweenManager.AddTween(uiScoreTextTween, false);
		uiComboTextTween.isDead = true;
		uiComboTextTween.doStayAlive = true;
		uiComboTextTween.SetTransition(step => {
			float size = 1f + (1f - step) * 0.2f;
			uiComboText.transform.localScale = new Vector3(size, size, size);
		});
		TweenManager.AddTween(uiComboTextTween, false);
		uiIncTextTween.isDead = true;
		uiIncTextTween.doStayAlive = true;
		uiIncTextTween.SetTransition(step => {
//			float size = 1.5f + (1f - step) * 0.2f;
			float size = (1f - step) * 1.5f;
			uiIncText.transform.localScale = new Vector3(size, size, size);
		});
		TweenManager.AddTween(uiIncTextTween, false);
	}

	public void StartGame() {
		// Reset state
		rand = new System.Random(seed);
		leftSide = Side.Left;
		rightSide = Side.Right;
		leftHeading = Heading.Down;
		rightHeading = Heading.Down;

		index = 0;
		counter = 0;

		hasStarted = true;
		isInPrelude = false;
	}

	public void ManualUpdate() {
		player.ManualUpdate();
		leftTip.ManualUpdate();
		rightTip.ManualUpdate();

		if (!hasStarted)
			return;

		if (hasStarted && !isInPrelude && !MusicPlayer.IsPlaying) {  // Replay
//			source.pitch *= 1.1f;
			MusicPlayer.Restart();
			StartGame();
		}

		Speed = bufferZ / bufferInterval;

		// Init new blocks
		double bufferTime = MusicPlayer.LiveTime + bufferInterval;
		while (index < notes.Length && notes[index].startTime <= bufferTime) {
			if (notes[index].isPara) {
				if (counter % level == 0)
					InitBlock(notes[index], notes[index + 1]);
				index += 2;
			} else {
				if (counter % level == 0)
					InitBlock(notes[index]);
				index += 1;
			}

			counter += 1;
		}
			
		// Update live blocks
		var node = liveBlockList.First;
		while (node != null) {
			var next = node.Next;

			var block = node.Value;
			if (shouldAutoPlay && (Math.Abs(MusicPlayer.LiveTime - block.startTime) < TimePerfect || MusicPlayer.LiveTime > block.startTime)) {
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
			if (deadBlock.startTime + recycleInterval + recycleGraceInterval < MusicPlayer.LiveTime) {  // Ready to be revived
				deadBlock.gameObject.SetActive(false);
				inactiveBlockList.AddLast(deadBlock);
				deadBlockList.Remove(node);
			}

			node = next;
		}
	}

	#region Init Block

	void InitBlock(LiveNote note1, LiveNote note2) {
		if (note1.x > note2.x) {
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

	void InitBlock(LiveNote note) {
		float lane = note.x;
		const float middle = 0.2f;

		// 1 3     6 8
		//     [4]
		// 0 2     5 7
		if (lane < -middle) {  // left side
			if (rightSide == Side.Left)
				UseRight(rightHeading, note);
			else if (leftSide == Side.Left)
				UseLeft(leftHeading, note);
			else {  // no hand on left, use left hand
				UseLeft(Heading.Right, note);
				leftSide = Side.Left;
			}
		} else if (lane > middle) {  // right side
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

	void UseLeft(Heading heading, LiveNote note) {
		heading = GetDerivedHeading(heading);
		CreateBlock(Side.Left, heading, note);
		leftHeading = GetEndHeading(heading);
	}

	void UseRight(Heading heading, LiveNote note) {
		heading = GetDerivedHeading(heading);
		CreateBlock(Side.Right, heading, note);
		rightHeading = GetEndHeading(heading);
	}

	void CreateBlock(Side hand, Heading heading, LiveNote note) {
		LiveBlock block = null;

		var node = deadBlockList.First;
		while (node != null) {
			var next = node.Next;

			var deadBlock = node.Value;
			if (deadBlock.startTime + recycleInterval < MusicPlayer.LiveTime) {  // Ready to be revived
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

		block.createTime = MusicPlayer.LiveTime;
		block.startTime = note.startTime;
		block.isParallel = note.isPara;
		block.isLong = note.isSpecial;

		block.startX = note.x;
		block.startY = note.y + 1;
		block.x = note.x;
		block.y = note.y;
		block.transform.Rotate(HeadingToRotation(heading), 0, 0);

		block.canvas.rotation = Quaternion.identity;
//		block.uiDirectionTrans.localEulerAngles = new Vector3(0, 0, -HeadingToRotation(heading));

		liveBlockList.AddLast(block);
	}

	#endregion

	public static bool RayTriangleIntersection(Vector3 orig, Vector3 dir, Vector3 vert0, Vector3 vert1, Vector3 vert2, out float t) {
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

	/**
	 * vert0 - vert1
	 *   |   \   |
	 * vert3 - vert2
	 */
	public static bool RayQuadIntersection(Vector3 orig, Vector3 dir, Vector3 vert0, Vector3 vert1, Vector3 vert2, Vector3 vert3, out float t) {
		return RayTriangleIntersection(orig, dir, vert0, vert1, vert2, out t) || RayTriangleIntersection(orig, dir, vert0, vert2, vert3, out t);
	}

	void UpdateBlock(LiveBlock block) {
//		const float damping = 100;

		if (!block.isClosed) {
			double closeTime = MusicPlayer.LiveTime - block.createTime;
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

		if (MusicPlayer.LiveTime <= block.startTime) {  // Not yet
			var offset = block.startTime - MusicPlayer.LiveTime;
			var x = Easing.Ease(bufferEasingTypeX, bufferEasingPhaseX, block.x, bufferX * block.startX, offset, bufferInterval);
			var y = Easing.Ease(bufferEasingTypeY, bufferEasingPhaseY, block.y, bufferY * block.startY, offset, bufferInterval);
			var z = Easing.Ease(bufferEasingTypeZ, bufferEasingPhaseZ, 0, bufferZ, offset, bufferInterval);
//			block.transform.localPosition = Vector3.Lerp(block.transform.localPosition, new Vector3(x, y, z), UnityEngine.Time.deltaTime * damping);
			block.transform.localPosition = new Vector3(x, y, z);
		} else if (MusicPlayer.LiveTime - block.startTime < cacheInterval && MusicPlayer.LiveTime - block.startTime < TimeBad) {  // Over due
			var offset = MusicPlayer.LiveTime - block.startTime;
			var x = Easing.Ease(cacheEasingTypeX, cacheEasingPhaseX, block.x, cacheX * block.x, offset, cacheInterval);
			var y = Easing.Ease(cacheEasingTypeY, cacheEasingPhaseY, block.y, cacheY * block.y, offset, cacheInterval);
			var z = Easing.Ease(cacheEasingTypeZ, cacheEasingPhaseZ, 0, cacheZ, offset, cacheInterval);
//			block.transform.localPosition = Vector3.Lerp(block.transform.localPosition, new Vector3(x, y, z), UnityEngine.Time.deltaTime * damping);
			block.transform.localPosition = new Vector3(x, y, z);
		} else {  // Miss
			AudioFxPool.Play(badAudioConfigs[Random.Range(0, badAudioConfigs.Length)], block.transform.position);
			FlashFxPool.Flash(badFlashFxConfig, block.transform.position);

			uiScoreText.material = badMaterial;
			uiComboText.material = badMaterial;
			uiIncText.text = "";
			uiComboText.text = "x 0 x";
			uiComboTextTween.isDead = false;
			uiComboTextTween.time = 0;

			block.shouldDie = true;
			block.shouldDieSilently = true;
			combo = 0;
		}

		if (block.isClosed && MusicPlayer.LiveTime - block.startTime < cacheInterval) {  // Detect collision
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

		if (RayQuadIntersection(blockTopLeft, blockTipRight - blockTopLeft, tipPosition, lastTipPosition, lastBasePosition, basePosition, out t) && 0 <= t && t <= 1)
			block.Collide(tip);
	}

	void KillBlock(LiveBlock block) {
		double offset = Math.Abs(block.startTime - MusicPlayer.LiveTime);

		if (offset <= TimeGood) {
			combo += 1;
			if (combo > maxCombo)
				maxCombo = combo;
		} else {
			combo = 0;
		}

		float inc = block.isParallel ? BaseParaScore : BaseNormalScore;
		inc *= GetScoreOffsetAddon(offset) * GetScoreComboAddon(combo);
		score += (int)inc;

		AudioFxConfig audioConfig;
		Material material;
		string text;
//		float scale;
		if (offset <= TimePerfect) {
//			scale = 2;
			material = perfectMaterial;
			text = perfectText;
			audioConfig = block.isLong ? specialAudioConfigs[Random.Range(0, specialAudioConfigs.Length)] : perfectAudioConfigs[Random.Range(0, perfectAudioConfigs.Length)];
		} else if (offset <= TimeGreat) {
//			scale = 1.5f;
			material = greatMaterial;
			text = greatText;
			audioConfig = greatAudioConfigs[Random.Range(0, greatAudioConfigs.Length)];
		} else if (offset <= TimeGood) {
//			scale = 1;
			material = goodMaterial;
			text = goodText;
			audioConfig = goodAudioConfigs[Random.Range(0, goodAudioConfigs.Length)];
		} else {  // if (offset <= TimeBad)
//			scale = 0.5f;
			material = badMaterial;
			text = badText;
			audioConfig = badAudioConfigs[Random.Range(0, badAudioConfigs.Length)];
		}

		var blockPosition = block.transform.position;
		hitParticleFxPreset.configs[0].count = combo << 1;  // 0th must be CubeCoin
		ParticleFxPool.Emit(hitParticleFxPreset, blockPosition, block.transform.rotation);
		FlashFxPool.Flash(hitFlashFxConfig, blockPosition);
		AudioFxPool.Play(audioConfig, blockPosition);

		uiScoreText.material = material;
		uiIncText.material = material;
		uiComboText.material = material;
		uiScoreText.text = score.ToString("N0");
		uiIncText.text = "+" + inc.ToString("N0");
		uiComboText.text = string.Format(combo == 0 ? "x {0:N0} x" : combo == maxCombo ? "^ {0:N0} ^" : "+ {0:N0} +", combo);
		uiScoreTextTween.isDead = false;
		uiScoreTextTween.time = 0;
		uiComboTextTween.isDead = false;
		uiComboTextTween.time = 0;
		uiIncTextTween.isDead = false;
		uiIncTextTween.time = 0;

		block.Die();

		block.uiComboText.material = material;
		block.uiJudgeText.material = material;
		block.uiScoreText.material = material;

		block.uiScoreText.text = string.Format("+{0:N0}", inc);
		block.uiJudgeText.text = text;
		if (combo > 10)
			block.uiComboText.text = string.Format("{0:N0}", combo);

		TweenManager.AddTween(new Tween(recycleInterval, textTween.easingType, textTween.easingPhase, step => {
			float inverse = (1f - step);// * scale;
			var size = new Vector3(inverse, inverse, inverse);
//			block.uiScoreText.color = new Color(1, 1, 1, inverse);
//			block.uiJudgeText.color = new Color(1, 1, 1, inverse);
//			block.uiComboText.color = new Color(1, 1, 1, inverse);
			block.uiScoreText.transform.localScale = size;
			block.uiJudgeText.transform.localScale = size;
			block.uiComboText.transform.localScale = size;
		}));
	}
}
