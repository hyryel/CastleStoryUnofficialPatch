using System;
using System.Collections.Generic;
using System.IO;
using Brix.Common.Collections;
using Brix.Engine;
using Brix.Game;
using Brix.Game.AI;
using Brix.Game.Components;
using Brix.Input;
using Brix.Legacy;
using Brix.Lua;
using Brix.Utils;
using MoonSharp.Interpreter;
using Smooth.Slinq;
using Smooth.Slinq.Context;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Brix.UI;

public class MiniMap : UpdatableGameComponent<MiniMap>
{
	[Serializable]
	public class Highlight
	{
		public string name;

		public Transform displayParent;

		public Dictionary<string, Transform> displayChildren = new Dictionary<string, Transform>();

		public Dictionary<string, GameObject> originalObjects = new Dictionary<string, GameObject>();

		public GameObject prefab;
	}

	public class Trackable : IPoolable, IResetable, IDisposable
	{
		public Transform Transform;

		public Sprite Sprite;

		public Color Color;

		public Transform TargetTransform;

		public Image TargetImage;

		public float TargetSize;

		public void Dispose()
		{
		}

		public void ResetObject()
		{
			Transform = null;
			Sprite = null;
			Color = Color.white;
			TargetTransform = null;
		}
	}

	public bool showFogOfWar = true;

	public bool showUnits = true;

	public bool showCrystals = true;

	public bool showTerrain;

	public RawImage mapImage;

	public Highlight bricktronHt = new Highlight
	{
		name = "Bricktron Highlight"
	};

	public Highlight crystalHt = new Highlight
	{
		name = "Crystal Highlight"
	};

	public Highlight crystalHtAlt = new Highlight
	{
		name = "Crystal Highlight Shards"
	};

	public Highlight cameraHt = new Highlight
	{
		name = "Camera Highlight"
	};

	public Highlight cameraHtTarget = new Highlight
	{
		name = "Camera Highlight Target"
	};

	public GameObject TrackablePrefabGo;

	public Transform TrackableParentTrs;

	public Transform cam;

	public Transform camTarget;

	public string camName;

	public string camTargetName;

	public float mapPadding = 1f;

	public Material mapMat;

	public Material mapMatInstance;

	public Bounds mapBounds;

	public Vector2 size;

	public Texture2D heightmap;

	public Texture2D contourMap;

	public bool useContourMap = true;

	private int _HighlightPosArray;

	private int _ShowFogOfWar;

	private int _ShowTerrain;

	private int _ContourTex;

	private bool _dragAccepted;

	private static readonly List<Trackable> Trackables = new List<Trackable>();

	public static LuaObjectDescriptor<Trackable> TrackableDescriptor = new LuaObjectDescriptor<Trackable>();

	protected override float SlowUpdateRate => 1f;

	protected override void Start()
	{
		base.Start();
		_HighlightPosArray = Shader.PropertyToID("_HighlightPosArray");
		_ShowFogOfWar = Shader.PropertyToID("_ShowFogOfWar");
		_ShowTerrain = Shader.PropertyToID("_ShowTerrain");
		_ContourTex = Shader.PropertyToID("_ContourTex");
		CursorPicker.IsHoveringMinimap = false;
		GameSignals.DisableUntil(GameSignals.LevelReady, this);
		GameSignals.LateConnectOnce(GameSignals.LevelReady, Init);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (mapMatInstance.mainTexture == heightmap)
		{
			mapMatInstance.mainTexture = null;
		}
		if (mapImage.texture == heightmap)
		{
			mapImage.texture = null;
		}
		mapImage.material = mapMat;
		if (heightmap != null)
		{
			UnityEngine.Object.Destroy(heightmap);
			heightmap = null;
		}
		if (contourMap != null)
		{
			UnityEngine.Object.Destroy(contourMap);
			contourMap = null;
		}
		if (mapMatInstance != null)
		{
			UnityEngine.Object.Destroy(mapMatInstance);
			mapMatInstance = null;
		}
		ClearTrackables();
	}

	private void Init()
	{
		InitHeightmap();
		InitCameras();
	}

