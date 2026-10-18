using System;
using System.Collections.Generic;
using Brix.Audio;
using Brix.External.Factories;
using Brix.External.Signals;
using Brix.Game.Components;
using Brix.Legacy;
using Brix.Lifecycle.Pooling;
using Brix.Utils.Unity;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Input;

public class DragHandle : GameComponent
{
	[Serializable]
	public class HandleDefaults
	{
		public HandleType type;

		public Sprite icon;

		public Color color;

		public CursorIcon hoverCursor;

		public CursorIcon dragCursor;

		public static HandleDefaults Null = new HandleDefaults
		{
			type = HandleType.None,
			icon = new Sprite(),
			color = Color.red,
			hoverCursor = CursorIcon.Idle,
			dragCursor = CursorIcon.Idle
		};

		public void ApplyTo(DragHandle handle)
		{
			handle.SetIcon(icon);
			handle.SetColor(color);
			handle.SetAvailableCursors(hoverCursor, dragCursor);
		}
	}

	[Serializable]
	public class Dependencies
	{
		public GameObject ball;

		public GameObject plane;

		public Animator animator;

		public LineRenderer line;

		public GameObject intersection;

		public SpriteRenderer icon;

		public LookAt LookAtComponent;

		public GameObject Circle;

		public SynchScale synchScale;

		public Material LineMaterial
		{
			set
			{
				line.sharedMaterial = value;
			}
		}

		public LookAt.Modes IconMode
		{
			get
			{
				return LookAtComponent ? LookAtComponent.Mode : LookAt.Modes.None;
			}
			set
			{
				if ((bool)LookAtComponent)
				{
					LookAtComponent.Mode = value;
				}
			}
		}

		public void Show(bool value)
		{
			if ((bool)ball)
			{
				ball.SetActive(value);
			}
			if ((bool)plane)
			{
				plane.SetActive(value);
			}
		}

		public void SetPlaneColor(Material mat)
		{
			if ((bool)plane)
			{
				plane.GetComponent<MeshRenderer>().material = mat;
			}
		}

		public bool Showing()
		{
			return ball.activeSelf || plane.activeSelf;
		}

		public void EnableCircle(bool value)
		{
			if ((bool)Circle)
			{
				Circle.SetActive(value);
			}
		}

		public void CircleLookAt(Vector3 direction)
		{
			if ((bool)Circle)
			{
				Circle.transform.LookAt(Circle.transform.position + direction * 9999f, ball.transform.right);
			}
		}

		public void SetLine(Vector3 start, Vector3 end)
		{
			if ((bool)line)
			{
				line.SetPosition(0, start);
				line.SetPosition(1, end);
				line.enabled = true;
			}
		}

		public void ResetLine()
		{
			if ((bool)line)
			{
				line.enabled = false;
			}
		}

		public void SetBallPosition(Vector3 position)
		{
			if ((bool)ball)
			{
				ball.transform.position = position;
			}
		}

		public void IntersectWith(Vector3 pos)
		{
			if ((bool)intersection)
			{
				intersection.transform.position = pos;
				intersection.SetActive(value: true);
			}
		}

		public void DontIntersect()
		{
			if ((bool)intersection)
			{
				intersection.SetActive(value: false);
			}
		}

		public void SetColor(Color color)
		{
			if ((bool)icon)
			{
				icon.color = color;
			}
		}

		public void SetIcon(Sprite sprite)
		{
			if ((bool)icon)
			{
				icon.sprite = sprite;
			}
		}

		public void IconLookAt(Vector3 direction)
		{
			if ((bool)icon)
			{
				icon.transform.LookAt(icon.transform.position + direction * 9999f, ball.transform.right);
			}
		}
	}

	public static class UISounds
	{
		public const string Side_Menu_Rollover = "Side_Menu_Rollover";

		public const string Side_Menu_Select = "Side_Menu_Select";

		public const string Side_Menu_Slide_In = "Side_Menu_Slide_In";

		public const string Side_Menu_Slide_Out = "Side_Menu_Slide_Out";

