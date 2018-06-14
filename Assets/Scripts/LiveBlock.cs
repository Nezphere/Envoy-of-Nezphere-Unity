using UnityEngine;

public class LiveBlock : MonoBehaviour {
	public Collider collier;
	public AudioSource source;
	public Transform leftShellTrans, rightShellTrans;
	public Renderer leftCoreRenderer, rightCoreRenderer;

	[Header("Config")]
	public Side side;
	public float minDyingSpeed = 12;
	public float velocityScaling = 1, randomForce = 1, randomTorque = 1;
	public Material leftMaterial, rightMaterial;
	public ParticleFxPreset hitFxPreset;

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

		var material = side == Side.Left ? leftMaterial : rightMaterial;
		leftCoreRenderer.sharedMaterial = material;
		rightCoreRenderer.sharedMaterial = material;

		isClosed = false;
		shouldDie = shouldDieSilently = false;

		foreach (var rigid in GetComponentsInChildren<Rigidbody>()) {
			rigid.isKinematic = true;
			rigid.velocity = Vector3.zero;
			rigid.transform.localPosition = Vector3.zero;
			rigid.transform.localRotation = Quaternion.identity;
		}

		collier.enabled = true;
	}

	public void Die(AudioClip clip, float volume) {
		collier.enabled = false;

		ParticleFxPool.Emit(hitFxPreset, transform.position, transform.rotation);
		
		foreach (var rigid in GetComponentsInChildren<Rigidbody>()) {
			rigid.isKinematic = false;
			rigid.velocity = hitVelocity * velocityScaling;
			rigid.AddRelativeForce(Random.insideUnitSphere * randomForce, ForceMode.Impulse);
			rigid.AddTorque(Random.insideUnitSphere * randomTorque, ForceMode.Impulse);
		}
		
		source.volume = volume * hitSpeed / minDyingSpeed;
		source.clip = clip;
		source.Play();
	}

	void OnTriggerEnter(Collider other) {
		if (shouldDie)
			return;

		var tip = other.GetComponentInChildren<SwordTip>();
		if (tip == null || tip.side != side)
			return;

		if (Vector3.Dot(tip.velocity, -transform.up) > minDyingSpeed) {
			shouldDie = true;

			hitSpeed = tip.speed;
			hitVelocity = tip.velocity;

			collier.enabled = false;
		}
	}

	void OnTriggerStay(Collider other) {
		OnTriggerEnter(other);
	}
}