	protected override void BrixUpdate()
	{
		UpdateCameras();
		UpdateFogOfWar();
		UpdateCursor();
	}

	protected override void BrixSlowUpdate()
	{
		UpdateBricktrons();
		UpdateCrystals();
		UpdateTrackables();
	}

	private void InitHeightmap()
	{
		size = GetComponent<RectTransform>().rect.size;
		mapBounds = Voxel.Engine.ComputeOccupiedBounds();
		mapBounds.size = Vector3.one * (Mathf.Max(mapBounds.size.x, mapBounds.size.z) + mapPadding);
		Vector2 mainTextureScale = new Vector2(mapBounds.size.x, mapBounds.size.z) / 1024f;
		Vector2 mainTextureOffset = new Vector2(mapBounds.min.x, mapBounds.min.z) / 1024f;
		heightmap = new Texture2D(1024, 1024, TextureFormat.ARGB32, mipmap: true, linear: true);
		heightmap.filterMode = FilterMode.Point;
		heightmap.LoadImage(File.ReadAllBytes(System.IO.Path.Combine(GameParam.Map.path, "Heightmap.png")));
		heightmap.Apply();
		contourMap = new Texture2D((int)size.x, (int)size.y, TextureFormat.ARGB32, mipmap: true, linear: true);
		contourMap.filterMode = FilterMode.Point;
		BuildContourImage(heightmap, new XY((int)mapBounds.min.x, (int)mapBounds.min.z), new XY((int)mapBounds.size.x, (int)mapBounds.size.z), contourMap);
		contourMap.Apply();
		mapMat = mapImage.material;
		mapMatInstance = new Material(mapMat);
		mapMatInstance.mainTexture = heightmap;
		mapMatInstance.mainTextureScale = mainTextureScale;
		mapMatInstance.mainTextureOffset = mainTextureOffset;
		mapImage.material = mapMatInstance;
		mapImage.texture = heightmap;
		if (useContourMap)
		{
			mapMatInstance.SetTexture(_ContourTex, contourMap);
			mapMatInstance.SetTextureOffset(_ContourTex, Vector2.zero);
			mapMatInstance.SetTextureScale(_ContourTex, Vector2.one);
		}
	}

	private static void BuildContourImage(Texture2D original, XY start, XY size, Texture2D contours)
	{
		Color32[] pixels = original.GetPixels32();
		Color32[] array = new Color32[contours.width * contours.height];
		if (size.x >= contours.width)
		{
			for (int i = 0; i < pixels.Length; i++)
			{
				int num = i % original.width;
				int num2 = i / original.width;
				int num3 = Mathf.RoundToInt(((float)num + 0.5f - (float)start.x) * (float)contours.width / (float)size.x);
				int num4 = Mathf.RoundToInt(((float)num2 + 0.5f - (float)start.y) * (float)contours.height / (float)size.y);
				if (num3 >= 0 && num3 < contours.width && num4 >= 0 && num4 < contours.height)
				{
					Color32 p = pixels[i];
					Color32 a = ((num - 1 < 0) ? default(Color32) : pixels[i - 1]);
					Color32 b = ((num2 - 1 < 0) ? default(Color32) : pixels[i - original.width]);
					Color32 c = ((num + 1 >= original.width) ? default(Color32) : pixels[i + 1]);
					Color32 d = ((num2 + 1 >= original.height) ? default(Color32) : pixels[i + original.width]);
					int num5 = num3 + contours.width * num4;
					ref Color32 reference = ref array[num5];
					reference = MaxColor(array[num5], GetContour(p, a, b, c, d));
				}
			}
		}
		else
		{
			for (int j = 0; j < array.Length; j++)
			{
				int num6 = j % contours.width;
				int num7 = j / contours.width;
				int num8 = Mathf.RoundToInt(((float)num6 + 0.5f) * (float)size.x / (float)contours.width + (float)start.x);
				int num9 = Mathf.RoundToInt(((float)num7 + 0.5f) * (float)size.y / (float)contours.height + (float)start.y);
				if (num8 >= 0 && num8 < original.width && num9 >= 0 && num9 < original.height)
				{
					int num10 = num8 + original.width * num9;
					Color32 p2 = pixels[num10];
					Color32 a2 = ((num8 - 1 < 0) ? default(Color32) : pixels[num10 - 1]);
					Color32 b2 = ((num9 - 1 < 0) ? default(Color32) : pixels[num10 - original.width]);
					Color32 c2 = ((num8 + 1 >= original.width) ? default(Color32) : pixels[num10 + 1]);
					Color32 d2 = ((num9 + 1 >= original.height) ? default(Color32) : pixels[num10 + original.width]);
					ref Color32 reference2 = ref array[j];
					reference2 = MaxColor(array[j], GetContour(p2, a2, b2, c2, d2));
				}
			}
		}
		contours.SetPixels32(array);
	}