		public const string System_Menu_Rollover = "System_Menu_Rollover";

		public const string System_Menu_Select = "System_Menu_Select";

		public const string System_Menu_Slide_In = "System_Menu_Slide_In";

		public const string System_Menu_Slide_Out = "System_Menu_Slide_Out";
	}

	[Serializable]
	public class Spawner
	{
		[SerializeField]
		[DisplayRules(DisplayFlags.ReadOnly)]
		private DragHandle instance;

		[DisplayRules(DisplayFlags.HideAtRuntime)]
		[SerializeField]
		private Transform parent;

		[DisplayRules(DisplayFlags.HideAtRuntime)]
		[SerializeField]
		private string name;

		[DisplayRules(DisplayFlags.HideAtRuntime)]
		[SerializeField]
		private HandleType type;

		[DisplayRules(DisplayFlags.HideAtRuntime)]
		[SerializeField]
		private LookAt.Modes iconMode;

		[DisplayRules(DisplayFlags.HideAtRuntime)]
		[SerializeField]
		private VisibilityMode visibility;

		[SerializeField]
		public Faces Face;

		[DisplayRules(DisplayFlags.HideAtRuntime)]
		[SerializeField]
		private bool canGoBackward;

		[DisplayRules(DisplayFlags.HideAtRuntime)]
		[SerializeField]
		private bool enableOriginCircle = true;

		[DisplayRules(DisplayFlags.HideAtRuntime)]
		[SerializeField]
		private bool useCustomMaterial;

		[SerializeField]
		[DisplayRules(DisplayFlags.HideAtRuntime)]
		private Material lineMaterial;

		public DragHandle Instance => instance;

		public void Spawn()
		{
			if (type != HandleType.None)
			{
				ObjectPoolSingleton.RequestComponent<DragHandle>(out instance, VolumesUI.DragHandle);
				instance.transform.SetParent(parent);
				instance.transform.localPosition = Vector3.zero;
				instance.transform.localRotation = Quaternion.identity;
				instance.transform.localScale = Vector3.one;
				instance.name = name;
				instance.Visibility = visibility;
				instance.Type = type;
				instance.IconMode = iconMode;
				instance.canGoBackward = canGoBackward;
				instance.EnableOriginCircle(enableOriginCircle);
				instance.Face = Face;
				if (useCustomMaterial)
				{
					instance.SetLineMaterial(lineMaterial);
				}
			}
		}

		public void Release()
		{
			if (!Instance.IsNullOrReleased())
			{
				ObjectPoolSingleton.Release(Instance.gameObject);
			}
		}
	}

	private class State
	{
		public enum States
		{
			Hidden = -2,
			Collapsed,
			Normal,
			Expanded,
			Dragged
		}

		private States previousState;

		private Action refreshAction;

		public VisibilityMode Visibility;

		private CursorIcon hoverCursor;

		private CursorIcon dragCursor;

		private bool isDragging;

		private bool isAwake;

		private bool hovered;

		private States state
		{
			get
			{
				if (IsDragging)
				{
					return States.Dragged;
				}
				if (IsHovered)
				{
					return States.Expanded;
				}
				if (IsAwake || Visibility == VisibilityMode.Always)
				{
					return States.Normal;
				}
				if (Visibility == VisibilityMode.OnHover)
				{
					return States.Collapsed;
				}
				if (Visibility == VisibilityMode.HideInsteadOfCollapsing)
				{
					return States.Hidden;
				}
				throw new ArgumentOutOfRangeException();
			}
		}

		public bool IsDragging
		{
			get
			{
				return isDragging;
			}
			set
			{
				isDragging = value;
				RefreshIfNew();
			}
		}

		public bool IsAwake
		{
			get
			{
				return isAwake;
			}
			set
			{
				isAwake = value;
				if (!value)
				{
					hovered = false;
				}
				RefreshIfNew();
			}
		}

		public bool IsHovered
		{
			get
			{
				return hovered;
			}
			set
			{
				hovered = value;
				RefreshIfNew();
			}
		}

