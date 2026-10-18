using System;
using System.Collections.Generic;
using Brix.Utils.Logging;
using UnityEngine;

public class ScreenEdgeConstraint : MonoBehaviour
{
	public enum ConstraintEvent
	{
		None,
		Update,
		LateUpdate
	}

	private static BrixLogger logger = new BrixLogger(typeof(ScreenEdgeConstraint).FullName);

	private float inAngle;

	[Disabled]
	public float outAngle;

	private Vector2 cPos2 = default(Vector2);

	private Vector3 cPos3 = default(Vector3);

	[SerializeField]
	private bool m_ForceToEdge;

	[SerializeField]
	private ConstraintEvent m_ConstraintEvent;

	public RectTransform srcRectTransform;

	[SerializeField]
	private RectTransform m_TargetRectTransform;

	[SerializeField]
	private RectTransform m_TargetRotateRectTransform;

	[SerializeField]
	private Vector2 m_Padding = default(Vector2);

	[SerializeField]
	private float m_Power = 4f;

	public bool useCache;

	private Dictionary<int, float> intAngleDistanceMap = new Dictionary<int, float>();

	private Dictionary<int, Vector2> intAnglePosMap = new Dictionary<int, Vector2>();

	private Dictionary<int, float> intAngleCosMap = new Dictionary<int, float>();

	private Dictionary<int, float> intAngleSinMap = new Dictionary<int, float>();

	private Vector2 halfScreenSize = default(Vector2);

	public bool forceToEdge
	{
		get
		{
			return m_ForceToEdge;
		}
		set
		{
			m_ForceToEdge = value;
		}
	}

	public ConstraintEvent constraintEvent
	{
		get
		{
			return m_ConstraintEvent;
		}
		set
		{
			m_ConstraintEvent = value;
		}
	}

	public RectTransform targetRectTransform
	{
		get
		{
			return m_TargetRectTransform;
		}
		set
		{
			m_TargetRectTransform = value;
		}
	}

	public RectTransform targetRotateRectTransform
	{
		get
		{
			return m_TargetRotateRectTransform;
		}
		set
		{
			m_TargetRotateRectTransform = value;
		}
	}

	public Vector2 padding
	{
		get
		{
			return m_Padding;
		}
		set
		{
			m_Padding = value;
		}
	}

	public float power
	{
		get
		{
			return m_Power;
		}
		set
		{
			m_Power = value;
		}
	}

	public void SetAngle(float angle)
	{
		inAngle = angle;
		Constrain();
	}

	public void Constrain()
	{
		UpdateRectTransformPosition();
	}

	private void OnEnable()
	{
		UpdateScreenSize();
		RebuildCache();
		Constrain();
	}

	private void Start()
	{
		UpdateScreenSize();
		RebuildCache();
		Constrain();
	}

	private void Update()
	{
		if (constraintEvent == ConstraintEvent.Update)
		{
			Constrain();
		}
	}

	private void LateUpdate()
	{
		if (constraintEvent == ConstraintEvent.LateUpdate)
		{
			Constrain();
		}
	}

	private void OnRectTransformDimensionsChange()
	{
		UpdateScreenSize();
		RebuildCache();
		UpdateRectTransformPosition();
	}

