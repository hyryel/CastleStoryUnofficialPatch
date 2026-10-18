using System;
using System.Collections.Generic;
using Brix.Utils.Logging;
using UnityEngine;
using UnityEngine.Events;

public class InterpolatorTargetable : Interpolator
{
	[Serializable]
	public class State
	{
		public enum Group
		{
			_None,
			Alpha,
			Beta,
			Gamma
		}

		[Flags]
		public enum EventsUsed
		{
			OnEnter = 1,
			OnExit = 2
		}

		[Serializable]
		public class MenuEvent : UnityEvent
		{
		}

		[Serializable]
		public class BoolMenuEvent : UnityEvent<bool>
		{
		}

		[SerializeField]
		private string m_Name = string.Empty;

		[SerializeField]
		private bool m_Flag;

		[SerializeField]
		private Group m_Group;

		[SerializeField]
		private Color m_Color = Color.black;

		[SerializeField]
		private float m_Traverse;

		[SerializeField]
		private Vector2 m_TraverseRange = new Vector2(0f, 1f);

		[SerializeField]
		private bool m_IsEditTraverse;

		public static string[] eventsUsedLabels = new string[2] { "On Enter", "On Exit" };

		[HideInInspector]
		[SerializeField]
		private EventsUsed m_EventsUsed;

		[SerializeField]
		private MenuEvent m_OnEnter = new MenuEvent();

		[SerializeField]
		private MenuEvent m_OnExit = new MenuEvent();

		[SerializeField]
		private BoolMenuEvent m_OnEnterOrExit = new BoolMenuEvent();

		[SerializeField]
		private BoolMenuEvent m_OnExitOrEnter = new BoolMenuEvent();

		[SerializeField]
		private MenuEvent m_OnToggle = new MenuEvent();

		public string name
		{
			get
			{
				return m_Name;
			}
			set
			{
				m_Name = value;
			}
		}

		public bool flag
		{
			get
			{
				return m_Flag;
			}
			set
			{
				m_Flag = value;
			}
		}

		public Group group
		{
			get
			{
				return m_Group;
			}
			set
			{
				m_Group = value;
			}
		}

		public Color color
		{
			get
			{
				return m_Color;
			}
			set
			{
				m_Color = value;
			}
		}

		public float traverse
		{
			get
			{
				return ToUnitTraverse(m_Traverse);
			}
			set
			{
				m_Traverse = ToRangedTraverse(value);
			}
		}

		public float rawTraverse
		{
			get
			{
				return m_Traverse;
			}
			set
			{
				m_Traverse = value;
			}
		}

		public Vector2 traverseRange
		{
			get
			{
				return m_TraverseRange;
			}
			set
			{
				m_TraverseRange = value;
			}
		}

		public bool isEditTraverse
		{
			get
			{
				return m_IsEditTraverse;
			}
			set
			{
				m_IsEditTraverse = value;
			}
		}

		public virtual EventsUsed eventsUsed
		{
			get
			{
				return m_EventsUsed;
			}
			set
			{
				m_EventsUsed = value;
			}
		}

		public MenuEvent onEnter
		{
			get
			{
				return m_OnEnter;
			}
			set
			{
				m_OnEnter = value;
			}
		}

		public MenuEvent onExit
		{
			get
			{
				return m_OnExit;
			}
			set
			{
				m_OnExit = value;
			}
		}

		public BoolMenuEvent onEnterOrExit
		{
			get
			{
				return m_OnEnterOrExit;
			}
			set
			{
				m_OnEnterOrExit = value;
			}
		}

		public BoolMenuEvent onExitOrEnter
		{
			get
			{
				return m_OnExitOrEnter;
			}
			set
			{
				m_OnExitOrEnter = value;
			}
		}

		public MenuEvent onToggle
		{
			get
			{
				return m_OnToggle;
			}
			set
			{
				m_OnToggle = value;
			}
		}

		private float ToUnitTraverse(float rangedTraverse)
		{
			float num = (rangedTraverse - traverseRange.x) / (traverseRange.y - traverseRange.x);
			return Mathf.Round(num * 100f) / 100f;
		}

		private float ToRangedTraverse(float unitTraverse)
		{
			return Mathf.Lerp(m_TraverseRange.x, m_TraverseRange.y, unitTraverse);
		}

		public static bool IsEventUsed(EventsUsed eventsUsed, EventsUsed eventType)
		{
			return (eventsUsed & eventType) == eventType;
		}
	}

	private static BrixLogger logger = new BrixLogger(typeof(InterpolatorTargetable).FullName);

	[SerializeField]
	private float m_TargetTraverse;

	[SerializeField]
	private float m_PreviousTargetTraverse;

	private float previousVelocity;

	[SerializeField]
	private List<State> m_States = new List<State>();

	public UnityEvent[] m_OnEnters;

	public UnityEvent[] m_OnExits;

	public float targetTraverse
	{
		get
		{
			return m_TargetTraverse;
		}
		set
		{
			m_TargetTraverse = value;
		}
	}

