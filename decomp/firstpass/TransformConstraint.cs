using Brix.Game.Components;
using Brix.Util;
using UnityEngine;
using UnityEngine.Events;

public class TransformConstraint : UpdatableGameComponent<TransformConstraint>
{
	public enum CameraSpace
	{
		Screen,
		Viewport
	}

	private Camera cam;

	private Transform camTransform;

	public Transform masterTransform;

	public RectTransform slaveRectTransform;

	public Vector3 worldOffset = Vector3.zero;

	public Vector3 screenOffset = Vector3.zero;

	[Disabled]
	public float outDistance;

	public bool inverseWhenBehindCamera = true;

	public CameraSpace cameraSpace;

	private Vector3 cPosA = default(Vector3);

	private Vector3 cPosB = default(Vector3);

	public UnityEvent m_OnAfterConstrain = new UnityEvent();

	public void Constrain()
	{
		if (cam == null)
		{
			cam = Camera.main;
			camTransform = cam.transform;
		}
		if (!(masterTransform == null) && !(slaveRectTransform == null))
		{
			cPosB = ((cameraSpace != CameraSpace.Screen) ? cam.WorldToViewportPoint(masterTransform.position + worldOffset) : cam.WorldToScreenPoint(masterTransform.position + worldOffset));
			outDistance = cPosB.z;
			if (inverseWhenBehindCamera && IsBehindCamera())
			{
				cPosB.x *= -1f;
				cPosB.y *= -1f;
			}
			cPosA.x = cPosB.x;
			cPosA.y = cPosB.y;
			cPosA.z = slaveRectTransform.position.z;
			slaveRectTransform.position = Calcule.RoundVecteur(cPosA) + screenOffset;
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		Constrain();
	}

	public bool IsBehindCamera()
	{
		Transform transform = camTransform;
		Vector3 lhs = masterTransform.position - transform.position;
		if (Vector3.Dot(lhs, transform.forward) < 0f)
		{
			return true;
		}
		return false;
	}

	protected override void Start()
	{
		base.Start();
		Constrain();
	}

	protected override void BrixUpdate()
	{
		Constrain();
	}
}