		public State(Action RefreshAction)
		{
			refreshAction = RefreshAction;
		}

		public void SetCursors(CursorIcon hover, CursorIcon drag)
		{
			hoverCursor = hover;
			dragCursor = drag;
		}

		public CursorIcon Cursor()
		{
			return state switch
			{
				States.Dragged => dragCursor, 
				States.Expanded => hoverCursor, 
				_ => CursorIcon.Idle, 
			};
		}

		public string Sound()
		{
			return state switch
			{
				States.Dragged => "Side_Menu_Select", 
				States.Expanded => "Side_Menu_Rollover", 
				States.Normal => "System_Menu_Rollover", 
				_ => string.Empty, 
			};
		}

		public void UpdateAnim(Animator animator)
		{
			if ((bool)animator && animator.isInitialized)
			{
				animator.SetInteger("State", (int)state);
			}
		}

		private void RefreshIfNew()
		{
			States states = state;
			if (previousState != states)
			{
				refreshAction();
				previousState = states;
			}
		}
	}

	public enum VisibilityMode
	{
		Always,
		OnHover,
		HideInsteadOfCollapsing
	}

	[DisplayRules(DisplayFlags.HideAtRuntime)]
	[SerializeField]
	private List<HandleDefaults> defaults = new List<HandleDefaults>();

	public static Signal<DragHandle> RequestedSignal = new Signal<DragHandle>();

	private Signal<DragHandle, ClickInfo> actionedSignal = new Signal<DragHandle, ClickInfo>();

	private EdgeSignal<DragHandle> dragSignals = new EdgeSignal<DragHandle>();

	private EdgeSignal<DragHandle> hoverSignals = new EdgeSignal<DragHandle>();

	[SerializeField]
	private Dependencies dependencies;

	private State state;

	private HandleBehaviour behaviour;

	private Vector3 MaxPoint;

	private Vector3 MinPoint;

	public bool canGoBackward = true;

	public Faces Face;

	public LookAt.Modes iconMode;

	[DisplayRules(DisplayFlags.ReadOnlyAtRuntime)]
	[SerializeField]
	private HandleType type = HandleType.Base;

	public VisibilityMode Visibility
	{
		set
		{
			state.Visibility = value;
		}
	}

	public HandleType Type
	{
		get
		{
			return type;
		}
		set
		{
			type = value;
			behaviour = HandleBehaviour.OfType(value);
			DefaultFor(behaviour).ApplyTo(this);
			behaviour.SetDefaultOrientation(this);
		}
	}

	public bool Active
	{
		get
		{
			return base.gameObject.activeSelf;
		}
		set
		{
			base.gameObject.SetActive(value);
		}
	}

	public bool Showing
	{
		get
		{
			return dependencies.Showing();
		}
		set
		{
			dependencies.Show(value);
			state.UpdateAnim(dependencies.animator);
		}
	}

	public Vector3 Origin
	{
		get
		{
			return base.transform.position;
		}
		set
		{
			base.transform.position = value;
		}
	}

	public Vector3 Destination { get; set; }

	public Vector3 Min => MinPoint;

	public Vector3 Max => MaxPoint;

	public SynchScale SynchScale => dependencies.synchScale;

	public LookAt.Modes IconMode
	{
		get
		{
			return dependencies.IconMode;
		}
		set
		{
			dependencies.IconMode = value;
		}
	}

	public IReadOnlySignal<DragHandle, ClickInfo> ActionedSignal => actionedSignal;

	public IReadOnlyEdgeSignal<DragHandle> DragSignals => dragSignals;

	public IReadOnlyEdgeSignal<DragHandle> HoverSignals => hoverSignals;

	public override void Initialize()
	{
		state = new State(UpdateAnimState);
	}

	public override void OnRequested()
	{
		base.OnRequested();
		Type = HandleType.Base;
		RequestedSignal.Invoke(this);
	}

	public override void ResetComponent()
	{
		base.ResetComponent();
		canGoBackward = true;
		Showing = true;
		hoverSignals.Clear();
		dragSignals.Clear();
	}