	public float previousTargetTraverse
	{
		get
		{
			return m_PreviousTargetTraverse;
		}
		set
		{
			m_PreviousTargetTraverse = value;
		}
	}

	public List<State> states
	{
		get
		{
			return m_States;
		}
		set
		{
			m_States = value;
		}
	}

	public void UpdateStateTraverseRange()
	{
		Vector2 traverseRange = new Vector2(99f, -99f);
		foreach (State state in states)
		{
			state.traverseRange = new Vector2(0f, 1f);
			traverseRange.x = Mathf.Min(traverseRange.x, state.traverse);
			traverseRange.y = Mathf.Max(traverseRange.y, state.traverse);
		}
		foreach (State state2 in states)
		{
			state2.traverseRange = traverseRange;
		}
	}

	public override void OnEnable()
	{
		base.OnEnable();
		UpdateStateTraverseRange();
		ValidateStateNames();
	}

	private void ValidateStateNames()
	{
		State state = FindStateAtTraverse(base.traverse);
		if (state != null)
		{
			base.currentActiveStateName = state.name;
		}
		else
		{
			logger.Error("No state was found at traverse: " + base.traverse + "\n" + base.name);
		}
		State state2 = FindStateAtTraverse(targetTraverse);
		if (state2 != null)
		{
			base.currentTargetStateName = state2.name;
			return;
		}
		logger.Error("No state was found at target traverse: " + targetTraverse + "\n" + base.name);
	}

	private State FindLeftState(string stateName, float fromTraverse)
	{
		State result = null;
		for (int i = 0; i < states.Count; i++)
		{
			State state = states[i];
			if (state.traverse <= fromTraverse)
			{
				if (state.name == stateName)
				{
					result = state;
				}
				continue;
			}
			break;
		}
		return result;
	}

	private State FindLeftState(string stateName)
	{
		return FindLeftState(stateName, base.traverse);
	}

	private State FindRightState(string stateName)
	{
		State state = null;
		for (int num = states.Count - 1; num >= 0; num--)
		{
			State state2 = states[num];
			if (state2.traverse <= base.traverse)
			{
				break;
			}
			if (state2.name == stateName)
			{
				state = state2;
			}
		}
		if (state == null)
		{
			for (int i = 0; i < states.Count; i++)
			{
				State state3 = states[i];
				if (state3.traverse > base.traverse)
				{
					break;
				}
				if (state3.name == stateName)
				{
					state = state3;
					break;
				}
			}
		}
		return state;
	}

	public List<string> GetStateNames()
	{
		List<string> list = new List<string>();
		foreach (State state in states)
		{
			string item = state.name;
			if (!list.Contains(item))
			{
				list.Add(item);
			}
		}
		return list;
	}

	public List<float> GetStateTraverses()
	{
		List<float> list = new List<float>();
		foreach (State state in states)
		{
			float rawTraverse = state.rawTraverse;
			float num = (rawTraverse - state.traverseRange.x) / (state.traverseRange.y - state.traverseRange.x);
			num = Mathf.Round(num * 100f) / 100f;
			if (!list.Contains(num))
			{
				list.Add(num);
			}
		}
		return list;
	}

	public void PlayTo(string stateName)
	{
		if (base.subInterpolatorTargetable != null)
		{
			base.subInterpolatorTargetable.PlayTo(stateName);
		}
		if (!(stateName == string.Empty) && GetStateNames().Contains(stateName))
		{
			if (base.currentTargetStateName == string.Empty)
			{
				base.currentTargetStateName = stateName;
				UpdateTargetTraverse();
			}
			else if (base.isQueueTarget)
			{
				base.nextTargetStateNames.Add(stateName);
			}
		}
	}

	public void WarpTo(string stateName)
	{
		if (base.subInterpolatorTargetable != null)
		{
			base.subInterpolatorTargetable.WarpTo(stateName);
		}
		if (!(stateName == string.Empty) && GetStateNames().Contains(stateName))
		{
			base.currentTargetStateName = stateName;
			base.nextTargetStateNames.Clear();
			State state = FindRightState(stateName);
			float num = (targetTraverse = state.traverse);
			base.traverse = num;
		}
	}

	public State FindState(string stateName)
	{
		State state = null;
		float num = 0f;
		foreach (State state2 in states)
		{
			if (state2.name == stateName)
			{
				float num2 = state2.traverse - base.traverse;
				num2 = ((!(num2 < 0f)) ? num2 : 0f);
				if ((num2 != 0f || state == null) && (num2 > num || state == null))
				{
					state = state2;
					num = num2;
				}
			}
		}
		return state;
	}