	private void UpdateRectTransformPosition()
	{
		if (targetRectTransform == null)
		{
			return;
		}
		if (srcRectTransform == null)
		{
			outAngle = inAngle;
			cPos2 = GetPosition(outAngle, GetDistance(outAngle));
			cPos3.x = cPos2.x;
			cPos3.y = cPos2.y;
			targetRectTransform.localPosition = cPos3;
			cPos3.x = 0f;
			cPos3.y = 0f;
			cPos3.z = outAngle;
			targetRotateRectTransform.localEulerAngles = cPos3;
			return;
		}
		cPos3 = srcRectTransform.localPosition;
		outAngle = Mathf.Repeat(Mathf.Atan2(cPos3.y, cPos3.x) * 57.29578f, 360f);
		float num = Mathf.Pow(Mathf.Pow(cPos3.x, 2f) + Mathf.Pow(cPos3.y, 2f), 0.5f);
		if (useCache)
		{
			int num2 = Mathf.RoundToInt(outAngle);
			num2 = ((num2 != 360) ? num2 : 0);
			if (forceToEdge || num > GetCachedDistance(num2))
			{
				cPos2 = GetCachedPosition(num2);
				cPos3.x = cPos2.x;
				cPos3.y = cPos2.y;
			}
		}
		else
		{
			float distance = GetDistance(outAngle);
			if (forceToEdge || num > distance)
			{
				cPos2 = GetPosition(outAngle, distance);
				cPos3.x = cPos2.x;
				cPos3.y = cPos2.y;
			}
		}
		targetRectTransform.localPosition = cPos3;
		cPos3.x = 0f;
		cPos3.y = 0f;
		cPos3.z = outAngle;
		targetRotateRectTransform.localEulerAngles = cPos3;
	}

	private float EvaluateDistance(float t)
	{
		float f = t * (float)Math.PI * 2f;
		cPos2.x = Mathf.Cos(f);
		cPos2.y = Mathf.Sin(f);
		cPos2.x = Mathf.Abs(cPos2.x);
		cPos2.y = Mathf.Abs(cPos2.y);
		cPos2.x /= halfScreenSize.x;
		cPos2.y /= halfScreenSize.y;
		float num = Mathf.Pow(Mathf.Pow(cPos2.x, power) + Mathf.Pow(cPos2.y, power), 1f / power);
		return 1f / num;
	}

	private float GetDistance(float angle)
	{
		float t = angle / 360f;
		return EvaluateDistance(t);
	}

	public float GetCachedDistance(int intAngle)
	{
		if (!intAngleDistanceMap.ContainsKey(intAngle))
		{
			logger.Error("Missing cache. Aborting.");
			return 0f;
		}
		return intAngleDistanceMap[intAngle];
	}

	private Vector2 GetPosition(int intAngle, float distance)
	{
		cPos2.x = intAngleCosMap[intAngle] * distance;
		cPos2.y = intAngleSinMap[intAngle] * distance;
		return cPos2;
	}

	private Vector2 GetPosition(float angle, float distance)
	{
		float f = angle * ((float)Math.PI / 180f);
		cPos2.x = Mathf.Cos(f) * distance;
		cPos2.y = Mathf.Sin(f) * distance;
		return cPos2;
	}

	private Vector2 GetCachedPosition(int intAngle)
	{
		if (!intAnglePosMap.ContainsKey(intAngle))
		{
			logger.Error("Missing cache. Aborting.\n" + intAngle);
			return Vector2.zero;
		}
		return intAnglePosMap[intAngle];
	}

	public void RebuildCache()
	{
		if (useCache)
		{
			intAngleCosMap.Clear();
			intAngleSinMap.Clear();
			intAngleDistanceMap.Clear();
			intAnglePosMap.Clear();
			for (int i = 0; i < 360; i++)
			{
				float num = i;
				float f = num * ((float)Math.PI / 180f);
				intAngleCosMap.Add(i, Mathf.Cos(f));
				intAngleSinMap.Add(i, Mathf.Sin(f));
				intAngleDistanceMap.Add(i, GetDistance(num));
				intAnglePosMap.Add(i, GetPosition(i, intAngleDistanceMap[i]));
			}
		}
	}

	private void UpdateScreenSize()
	{
		halfScreenSize.x = 0.5f * (float)Screen.width;
		halfScreenSize.y = 0.5f * (float)Screen.height;
		halfScreenSize.x -= 0.5f * padding.x;
		halfScreenSize.y -= 0.5f * padding.y;
	}

	private void OnAfterScreenSizeChange()
	{
		UpdateScreenSize();
	}
}
