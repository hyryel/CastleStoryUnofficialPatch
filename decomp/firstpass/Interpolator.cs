using System.Collections.Generic;
using UnityEngine;

public class Interpolator : MonoBehaviour
{
	public delegate void Evaluation();

	public static bool isDebug;

	public bool isSelfDebug;

	[SerializeField]
	private bool m_IsEditorDebug;

	[SerializeField]
	private bool m_IsEditorAdvanced;

	[SerializeField]
	private float m_Traverse;

	[SerializeField]
	private float m_DeltaTraverse;

	[SerializeField]
	private float m_DeltaTraverseMin = 0.1f;

	[SerializeField]
	private float m_DeltaTraverseMax = 0.01f;

	[SerializeField]
	private AnimationCurve m_DeltaTraverseCurve = new AnimationCurve();

	[SerializeField]
	private bool m_DeltaTraverseCurveEnabled = true;

	[SerializeField]
	private float m_DeltaDeltaTraverse = 0.001f;

	[SerializeField]
	private float m_DeltaDeltaTraverseMin = 0.1f;

	[SerializeField]
	private float m_DeltaDeltaTraverseMax = 0.01f;

	[SerializeField]
	private AnimationCurve m_DeltaDeltaTraverseCurve = new AnimationCurve();

	[SerializeField]
	private bool m_DeltaDeltaTraverseCurveEnabled = true;

	[SerializeField]
	private bool m_IsQueueTarget;

	[SerializeField]
	private string m_PreviousTargetStateName = string.Empty;

	[SerializeField]
	private string m_CurrentTargetStateName = string.Empty;

	[SerializeField]
	private List<string> m_NextTargetStateNames = new List<string>();

	[SerializeField]
	private string m_PreviousActiveStateName = string.Empty;

	[SerializeField]
	private string m_CurrentActiveStateName = string.Empty;

	[SerializeField]
	private InterpolatorTargetable m_SubInterpolatorTargetable;

	public Evaluation OnEvaluation;

	[SerializeField]
	private bool m_IsPlaying;

	[SerializeField]
	private bool m_IsEvaluating = true;

	public float traverse
	{
		get
		{
			return m_Traverse;
		}
		set
		{
			SetTraverse(m_Traverse, value);
		}
	}

	public float deltaTraverse
	{
		get
		{
			return m_DeltaTraverse;
		}
		set
		{
			m_DeltaTraverse = Mathf.Clamp(value, 0f, 1f);
		}
	}

	public float deltaTraverseMin
	{
		get
		{
			return m_DeltaTraverseMin;
		}
		set
		{
			m_DeltaTraverseMin = value;
		}
	}

	public float deltaTraverseMax
	{
		get
		{
			return m_DeltaTraverseMax;
		}
		set
		{
			m_DeltaTraverseMax = Mathf.Clamp(value, 0f, 1f);
		}
	}

	public AnimationCurve deltaTraverseCurve
	{
		get
		{
			return m_DeltaTraverseCurve;
		}
		set
		{
			m_DeltaTraverseCurve = value;
		}
	}

	public bool deltaTraverseCurveEnabled
	{
		get
		{
			return m_DeltaTraverseCurveEnabled;
		}
		set
		{
			m_DeltaTraverseCurveEnabled = value;
		}
	}

	public float deltaDeltaTraverse
	{
		get
		{
			return m_DeltaDeltaTraverse;
		}
		set
		{
			m_DeltaDeltaTraverse = Mathf.Clamp(value, 0f, 1f);
		}
	}

	public float deltaDeltaTraverseMin
	{
		get
		{
			return m_DeltaDeltaTraverseMin;
		}
		set
		{
			m_DeltaDeltaTraverseMin = value;
		}
	}

	public float deltaDeltaTraverseMax
	{
		get
		{
			return m_DeltaDeltaTraverseMax;
		}
		set
		{
			m_DeltaDeltaTraverseMax = Mathf.Clamp(value, 0f, 1f);
		}
	}

	public AnimationCurve deltaDeltaTraverseCurve
	{
		get
		{
			return m_DeltaDeltaTraverseCurve;
		}
		set
		{
			m_DeltaDeltaTraverseCurve = value;
		}
	}

	public bool deltaDeltaTraverseCurveEnabled
	{
		get
		{
			return m_DeltaDeltaTraverseCurveEnabled;
		}
		set
		{
			m_DeltaDeltaTraverseCurveEnabled = value;
		}
	}

	public bool isQueueTarget
	{
		get
		{
			return m_IsQueueTarget;
		}
		set
		{
			m_IsQueueTarget = value;
		}
	}

	public string previousTargetStateName
	{
		get
		{
			return m_PreviousTargetStateName;
		}
		set
		{
			m_PreviousTargetStateName = value;
		}
	}

	public string currentTargetStateName
	{
		get
		{
			return m_CurrentTargetStateName;
		}
		set
		{
			m_CurrentTargetStateName = value;
		}
	}

	public List<string> nextTargetStateNames
	{
		get
		{
			return m_NextTargetStateNames;
		}
		set
		{
			m_NextTargetStateNames = value;
		}
	}

	public string previousActiveStateName
	{
		get
		{
			return m_PreviousActiveStateName;
		}
		set
		{
			m_PreviousActiveStateName = value;
		}
	}

	public string currentActiveStateName
	{
		get
		{
			return m_CurrentActiveStateName;
		}
		set
		{
			m_CurrentActiveStateName = value;
		}
	}

	public InterpolatorTargetable subInterpolatorTargetable
	{
		get
		{
			return m_SubInterpolatorTargetable;
		}
		set
		{
			m_SubInterpolatorTargetable = value;
		}
	}

	public bool isPlaying
	{
		get
		{
			return m_IsPlaying;
		}
		set
		{
			m_IsPlaying = value;
		}
	}

	public bool isEvaluating
	{
		get
		{
			return m_IsEvaluating;
		}
		set
		{
			m_IsEvaluating = value;
		}
	}

	[ContextMenu("Toggle Debug")]
	public void ToggleEditorDebug()
	{
		m_IsEditorDebug = !m_IsEditorDebug;
	}

	[ContextMenu("Toggle Advanced")]
	public void ToggleEditorAdvanced()
	{
		m_IsEditorAdvanced = !m_IsEditorAdvanced;
	}

	private void SetTraverse(float srcTraverse, float dstTraverse)
	{
		if (srcTraverse != dstTraverse)
		{
			dstTraverse = Mathf.Clamp(dstTraverse, 0f, 1f);
			OnBeforeSetTraverse(srcTraverse, dstTraverse);
			m_Traverse = dstTraverse;
			OnAfterSetTraverse(srcTraverse, dstTraverse);
		}
	}

	protected virtual void OnBeforeSetTraverse(float srcTraverse, float dstTraverse)
	{
	}

	protected virtual void OnAfterSetTraverse(float srcTraverse, float dstTraverse)
	{
	}

	public void Evaluate()
	{
		if (OnEvaluation != null)
		{
			OnEvaluation();
		}
	}

	public void PressPlay()
	{
		isPlaying = true;
	}

	public void PressPause()
	{
		isPlaying = false;
	}

	protected virtual void Play()
	{
		traverse += deltaTraverse;
	}

	public virtual void Start()
	{
		if (isEvaluating)
		{
			Evaluate();
		}
	}

	public virtual void Update()
	{
		Apply();
	}

	public virtual void OnEnable()
	{
	}

	[ContextMenu("Apply")]
	public void Apply()
	{
		if (isPlaying)
		{
			Play();
		}
		if (isEvaluating)
		{
			Evaluate();
		}
	}
}