	protected override void Play()
	{
		float num = base.traverse;
		if (base.traverse != targetTraverse)
		{
			float num2 = targetTraverse - base.traverse;
			float num3 = num2;
			float num4 = Mathf.Sign(num2);
			float num5 = num4;
			float num6 = previousVelocity;
			float num7 = num6;
			float num8 = Mathf.Abs(targetTraverse - previousTargetTraverse);
			float num9 = Mathf.Abs(targetTraverse - base.traverse);
			float time = num9 / num8;
			float num10 = base.deltaTraverseCurve.Evaluate(time);
			num10 *= 1f - base.deltaTraverseMin;
			num10 += base.deltaTraverseMin;
			num7 = base.deltaTraverseMax * num10;
			num += (previousVelocity = (base.deltaTraverse = num7 * (Time.unscaledDeltaTime / Time.fixedUnscaledDeltaTime))) * num4;
			num3 = targetTraverse - num;
			num5 = Mathf.Sign(num3);
			num = ((num5 == num4) ? num : targetTraverse);
		}
		base.traverse = num;
	}

	public State FindState(string stateName, float stateTraverse)
	{
		foreach (State state in states)
		{
			if (state.name == stateName && state.traverse == stateTraverse)
			{
				return state;
			}
		}
		return null;
	}

	protected override void OnAfterSetTraverse(float srcTraverse, float dstTraverse)
	{
		State state = FindState(base.currentActiveStateName, srcTraverse);
		State state2 = FindState(base.currentTargetStateName, dstTraverse);
		if (state != null)
		{
			State state3 = state;
			base.previousActiveStateName = state3.name;
			state3.onExit.Invoke();
			state3.onEnterOrExit.Invoke(arg0: false);
			state3.onExitOrEnter.Invoke(arg0: true);
		}
		if (state2 != null)
		{
			State state4 = state2;
			base.currentActiveStateName = state4.name;
			base.deltaTraverse = 0f;
			base.deltaDeltaTraverse = 0f;
			state4.onEnter.Invoke();
			state4.onEnterOrExit.Invoke(arg0: true);
			state4.onExitOrEnter.Invoke(arg0: false);
			Next();
		}
	}

	private void Next()
	{
		base.currentTargetStateName = string.Empty;
		if (base.isQueueTarget && base.nextTargetStateNames.Count != 0)
		{
			base.currentTargetStateName = base.nextTargetStateNames[0];
			base.nextTargetStateNames.RemoveAt(0);
		}
		UpdateTargetTraverse();
	}

	private void UpdateTargetTraverse()
	{
		if (base.currentTargetStateName == string.Empty || !GetStateNames().Contains(base.currentTargetStateName))
		{
			return;
		}
		State state = FindRightState(base.currentTargetStateName);
		if (state == null)
		{
			state = FindLeftState(base.currentTargetStateName);
			if (state == null)
			{
				logger.Error("Missing dstCurrentTargetState\ncurrentActiveStateName: " + base.currentActiveStateName + "\ncurrentTargetStateName: " + base.currentTargetStateName + "\n");
			}
		}
		State state2 = FindCurrentActiveState_Deprecated();
		if (state2 == null)
		{
			logger.Error("Missing srcCurrentActiveState\ncurrentActiveStateName: " + base.currentActiveStateName + "\ncurrentTargetStateName: " + base.currentTargetStateName + "\n");
		}
		State state3 = FindLeftState(state2.name, state.traverse);
		if (state3 == null)
		{
			logger.Error("Missing dstCurrentActiveState\ncurrentActiveStateName: " + base.currentActiveStateName + "\ncurrentTargetStateName: " + base.currentTargetStateName + "\n");
		}
		float num = base.traverse;
		num = state3?.traverse ?? num;
		targetTraverse = state.traverse;
		base.traverse = num;
	}

	private State FindStateAtTraverse(float t)
	{
		foreach (State state in states)
		{
			if (state.traverse == t)
			{
				return state;
			}
		}
		return null;
	}

	private State FindCurrentActiveState_Deprecated()
	{
		State result = null;
		foreach (State state in states)
		{
			if (base.currentActiveStateName == string.Empty)
			{
				return state;
			}
			if (state.name == base.currentActiveStateName && state.traverse == base.traverse)
			{
				result = state;
				break;
			}
		}
		return result;
	}

	private bool FindStateKit(int increment, string srcStateName, string dstStateName, ref State srcState, ref State dstState)
	{
		for (int i = 0; i < states.Count; i++)
		{
			int num = i + increment;
			if (num < states.Count)
			{
				State state = states[i];
				State state2 = states[num];
				if (!(state.traverse <= base.traverse) && state.name == srcStateName && state2.name == dstStateName)
				{
					srcState = state;
					dstState = state2;
					return true;
				}
			}
		}
		for (int j = 0; j < states.Count; j++)
		{
			int num2 = j + increment;
			if (num2 < states.Count)
			{
				State state3 = states[j];
				State state4 = states[num2];
				if (!(state3.traverse > base.traverse) && state3.name == srcStateName && state4.name == dstStateName)
				{
					srcState = state3;
					dstState = state4;
					return true;
				}
			}
		}
		return false;
	}

	private List<State> FindStates(string stateName)
	{
		List<State> list = new List<State>();
		foreach (State state in states)
		{
			if (state.name == stateName)
			{
				list.Add(state);
			}
		}
		return list;
	}
}
