using UnityEngine;
using UnityEngine.UI;

public class LiveBlock : MonoBehaviour {
	public Transform leftShellTrans, rightShellTrans;
	public Renderer leftCoreRenderer, rightCoreRenderer;
	public Light leftCoreLight, rightCoreLight;
	public TransformResetter resetter;
	public Text uiScoreText, uiJudgeText, uiComboText;
	public Transform canvas;

	[Header("Config")]
	public Side side;
	public float minDyingSpeed = 12;
	public float velocityScaling = 1, randomForce = 1, randomTorque = 1;
	public Material leftMaterial, rightMaterial;

	[Header("Close")]
	public bool isClosed;

	[Header("Note")]
	public double startTime;
	public bool isParallel;

	[Header("Block")]
	public double createTime;
	public Heading heading;
	public float startX, startY, x, y;
	public bool shouldDie, shouldDieSilently;

	[Header("Dying Message")]
	public float hitSpeed;
	public Vector3 hitVelocity;


	public void Init(Side side) {
		this.side = side;

		uiScoreText.text = "";
		uiJudgeText.text = "";
		uiComboText.text = "";

		var material = side == Side.Left ? leftMaterial : rightMaterial;
		leftCoreRenderer.sharedMaterial = material;
		rightCoreRenderer.sharedMaterial = material;

		var color = material.GetColor("_EmissionColor");
		leftCoreLight.color = color;
		rightCoreLight.color = color;
			

		isClosed = false;
		shouldDie = shouldDieSilently = false;

		foreach (var rigid in GetComponentsInChildren<Rigidbody>()) {
			rigid.isKinematic = true;
			rigid.velocity = Vector3.zero;
			rigid.GetComponent<TransformResetter>().ResetTransform();
		}

		resetter.ResetTransform();
	}

	public void Die() {
		foreach (var rigid in GetComponentsInChildren<Rigidbody>()) {
			rigid.isKinematic = false;
			rigid.velocity = hitVelocity * velocityScaling;
			rigid.AddRelativeForce(Random.insideUnitSphere * randomForce, ForceMode.Impulse);
			rigid.AddTorque(Random.insideUnitSphere * randomTorque, ForceMode.Impulse);
		}
	}

	public void Collide(SwordTip tip) {
		shouldDie = true;

		hitSpeed = tip.speed;
		hitVelocity = tip.velocity;
	}
}
