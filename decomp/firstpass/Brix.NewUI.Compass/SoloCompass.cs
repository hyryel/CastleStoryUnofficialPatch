using System;
using Brix.External.Signals;
using UnityEngine;

namespace Brix.NewUI.Compass;

public class SoloCompass : Compass
{
	public Signal<SoloCompass> m_OnBeforeKill = new Signal<SoloCompass>();

	public Signal<SoloCompass> m_OnAfterCreate = new Signal<SoloCompass>();

	public Signal<SoloCompass> m_OnAfterPositionChange = new Signal<SoloCompass>();

	public Signal<SoloCompass> m_OnAfterCullingShow = new Signal<SoloCompass>();

	public Signal<SoloCompass> m_OnAfterCullingHide = new Signal<SoloCompass>();

	private bool firstRun = true;

	public bool cullingFlag;

	public bool groupOverrideFlag;

	public bool finalFlag;

	private GameObject m_ConstrainerGo;

	[SerializeField]
	private TransformConstraint m_TransformConstraint;

	public float groupTimeJoined = -1f;

	private GroupCompass m_GroupCompass;

	private bool isPreviousLocalPositionValid;

	private Vector3 previousLocalPosition = default(Vector3);

	public GameObject constrainerGo
	{
		get
		{
			return m_ConstrainerGo;
		}
		set
		{
			m_ConstrainerGo = value;
		}
	}

	public TransformConstraint transformConstraint
	{
		get
		{
			return m_TransformConstraint;
		}
		set
		{
			m_TransformConstraint = value;
		}
	}

	public GroupCompass groupCompass
	{
		get
		{
			return m_GroupCompass;
		}
		set
		{
			m_GroupCompass = value;
		}
	}

	public void CullingToggle(bool flag)
	{
		if (firstRun)
		{
			firstRun = false;
		}
		cullingFlag = flag;
		finalFlag = cullingFlag;
		ContentToggle(cullingFlag);
		BroadcastCullingToggle();
	}

	public void BroadcastCullingToggle()
	{
		if (cullingFlag)
		{
			m_OnAfterCullingShow.Invoke(this);
		}
		else
		{
			m_OnAfterCullingHide.Invoke(this);
		}
	}

	public void OnAfterGroupShow(GroupCompass groupCompass)
	{
		groupOverrideFlag = true;
		ApplyGroupOverrideFlag();
	}

	public void OnAfterGroupHide(GroupCompass groupCompass)
	{
		groupOverrideFlag = false;
		ApplyGroupOverrideFlag();
	}

	private void ApplyGroupOverrideFlag()
	{
		bool flag = cullingFlag && !groupOverrideFlag;
		if (finalFlag != flag)
		{
			finalFlag = flag;
			ContentToggle(finalFlag);
		}
	}

	public void SetConstrainerGo(GameObject constrainerGo)
	{
		this.constrainerGo = constrainerGo;
		transformConstraint.masterTransform = constrainerGo.transform;
	}

	public void DoCreate(Sprite icon, GameObject go)
	{
		Create(icon);
		SetConstrainerGo(go);
	}

	public void DoCreate(Sprite icon, GameObject go, Action<SoloCompass> OnAfterCreate, Action<SoloCompass> OnBeforeKill)
	{
		if (OnAfterCreate != null)
		{
			m_OnAfterCreate.Connect(OnAfterCreate);
		}
		if (OnBeforeKill != null)
		{
			m_OnBeforeKill.Connect(OnBeforeKill);
		}
		SetConstrainerGo(go);
		Create(icon);
	}

	protected override void Create(Sprite icon)
	{
		base.Create(icon);
		OnCreate();
		OnAfterCreate();
	}

	protected override void OnCreate()
	{
		base.OnCreate();
		ContentHide();
	}

	protected override void OnAfterCreate()
	{
		base.OnAfterCreate();
		m_OnAfterCreate.Invoke(this);
	}

	protected override void OnKill()
	{
		m_OnBeforeKill.Invoke(this);
		base.OnKill();
	}

	protected override void Update()
	{
		TryPositionChange();
		base.Update();
	}

	private void TryPositionChange()
	{
		if (isPreviousLocalPositionValid && previousLocalPosition != base.contentRectTransform.localPosition)
		{
			m_OnAfterPositionChange.Invoke(this);
		}
		previousLocalPosition = base.contentRectTransform.localPosition;
		isPreviousLocalPositionValid = true;
	}
}
