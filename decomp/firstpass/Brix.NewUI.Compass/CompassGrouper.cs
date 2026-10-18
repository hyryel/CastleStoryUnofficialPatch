using System.Collections.Generic;
using Brix.External.Signals;
using Brix.Utils;
using Brix.Utils.Logging;
using Brix.Utils.Signals;
using UnityEngine;

namespace Brix.NewUI.Compass;

public class CompassGrouper : MonoBehaviour
{
	public enum Quadrant
	{
		Right,
		TopRight,
		Top,
		TopLeft,
		Left,
		BottomLeft,
		Bottom,
		BottomRight
	}

	private static BrixLogger logger = new BrixLogger(typeof(CompassGrouper).FullName);

	protected Signal Disconnected = new Signal();

	[SerializeField]
	protected Sprite m_Icon;

	[SerializeField]
	private CompassDispenser m_Dispenser;

	private GroupCompass[] m_GroupCompasses = new GroupCompass[8];

	private List<SoloCompass> m_SoloCompasses = new List<SoloCompass>();

	private readonly Dictionary<SoloCompass, SignalConnection> _afterPositionChangeConnections = new Dictionary<SoloCompass, SignalConnection>();

	public bool lateAssignement = true;

	private Rect[] m_QuadrantBounds = new Rect[8];

	public Sprite icon => m_Icon;

	public CompassDispenser dispenser => m_Dispenser;

	public GroupCompass[] groupCompasses => m_GroupCompasses;

	public List<SoloCompass> soloCompasses => m_SoloCompasses;

	private void CreateGroupCompasses()
	{
		for (int i = 0; i < 8; i++)
		{
			GroupCompass[] array = m_GroupCompasses;
			int num = i;
			CompassDispenser compassDispenser = dispenser;
			Sprite sprite = icon;
			string text = base.name;
			Quadrant quadrant = (Quadrant)i;
			array[num] = compassDispenser.GetOrCreateGroupCompass(sprite, text + " [" + quadrant.ToString() + "]");
			m_GroupCompasses[i].grouper = this;
		}
	}

	private void AutoAssign(SoloCompass soloCompass)
	{
		float angleTraverseInQuadrant;
		int quadrant = (int)GetQuadrant(soloCompass.screenEdgeConstraint.outAngle, out angleTraverseInQuadrant);
		GroupCompass groupCompass = groupCompasses[quadrant];
		groupCompass.Add(soloCompass);
	}

	private void AutoUnassign(SoloCompass soloCompass)
	{
		if (!(soloCompass.groupCompass == null))
		{
			soloCompass.groupCompass.Remove(soloCompass);
		}
	}

	private void UpdateGroupCompassPositions()
	{
		GroupCompass[] array = groupCompasses;
		foreach (GroupCompass groupCompass in array)
		{
			groupCompass.lateUpdatePos = true;
		}
	}

	public List<SoloCompass> Add(SoloCompass soloCompass)
	{
		if (!m_SoloCompasses.Contains(soloCompass))
		{
			soloCompasses.Add(soloCompass);
			OnAfterAdd(soloCompass);
		}
		return soloCompasses;
	}

	public List<SoloCompass> Remove(SoloCompass soloCompass)
	{
		if (m_SoloCompasses.Contains(soloCompass))
		{
			soloCompasses.Remove(soloCompass);
			OnAfterRemove(soloCompass);
		}
		return soloCompasses;
	}

	public List<SoloCompass> Clear()
	{
		soloCompasses.Clear();
		return soloCompasses;
	}

	private void OnAfterAdd(SoloCompass soloCompass)
	{
		AutoAssign(soloCompass);
		SignalConnection signalConnection = soloCompass.m_OnAfterPositionChange.Connect(OnAfterPositionChange);
		signalConnection.DisconnectWhen(Disconnected);
		_afterPositionChangeConnections.GetValueOrDefaultThenUpdate(soloCompass, signalConnection, SignalConnection.Empty).Disconnect();
	}

	private void OnAfterRemove(SoloCompass soloCompass)
	{
		_afterPositionChangeConnections.GetValueOrDefaultAndRemove(soloCompass, SignalConnection.Empty).Disconnect();
		AutoUnassign(soloCompass);
	}

	public void OnAfterPositionChange(SoloCompass soloCompass)
	{
		if (soloCompass == null || !m_SoloCompasses.Contains(soloCompass))
		{
			logger.Error("Illegal operation");
			return;
		}
		AutoAssign(soloCompass);
		if (soloCompass.groupCompass != null)
		{
			soloCompass.groupCompass.lateUpdatePos = true;
		}
	}

	public void OnAfterScreenDimensionsChange()
	{
		UpdateQuadrantBounds();
		UpdateGroupCompassPositions();
	}

	public GroupCompass Get(Quadrant quadrant)
	{
		return m_GroupCompasses[(int)quadrant];
	}

	public void Set(Quadrant quadrant, GroupCompass groupCompass)
	{
		m_GroupCompasses[(int)quadrant] = groupCompass;
	}

	private void UpdateQuadrantBounds()
	{
		float num = Screen.width;
		float num2 = Screen.height;
		float num3 = num / 3f;
		float num4 = num2 / 3f;
		for (int i = 0; i < 8; i++)
		{
			m_QuadrantBounds[i].width = num3;
			m_QuadrantBounds[i].height = num4;
		}
		float num5 = num3 * 2f;
		float num6 = num4 * 2f;
		float num7 = 0f;
		float num8 = 0f;
		num7 = num * -0.5f;
		num8 = num2 * -0.5f;
		num3 += num7;
		num4 += num8;
		num5 += num7;
		num6 += num8;
		m_QuadrantBounds[3].x = num7;
		m_QuadrantBounds[2].x = num3;
		m_QuadrantBounds[1].x = num5;
		m_QuadrantBounds[0].x = num5;
		m_QuadrantBounds[7].x = num5;
		m_QuadrantBounds[6].x = num3;
		m_QuadrantBounds[5].x = num7;
		m_QuadrantBounds[4].x = num7;
		m_QuadrantBounds[3].y = num6;
		m_QuadrantBounds[2].y = num6;
		m_QuadrantBounds[1].y = num6;
		m_QuadrantBounds[0].y = num4;
		m_QuadrantBounds[7].y = num8;
		m_QuadrantBounds[6].y = num8;
		m_QuadrantBounds[5].y = num8;
		m_QuadrantBounds[4].y = num4;
	}

	private Quadrant GetQuadrant(Vector3 position)
	{
		int result = 0;
		for (int i = 0; i < 8; i++)
		{
			if (!(position.x < m_QuadrantBounds[i].xMin) && !(position.x > m_QuadrantBounds[i].xMax) && !(position.y < m_QuadrantBounds[i].yMin) && !(position.y > m_QuadrantBounds[i].yMax))
			{
				result = i;
			}
		}
		return (Quadrant)result;
	}

	private Quadrant GetQuadrant(float angle, out float angleTraverseInQuadrant)
	{
		float num = 45f;
		float num2 = num / 2f;
		angle -= num2;
		angleTraverseInQuadrant = Mathf.Abs(angle % num - num2) / num2;
		int num3 = Mathf.RoundToInt(angle / num);
		return (num3 != 8) ? ((Quadrant)num3) : Quadrant.Right;
	}

	private void Start()
	{
		CreateGroupCompasses();
		UpdateQuadrantBounds();
	}

	private void OnDestroy()
	{
		Disconnected.Invoke();
	}
}
