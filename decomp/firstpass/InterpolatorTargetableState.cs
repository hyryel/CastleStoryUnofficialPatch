using UnityEngine;

[RequireComponent(typeof(InterpolatorTargetable))]
public class InterpolatorTargetableState : MonoBehaviour
{
	public enum Group
	{
		_None,
		Alpha,
		Beta,
		Gamma
	}

	[SerializeField]
	private string m_Label = string.Empty;

	[SerializeField]
	private bool m_Flag;

	[SerializeField]
	private Group m_Group;

	[SerializeField]
	private Color m_Color = Color.black;

	[SerializeField]
	private float m_Traverse;

	[SerializeField]
	private bool m_IsEditTraverse;

	[BoxBegin("Events")]
	[SerializeField]
	[Collapsable(true)]
	[Divider(4f)]
	private MenuEvent m_OnEnter = new MenuEvent();

	[SerializeField]
	private MenuEvent m_OnExit = new MenuEvent();

	[SerializeField]
	private BoolMenuEvent m_OnEnterOrExit = new BoolMenuEvent();

	[SerializeField]
	private BoolMenuEvent m_OnExitOrEnter = new BoolMenuEvent();

	[Collapsable(true)]
	[SerializeField]
	[BoxEnd(BoxEndAttribute.Order.After, BoxEndAttribute.Target.All)]
	private MenuEvent m_OnToggle = new MenuEvent();

	public string label
	{
		get
		{
			return m_Label;
		}
		set
		{
			m_Label = value;
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
			return m_Traverse;
		}
		set
		{
			m_Traverse = value;
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

	private void Start()
	{
	}

	private void Update()
	{
	}
}