	private void OnEnable()
	{
		state.UpdateAnim(dependencies.animator);
	}

	private HandleDefaults DefaultFor(HandleBehaviour behaviour)
	{
		HandleDefaults handleDefaults = defaults.Slinq().Where((HandleDefaults d, HandleType t) => d.type == t, behaviour.Type).FirstOrDefault();
		if (handleDefaults != null)
		{
			return handleDefaults;
		}
		return HandleDefaults.Null;
	}

	private void UpdateAnimState()
	{
		if (Showing)
		{
			SoundEngine.Play(state.Sound());
		}
		state.UpdateAnim(dependencies.animator);
	}

	public void SetMinMax(Vector3 min, Vector3 max)
	{
		MinPoint = min;
		MaxPoint = max;
	}

	public void Towards(Vector3 direction)
	{
		base.transform.LookAt(direction + base.transform.position, Vector3.up);
		dependencies.CircleLookAt(Vector3.up);
	}

	public CursorIcon RecommendedCursor()
	{
		return state.Cursor();
	}

	public void SetAvailableCursors(CursorIcon hover, CursorIcon drag)
	{
		state.SetCursors(hover, drag);
	}

	public void SetLine(Vector3 start, Vector3 end)
	{
		dependencies.SetLine(start, end);
	}

	public void ResetLine()
	{
		dependencies.ResetLine();
	}

	public void SetLineMaterial(Material material)
	{
		dependencies.LineMaterial = material;
	}

	public void SetBallPosition(Vector3 position)
	{
		dependencies.SetBallPosition(position);
	}

	public void ResetBallPosition()
	{
		dependencies.SetBallPosition(base.transform.position);
	}

	public void IntersectWith(Vector3 pos)
	{
		dependencies.IntersectWith(pos);
	}

	public void DontIntersect()
	{
		dependencies.DontIntersect();
	}

	public void SetColor(Color color)
	{
		dependencies.SetColor(color);
	}

	public void SetPlaneColor(Material mat)
	{
		dependencies.SetPlaneColor(mat);
	}

	public void SetIcon(Sprite sprite)
	{
		dependencies.SetIcon(sprite);
	}

	public void EnableOriginCircle(bool value)
	{
		dependencies.EnableCircle(value);
	}

	public void WakeUp()
	{
		state.IsAwake = true;
	}

	public void Sleep()
	{
		state.IsAwake = false;
	}

	public void HoverStart()
	{
		behaviour.HoverStart(this);
		state.IsHovered = true;
		hoverSignals.start.Invoke(this);
	}

	public void HoverEnd()
	{
		behaviour.HoverEnd(this);
		state.IsHovered = false;
		if (!state.IsDragging)
		{
			hoverSignals.end.Invoke(this);
		}
	}

	public void Pressed(ClickInfo click)
	{
		behaviour.Pressed(this, click);
		actionedSignal.Invoke(this, click);
	}

	public void DragStarted()
	{
		behaviour.DragStarted(this);
		state.IsDragging = true;
		dragSignals.start.Invoke(this);
	}

	public void Drag(Vector3 dragOrigin, Vector3 rayCollisionPoint)
	{
		Destination = behaviour.GetDragDestination(this, dragOrigin, rayCollisionPoint);
		behaviour.Drag(this);
		state.IsDragging = true;
		dragSignals.on.Invoke(this);
	}

	public void DragEnded()
	{
		behaviour.DragEnded(this);
		state.IsDragging = false;
		dragSignals.end.Invoke(this);
		if (!state.IsHovered)
		{
			hoverSignals.end.Invoke(this);
		}
	}

	public Vector3 GetDragDestination(Vector3 dragOrigin, Vector3 rayCollisionPoint)
	{
		return behaviour.GetDragDestination(this, dragOrigin, rayCollisionPoint);
	}

	public Plane DragPlane()
	{
		return behaviour.DragPlane(this);
	}

	public void PickerBehaviour(HandleDragPicker handleDragPicker)
	{
		behaviour.PickerBehaviour(this, handleDragPicker);
	}
}
