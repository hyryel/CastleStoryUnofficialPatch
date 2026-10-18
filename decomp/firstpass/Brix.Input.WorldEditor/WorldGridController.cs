using Brix.Engine;
using Brix.External.Signals;
using Brix.Lua;
using Brix.Util;
using UnityEngine;

namespace Brix.Input.WorldEditor;

public class WorldGridController : MonoBehaviour
{
	public static readonly Vector3 BoundsBuffer = new Vector3(48f, 16f, 48f);

	public static readonly Vector3 MinBounds = Vector3.zero + BoundsBuffer;

	public static readonly Vector3 MaxBounds = new Vector3(1023f, 255f, 1023f) - BoundsBuffer;

	public Vector3 defaultGridPosition = new Vector3(512.5f, 0f, 512.5f);

	public Color DefaultBrushColor;

	public Color RemoverColor = new Color(1f, 0.3f, 0.1f);

	private Renderer _renderer;

	private bool _activeByUser = true;

	private bool _activeByBrush;

	private bool _activeByUndersideMenu;

	private bool _useHeightOverride;

	private int _heightOverride = 128;

	private Texture2D _customGuide;

	private float _opacityByUser = 1f;

	private const float _opacityByUndersideMenu = 1f;

	private const float _opacityByCreateIsland = 1f;

	public static Signal RefreshSignal = new Signal();

	public bool Active => _activeByUser || _activeByUndersideMenu || _activeByBrush;

	public bool ActiveByUser
	{
		get
		{
			return _activeByUser;
		}
		set
		{
			_activeByUser = value;
			UpdateOpacity();
			if (UIGameObserver.lua_OnSetGrid != null)
			{
				LuaHookBase.InvokeFromCLR(UIGameObserver.lua_OnSetGrid);
			}
			UpdateCameraMode();
		}
	}

	public bool ActiveByUndersideMenu
	{
		get
		{
			return _activeByUndersideMenu;
		}
		set
		{
			_activeByUndersideMenu = value;
			UpdateOpacity();
			RefreshSignal.Invoke();
			UpdateCameraMode();
		}
	}

	public bool ActiveByBrush
	{
		get
		{
			return _activeByBrush;
		}
		set
		{
			_activeByBrush = value;
			UpdateOpacity();
			RefreshSignal.Invoke();
			UpdateCameraMode();
		}
	}

	public bool UseHeightOverride
	{
		get
		{
			return _useHeightOverride;
		}
		set
		{
			_useHeightOverride = value;
			RefreshSignal.Invoke();
		}
	}

	public int HeightOverride
	{
		get
		{
			return _heightOverride;
		}
		set
		{
			_heightOverride = value;
			RefreshSignal.Invoke();
		}
	}

	public int HeightMin
	{
		get
		{
			Vector3 minBounds = MinBounds;
			return Mathf.RoundToInt(minBounds.y);
		}
	}

	public int HeightMax
	{
		get
		{
			Vector3 maxBounds = MaxBounds;
			return Mathf.RoundToInt(maxBounds.y);
		}
	}

	public int Height => (int)base.transform.position.y;

	public Vector3 BrushPosition
	{
		get
		{
			return _renderer.material.GetVector("_Point");
		}
		set
		{
			_renderer.material.SetVector("_Point", value);
		}
	}

	public float Size
	{
		get
		{
			return _renderer.material.GetFloat("_Radius") * 2f;
		}
		set
		{
			_renderer.material.SetFloat("_Radius", value / 2f);
		}
	}

	public float OpacityByUser
	{
		get
		{
			return _opacityByUser;
		}
		set
		{
			_opacityByUser = value;
			UpdateOpacity();
			RefreshSignal.Invoke();
		}
	}

	public Color BrushColor
	{
		get
		{
			return _renderer.material.GetColor("_Color2");
		}
		set
		{
			_renderer.material.SetColor("_Color2", value);
		}
	}

	private void UpdateOpacity()
	{
		float value = _opacityByUser;
		if (_activeByBrush && !_activeByUser)
		{
			value = 1f;
		}
		if (_activeByUndersideMenu)
		{
			value = 1f;
		}
		_renderer.material.SetFloat("_GridOpacity", value);
	}

	private void UpdateCameraMode()
	{
	}

	private void Start()
	{
		if (InputModeController.DefaultMode != InputMode.worldEditor)
		{
			_activeByUser = false;
		}
		_renderer = GetComponent<Renderer>();
		GameSignals.Connect(PickingObserver.lua_OnSetUserActivePicker, OverrideActive);
		DefaultBrushColor = BrushColor;
	}

	private void Update()
	{
		_renderer.enabled = Active;
		if (Active)
		{
			Vector3 position = defaultGridPosition;
			if (_useHeightOverride)
			{
				position.y = (float)_heightOverride + 0.5f;
			}
			else
			{
				position.y = Calcule.RoundVecteur(Camera.main.GetComponent<CameraScript>().GizmoTarget.transform.position).y - 0.5f;
			}
			position = GetClosestHeightInBounds(position);
			if (position != base.transform.position)
			{
				base.transform.position = position;
				RefreshSignal.Invoke();
			}
		}
	}

	public Vector3 GetClosestHeightInBounds(Vector3 position)
	{
		float y = position.y;
		Vector3 maxBounds = MaxBounds;
		if (y > maxBounds.y)
		{
			Vector3 maxBounds2 = MaxBounds;
			position.y = maxBounds2.y + 0.5f;
		}
		else
		{
			float y2 = position.y;
			Vector3 minBounds = MinBounds;
			if (y2 < minBounds.y)
			{
				Vector3 minBounds2 = MinBounds;
				position.y = minBounds2.y + 0.5f;
			}
		}
		return position;
	}

	public void SetGuide(Texture2D texture)
	{
		_customGuide = texture;
		_renderer.material.SetTexture("_Texture", texture);
	}

	public Texture2D GetGuide()
	{
		return _customGuide;
	}

	private void OverrideActive(Picker pOut, Picker pIn)
	{
		if (pIn != null && (pIn.mainComponent is CreateIslandBrush || pIn.mainComponent is TerrainHeightBrush))
		{
			ActiveByBrush = true;
			Size = pIn.SharedSettings.ConvertedSize;
		}
		else
		{
			ActiveByBrush = false;
			Size = 0f;
		}
	}
}
