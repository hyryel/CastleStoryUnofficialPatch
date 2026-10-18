using System;
using System.Collections.Generic;
using Brix.External.Signals;
using Brix.Utils;
using UnityEngine;

namespace Brix.NewUI.Compass;

public class GroupCompass : Compass
{
	[SerializeField]
	private GameObject m_SoloCompassCloneBin;

	[SerializeField]
	private GameObject m_DefaultSoloCompassCloneBin;

	private Signal<GroupCompass> m_OnBeforeKill = new Signal<GroupCompass>();

	private Signal<GroupCompass> m_OnAfterCreate = new Signal<GroupCompass>();

	public Signal<GroupCompass> m_OnAfterShow = new Signal<GroupCompass>();

	public Signal<GroupCompass> m_OnAfterHide = new Signal<GroupCompass>();

	public bool firstRun = true;

	public bool contentFlag;

	public List<SoloCompass> m_SoloCompasses = new List<SoloCompass>();

	protected Dictionary<SoloCompass, List<SignalConnection>> Connections = new Dictionary<SoloCompass, List<SignalConnection>>();

	public CompassGrouper grouper;

	public bool lateUpdatePos;

	public List<float> angleListA = new List<float>();

	public List<float> angleListB = new List<float>();

	public List<SoloCompass> soloCompasses => m_SoloCompasses;

	private void AutoToggle()
	{
		int num = 0;
		foreach (SoloCompass soloCompass in soloCompasses)
		{
			if (soloCompass.cullingFlag)
			{
				num++;
			}
		}
		bool flag = num >= 1;
		base.count = num;
		if (firstRun)
		{
			firstRun = false;
		}
		else if (contentFlag != flag)
		{
		}
		contentFlag = flag;
		ContentToggle(flag);
		BroadcastToggle();
		lateUpdatePos = true;
	}

	private void BroadcastToggle()
	{
		if (contentFlag)
		{
			m_OnAfterShow.Invoke(this);
		}
		else
		{
			m_OnAfterHide.Invoke(this);
		}
	}

	private void OnAfterSoloCullingShow(SoloCompass soloCompass)
	{
		AutoToggle();
	}

	private void OnAfterSoloCullingHide(SoloCompass soloCompass)
	{
		AutoToggle();
	}

	public List<SoloCompass> Add(SoloCompass soloCompass)
	{
		if (soloCompass.groupTimeJoined != -1f && soloCompass.groupTimeJoined < Time.time + 1f && grouper.lateAssignement)
		{
			return soloCompasses;
		}
		if (soloCompass.groupCompass == this)
		{
			return soloCompasses;
		}
		if (soloCompass.groupCompass != null)
		{
			soloCompass.groupCompass.Remove(soloCompass);
		}
		if (!m_SoloCompasses.Contains(soloCompass))
		{
			m_SoloCompasses.Add(soloCompass);
		}
		soloCompass.groupCompass = this;
		if (grouper.lateAssignement)
		{
			soloCompass.groupTimeJoined = Time.time;
		}
		soloCompass.transform.SetParent(m_SoloCompassCloneBin.transform, worldPositionStays: false);
		List<SignalConnection> list = new List<SignalConnection>();
		list.Add(soloCompass.m_OnAfterCullingShow.Connect(OnAfterSoloCullingShow));
		list.Add(soloCompass.m_OnAfterCullingHide.Connect(OnAfterSoloCullingHide));
		list.Add(m_OnAfterShow.Connect(soloCompass.OnAfterGroupShow));
		list.Add(m_OnAfterHide.Connect(soloCompass.OnAfterGroupHide));
		Connections.Add(soloCompass, list);
		AutoToggle();
		BroadcastToggle();
		return soloCompasses;
	}

	public List<SoloCompass> Remove(SoloCompass soloCompass)
	{
		if (!soloCompasses.Contains(soloCompass) || soloCompass.groupCompass != this)
		{
			return soloCompasses;
		}
		soloCompass.transform.SetParent(m_DefaultSoloCompassCloneBin.transform, worldPositionStays: false);
		List<SignalConnection> value = Connections.GetAndRemove(soloCompass).Value;
		if (value != null)
		{
			foreach (SignalConnection item in value)
			{
				item.Disconnect();
			}
		}
		if (m_SoloCompasses.Contains(soloCompass))
		{
			m_SoloCompasses.Remove(soloCompass);
			soloCompass.groupCompass = null;
		}
		AutoToggle();
		return soloCompasses;
	}

	public List<SoloCompass> Clear()
	{
		foreach (SoloCompass soloCompass in soloCompasses)
		{
			soloCompass.groupCompass = null;
		}
		m_SoloCompasses.Clear();
		return soloCompasses;
	}

	public void DoCreate(Sprite icon, Action<GroupCompass> OnAfterCreate = null, Action<GroupCompass> OnBeforeKill = null)
	{
		Create(icon);
		if (OnAfterCreate != null)
		{
			m_OnAfterCreate.Connect(OnAfterCreate);
		}
		if (OnBeforeKill != null)
		{
			m_OnBeforeKill.Connect(OnBeforeKill);
		}
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

	private void LateUpdate()
	{
		TryLateUpdatePosition();
	}

	private void TryLateUpdatePosition()
	{
		if (lateUpdatePos)
		{
			lateUpdatePos = false;
			UpdatePosition();
		}
	}

	private void UpdatePosition()
	{
		angleListA.Clear();
		angleListB.Clear();
		float num = 0f;
		int num2 = 0;
		bool flag = true;
		float num3 = 0f;
		foreach (SoloCompass soloCompass in soloCompasses)
		{
			if (!soloCompass.cullingFlag)
			{
				continue;
			}
			num2++;
			float num4 = soloCompass.screenEdgeConstraint.outAngle;
			angleListA.Add(num4);
			if (!flag)
			{
				float num5 = num3 - num4;
				if (Mathf.Abs(num5) > 180f)
				{
					if (num5 >= 0f)
					{
						num4 -= 360f;
						angleListB.Add(num4);
					}
					else
					{
						num4 += 360f;
						angleListB.Add(num4);
					}
				}
				else
				{
					angleListB.Add(num4);
				}
			}
			else
			{
				angleListB.Add(num4);
			}
			num += num4;
			num3 = num4;
			flag = false;
		}
		if (num2 != 0)
		{
			num /= (float)num2;
			base.screenEdgeConstraint.SetAngle(num);
		}
	}
}