	private static Color32 GetContour(Color32 p, Color32 a, Color32 b, Color32 c, Color32 d)
	{
		if (p.r >= p.g)
		{
			return default(Color32);
		}
		if (a.r >= a.g || b.r >= b.g || c.r >= c.g || d.r >= d.g)
		{
			return new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
		}
		int num = p.g - 2;
		if (num >= a.g || num >= b.g || num >= c.g || num >= d.g)
		{
			return new Color32(0, byte.MaxValue, byte.MaxValue, 0);
		}
		num = p.g + 2;
		if (num <= a.g || num <= b.g || num <= c.g || num <= d.g)
		{
			return new Color32(0, byte.MaxValue, 0, byte.MaxValue);
		}
		return new Color32(0, byte.MaxValue, 0, 0);
	}

	private static Color32 MaxColor(Color32 a, Color32 b)
	{
		return new Color32((byte)Mathf.Max(a.r, b.r), (byte)Mathf.Max(a.g, b.g), (byte)Mathf.Max(a.b, b.b), (byte)Mathf.Max(a.a, b.a));
	}

	private void UpdateBricktrons()
	{
		if (bricktronHt.prefab == null)
		{
			return;
		}
		RefreshDisplayChildrenList(bricktronHt);
		bricktronHt.originalObjects.Clear();
		if (showUnits)
		{
			Slinq<Labor, ConcatContext<HashSetSlinqContext<Labor>, Labor, ConcatContext<HashSetSlinqContext<Labor>, Labor, HashSetSlinqContext<Labor>>>> slinq = UIGameObserver.bricktrons.CheckCreation().Slinq().Concat(UIGameObserver.alliedBricktrons.CheckCreation().Slinq())
				.Concat(UIGameObserver.enemyBricktrons.CheckCreation().Slinq());
			foreach (Labor item in slinq.NoLeak())
			{
				if (item.vision.IsVisibleForMe())
				{
					bricktronHt.originalObjects.Add(item.StringInstanceID, item.gameObject);
					Transform transform = RefreshOriginalObject(bricktronHt, item.StringInstanceID, item.gameObject, clampPosition: true);
					Image component = transform.GetComponent<Image>();
					if (component != null)
					{
						component.color = item.faction.FactionColor;
					}
				}
			}
		}
		CleanupOriginalObjects(bricktronHt);
	}

	private void UpdateCrystals()
	{
		if (crystalHt.prefab == null)
		{
			return;
		}
		RefreshDisplayChildrenList(crystalHt);
		RefreshDisplayChildrenList(crystalHtAlt);
		crystalHt.originalObjects.Clear();
		crystalHtAlt.originalObjects.Clear();
		if (showCrystals)
		{
			foreach (SpawnerCristal allCrystal in SpawnerCristal.AllCrystals)
			{
				FireflyNest fireflyNest = allCrystal.FireflyNest;
				if (fireflyNest.isHome)
				{
					crystalHt.originalObjects.Add(fireflyNest.Name, fireflyNest.gameObject);
				}
				else
				{
					crystalHtAlt.originalObjects.Add(fireflyNest.Name, fireflyNest.gameObject);
				}
				Transform transform = ((!fireflyNest.isHome) ? RefreshOriginalObject(crystalHtAlt, fireflyNest.Name, fireflyNest.gameObject, clampPosition: true) : RefreshOriginalObject(crystalHt, fireflyNest.Name, fireflyNest.gameObject, clampPosition: true));
				int childCount = transform.childCount;
				for (int i = 0; i < childCount; i++)
				{
					Transform child = transform.GetChild(i);
					Image component = child.GetComponent<Image>();
					if (component == null)
					{
						continue;
					}
					switch (i)
					{
					case 0:
						component.color = fireflyNest.faction.FactionColor;
						break;
					case 1:
					{
						CaptureClock component2 = fireflyNest.GetComponent<CaptureClock>();
						component.color = (component2.CurrentCapturers ?? component2.CaptureDefault).FactionColor;
						if (fireflyNest.isHome)
						{
							component.fillAmount = Mathf.Lerp(0.05f, 0.95f, component2.CaptureRatio);
						}
						else
						{
							component.fillAmount = Mathf.Lerp(0.085f, 0.91f, component2.CaptureRatio);
						}
						break;
					}
					case 2:
						component.color = fireflyNest.faction.FactionColor;
						if (!Affiliation.IsAllied(fireflyNest.faction))
						{
							component.fillAmount = 0.1f;
						}
						else if (fireflyNest.isHome)
						{
							component.fillAmount = Mathf.Lerp(0.1f, 0.86f, fireflyNest.crystalConversionProgress);
						}
						else
						{
							component.fillAmount = Mathf.Lerp(0.1f, 0.85f, fireflyNest.crystalConversionProgress);
						}
						break;
					}
				}
			}
		}
		CleanupOriginalObjects(crystalHt);
		CleanupOriginalObjects(crystalHtAlt);
	}

	private void InitCameras()
	{
		CameraScript component = Camera.main.GetComponent<CameraScript>();
		cam = component.transform;
		camTarget = component.GizmoTarget.transform;
		camName = cam.name;
		camTargetName = camTarget.name;
		RefreshDisplayChildrenList(cameraHt);
		RefreshDisplayChildrenList(cameraHtTarget);
		cameraHt.originalObjects.Clear();
		cameraHtTarget.originalObjects.Clear();
		cameraHt.originalObjects.Add(camName, cam.gameObject);
		cameraHtTarget.originalObjects.Add(camTargetName, camTarget.gameObject);
		UpdateCameras();
		CleanupOriginalObjects(cameraHt);
		CleanupOriginalObjects(cameraHtTarget);
	}

	private void UpdateCameras()
	{
		if (!(cameraHt.prefab == null))
		{
			Transform transform = RefreshOriginalObject(cameraHt, camName, cam.gameObject, clampPosition: false);
			RefreshOriginalObject(cameraHtTarget, camTargetName, camTarget.gameObject, clampPosition: true);
			Vector3 vector = camTarget.position - cam.position;
			vector = new Vector3(vector.x, vector.z, vector.y);
			if (vector.y >= 0f)
			{
				transform.localRotation = Quaternion.FromToRotation(Vector3.up, vector);
			}
			else
			{
				transform.localRotation = Quaternion.Euler(0f, 0f, 180f) * Quaternion.FromToRotation(Vector3.up, -vector);
			}
		}
	}

	private void UpdateFogOfWar()
	{
		if (FogOfWarRenderer.VisionArray != null)
		{
			mapMatInstance.SetVectorArray(_HighlightPosArray, FogOfWarRenderer.VisionArray);
		}
		mapMatInstance.SetFloat(_ShowFogOfWar, (showFogOfWar && !LocalFactionVision.AllVisible) ? 1 : 0);
		mapMatInstance.SetFloat(_ShowTerrain, showTerrain ? 1 : 0);
	}

	private void UpdateCursor()
	{
		if (CursorPicker.IsHoveringMinimap)
		{
			Picking.Instance.ForceUpdateActionModifiers();
		}
	}

	private void RefreshDisplayChildrenList(Highlight ht)
	{
		ht.displayChildren.Clear();
		int childCount = ht.displayParent.childCount;
		for (int i = 0; i < childCount; i++)
		{
			Transform child = ht.displayParent.GetChild(i);
			ht.displayChildren.Add(child.name, child);
		}
	}

	private Transform RefreshOriginalObject(Highlight ht, string name, GameObject go, bool clampPosition)
	{
		if (!ht.displayChildren.TryGetValue(name, out var value))
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(ht.prefab);
			gameObject.name = name;
			gameObject.transform.SetParent(ht.displayParent, worldPositionStays: false);
			value = gameObject.transform;
			ht.displayChildren.Add(name, gameObject.transform);
		}
		SetTargetPosition(go.transform, value.transform, clampPosition);
		return value;
	}

	private void SetTargetPosition(Transform trs, Transform targetTrs, bool clampPosition)
	{
		Vector3 position = trs.position;
		Vector2 vector = new Vector2(position.x, position.z);
		vector.x = (vector.x - mapBounds.min.x) / mapBounds.size.x;
		vector.y = (vector.y - mapBounds.min.z) / mapBounds.size.z;
		if (clampPosition)
		{
			vector = new Vector2(Mathf.Clamp01(vector.x), Mathf.Clamp01(vector.y));
		}
		else
		{
			bool flag = vector == new Vector2(Mathf.Clamp01(vector.x), Mathf.Clamp01(vector.y));
			if (targetTrs.gameObject.activeSelf != flag)
			{
				targetTrs.gameObject.SetActive(flag);
			}
		}
		targetTrs.localPosition = new Vector3(vector.x * size.x, vector.y * size.y, 0f);
	}

	private void CleanupOriginalObjects(Highlight ht)
	{
		foreach (KeyValuePair<string, Transform> displayChild in ht.displayChildren)
		{
			if (!ht.originalObjects.ContainsKey(displayChild.Key))
			{
				UnityEngine.Object.Destroy(displayChild.Value.gameObject);
			}
		}
	}

	public void OnPointerDown(BaseEventData eventData)
	{
		PointerEventData pointerEventData = eventData as PointerEventData;
		if (pointerEventData != null && pointerEventData.button == PointerEventData.InputButton.Left)
		{
			_dragAccepted = true;
			MoveAt(GetSurfaceAt(pointerEventData));
		}
		if (pointerEventData != null && pointerEventData.button == PointerEventData.InputButton.Right)
		{
			TerrainActionPicker.SimulateDirectOrder(GetSurfaceAt(pointerEventData));
		}
	}

	public void OnPointerUp(BaseEventData eventData)
	{
		_dragAccepted = false;
	}

	public void OnDrag(BaseEventData eventData)
	{
		if (eventData is PointerEventData data && _dragAccepted)
		{
			MoveAt(GetSurfaceAt(data));
		}
	}

	public void OnPointerEnter(BaseEventData eventData)
	{
		CursorPicker.IsHoveringMinimap = true;
	}

	public void OnPointerExit(BaseEventData eventData)
	{
		CursorPicker.IsHoveringMinimap = false;
	}

	private Vector3 GetSurfaceAt(PointerEventData data)
	{
		RectTransform component = GetComponent<RectTransform>();
		RectTransformUtility.ScreenPointToLocalPointInRectangle(component, data.position, data.pressEventCamera, out var localPoint);
		localPoint = new Vector2(localPoint.x / component.rect.size.x + 0.5f, localPoint.y / component.rect.size.y + 0.5f);
		localPoint = new Vector2(Mathf.Clamp01(localPoint.x), Mathf.Clamp01(localPoint.y));
		localPoint = new Vector2(localPoint.x * mapBounds.size.x + mapBounds.min.x, localPoint.y * mapBounds.size.z + mapBounds.min.z);
		Vector3 position = new Vector3(localPoint.x, CameraScript.Target.y, localPoint.y);
		return Architecte.SurfaceRaycast(position, 50);
	}

	private void MoveAt(Vector3 surface)
	{
		CameraScript component = Camera.main.GetComponent<CameraScript>();
		component.LookAt(surface);
	}

	public static Trackable AddTrackable(Transform trs, Sprite sprite, Color color)
	{
		Trackable trackable = ObjectPool<Trackable>.Reserve();
		trackable.Transform = trs;
		trackable.Sprite = sprite;
		trackable.Color = color;
		Trackables.Add(trackable);
		return trackable;
	}

	public static void RemoveTrackable(Trackable trackable)
	{
		if (Trackables.Contains(trackable))
		{
			if (trackable.TargetTransform != null)
			{
				UnityEngine.Object.Destroy(trackable.TargetTransform.gameObject);
			}
			Trackables.Remove(trackable);
			ObjectPool<Trackable>.Release(trackable);
		}
	}

	public static void ClearTrackables()
	{
		for (int num = Trackables.Count - 1; num >= 0; num--)
		{
			RemoveTrackable(Trackables[num]);
		}
	}

	private void UpdateTrackables()
	{
		foreach (Trackable trackable in Trackables)
		{
			if (trackable.TargetTransform == null)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(TrackablePrefabGo);
				gameObject.name = base.name;
				trackable.TargetTransform = gameObject.transform;
				trackable.TargetTransform.SetParent(TrackableParentTrs, worldPositionStays: false);
				trackable.TargetImage = gameObject.GetComponent<Image>();
				trackable.TargetImage.sprite = trackable.Sprite;
				trackable.TargetImage.color = trackable.Color;
			}
			SetTargetPosition(trackable.Transform, trackable.TargetTransform, clampPosition: true);
		}
	}

	private static DynValue lua_AddTrackable(ScriptExecutionContext executionContext, CallbackArguments args)
	{
		LuaDriver luaDriver = args.AsComponent<LuaDriver>(0, "AddTrackable");
		Sprite sprite = args.AsUserData<Sprite>(1, "AddTrackable");
		Color color = args.AsUserData<Color>(2, "AddTrackable");
		Trackable value = AddTrackable(luaDriver.transform, sprite, color);
		return DynValue.FromObject(executionContext.OwnerScript, value);
	}

	private static DynValue lua_RemoveTrackable(ScriptExecutionContext executionContext, CallbackArguments args)
	{
		Trackable trackable = args.AsUserData<Trackable>(0, "RemoveTrackable");
		RemoveTrackable(trackable);
		return DynValue.Void;
	}

	private static DynValue lua_SetTrackableColor(ScriptExecutionContext executionContext, CallbackArguments args)
	{
		Trackable trackable = args.AsUserData<Trackable>(0, "SetTrackableColor");
		Color color = args.AsUserData<Color>(1, "SetTrackableColor");
		trackable.Color = color;
		if (trackable.TargetImage != null)
		{
			trackable.TargetImage.color = trackable.Color;
		}
		return DynValue.Void;
	}

	private static DynValue lua_SetTrackableSprite(ScriptExecutionContext executionContext, CallbackArguments args)
	{
		Trackable trackable = args.AsUserData<Trackable>(0, "SetTrackableSprite");
		Sprite sprite = args.AsUserData<Sprite>(1, "SetTrackableSprite");
		trackable.Sprite = sprite;
		if (trackable.TargetImage != null)
		{
			trackable.TargetImage.sprite = trackable.Sprite;
		}
		return DynValue.Void;
	}

	private static DynValue lua_ClearTrackables(ScriptExecutionContext executionContext, CallbackArguments args)
	{
		ClearTrackables();
		return DynValue.Void;
	}

	public static void RegisterClass(Script script)
	{
		DynValue value = DynValue.NewTable(script);
		value.Table.Set("Trackable", TrackableDescriptor.GetStaticProxy());
		value.Table.Set("AddTrackable", DynValue.NewCallback((ScriptExecutionContext exec, CallbackArguments args) => lua_AddTrackable(exec, args)));
		value.Table.Set("RemoveTrackable", DynValue.NewCallback((ScriptExecutionContext exec, CallbackArguments args) => lua_RemoveTrackable(exec, args)));
		value.Table.Set("ClearTrackables", DynValue.NewCallback((ScriptExecutionContext exec, CallbackArguments args) => lua_ClearTrackables(exec, args)));
		value.Table.Set("SetTrackableColor", DynValue.NewCallback((ScriptExecutionContext exec, CallbackArguments args) => lua_SetTrackableColor(exec, args)));
		value.Table.Set("SetTrackableSprite", DynValue.NewCallback((ScriptExecutionContext exec, CallbackArguments args) => lua_SetTrackableSprite(exec, args)));
		script.Globals.Set("Minimap", value);
	}
}
