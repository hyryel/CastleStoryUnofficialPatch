using System;
using System.Collections.Generic;
using Brix.Common.Collections;
using Brix.Components;
using Brix.Engine;
using Brix.External.Factories;
using Brix.External.Signals;
using Brix.Game;
using Brix.Game.Components;
using Brix.Game.Components.Wards;
using Brix.Legacy;
using Brix.Utils.Unity;
using Smooth.Slinq;
using UnityEngine;

public class WardRadiusRenderer : BrixSingleton<WardRadiusRenderer>
{
	public class RendererEntry
	{
		public GameComponent Component;

		public WardController.WardHoverGroup Group;

		public Color Color;

		public float Range;

		public Faction Faction => Component.faction;

		public Vector3 Position => Component.transform.position;
	}

	public Material wardMat;

	public float cameraFadeoutDistanceMin = 50f;

	public float cameraFadeoutDistanceMax = 200f;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public Faction faction;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public int wardCount;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public int currentWardCount;

	private readonly Vector4[] _wardArray = new Vector4[128];

	private int _exI;

	private Color _wardColor;

	private readonly Dictionary<GameObject, RendererEntry> _entries = new Dictionary<GameObject, RendererEntry>();

	private readonly Dictionary<GameObject, RendererEntry> _temporaryEntries = new Dictionary<GameObject, RendererEntry>();

	private SignalConnection _addedWardObjectConnection = SignalConnection.Empty;

	private SignalConnection _removedWardObjectConnection = SignalConnection.Empty;

	private SignalConnection _addedCrystalConnection = SignalConnection.Empty;

	private SignalConnection _removedCrystalConnection = SignalConnection.Empty;

	private Material _camEffectMat;

	private CameraCustomImageEffect _camEffect;

	private int _HighlightCamera;

	private int _FadeoutDistanceMin;

	private int _FadeoutDistanceMax;

	private int _HighlightPosArray;

	private int _HighlightColor;

	protected override void Start()
	{
		base.Start();
		_HighlightCamera = Shader.PropertyToID("_HighlightCamera");
		_FadeoutDistanceMin = Shader.PropertyToID("_FadeoutDistanceMin");
		_FadeoutDistanceMax = Shader.PropertyToID("_FadeoutDistanceMax");
		_HighlightPosArray = Shader.PropertyToID("_HighlightPosArray");
		_HighlightColor = Shader.PropertyToID("_HighlightColor");
		_camEffect = Camera.main.gameObject.AddComponent<CameraCustomImageEffect>();
		_camEffect.imageEffectMat = (_camEffectMat = new Material(wardMat));
		_camEffect.autoDetectCameraSettings = true;
		GameSignals.LateConnectOnce(GameSignals.LevelReady, delegate
		{
			Neo.Relais(Init, 0.1f, "ward shader init");
		});
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		UnityEngine.Object.Destroy(_camEffect);
		UnityEngine.Object.Destroy(_camEffectMat);
		UnInit();
	}

	private void Update()
	{
		if (Architecte.commence)
		{
			UpdateVisionArray();
			UpdateProperties();
		}
	}

	private void Init()
	{
		faction = Faction.DefaultAllied;
		_addedWardObjectConnection = UIGameObserver.wards.Added.Connect(OnAddedWardObject);
		_removedWardObjectConnection = UIGameObserver.wards.Removed.Connect(OnRemovedWardObject);
		_addedCrystalConnection = UIGameObserver.crystals.EnterCreation.Connect(OnAddedCrystal);
		_removedCrystalConnection = UIGameObserver.crystals.ExitCreation.Connect(OnRemovedCrystal);
		UIGameObserver.wards.GetVisibles().ForEach(delegate(WardController c, WardRadiusRenderer self)
		{
			self.OnAddedWardObject(c);
		}, this);
		UIGameObserver.crystals.CheckCreation().ForEach(delegate(SpawnerCristal c, WardRadiusRenderer self)
		{
			self.OnAddedCrystal(c);
		}, this);
	}

	private void UnInit()
	{
		_addedWardObjectConnection.Disconnect();
		_removedWardObjectConnection.Disconnect();
		_addedCrystalConnection.Disconnect();
		_removedCrystalConnection.Disconnect();
	}

	public void AddTemporaryEntry(RendererEntry entry)
	{
		_temporaryEntries[entry.Component.gameObject] = entry;
	}

	public void RemoveTemporaryEntry(RendererEntry entry)
	{
		_temporaryEntries.Remove(entry.Component.gameObject);
	}

	private void OnAddedWardObject(WardController w)
	{
		if (w.HoverGroup != WardController.WardHoverGroup.None)
		{
			_entries[w.gameObject] = new RendererEntry
			{
				Component = w,
				Group = w.HoverGroup,
				Color = w.HoverDisplayColor,
				Range = w.EffectRange
			};
			wardCount = _entries.Count;
		}
	}

	private void OnRemovedWardObject(WardController w)
	{
		_entries.Remove(w.gameObject);
		wardCount = _entries.Count;
	}

	private void OnAddedCrystal(SpawnerCristal w)
	{
		_entries[w.gameObject] = new RendererEntry
		{
			Component = w.GetComponent<GameComponent>(),
			Group = WardController.WardHoverGroup.Pylons,
			Color = Factory.Peek(ObjetsDynamiques.PylonGround).GetComponent<WardController>().HoverDisplayColor,
			Range = 20f
		};
		wardCount = _entries.Count;
	}

	private void OnRemovedCrystal(SpawnerCristal w)
	{
		_entries.Remove(w.gameObject);
		wardCount = _entries.Count;
	}

	private void UpdateProperties()
	{
		bool flag = currentWardCount != 0;
		if (_camEffect.enabled != flag)
		{
			_camEffect.enabled = flag;
		}
		if (_camEffect.enabled)
		{
			_camEffectMat.SetVector(_HighlightCamera, Camera.main.transform.position);
			_camEffectMat.SetColor(_HighlightColor, _wardColor);
			_camEffectMat.SetVectorArray(_HighlightPosArray, _wardArray);
			_camEffectMat.SetFloat(_FadeoutDistanceMin, cameraFadeoutDistanceMin);
			_camEffectMat.SetFloat(_FadeoutDistanceMax, cameraFadeoutDistanceMax);
		}
	}

	private void UpdateVisionArray()
	{
		int num = PopulateVisionArray();
		if (_exI > num)
		{
			Array.Clear(_wardArray, num, _exI - num);
		}
		_exI = num;
		currentWardCount = num;
	}

	private int PopulateVisionArray()
	{
		WardController wardController = UIGameObserver.wards.GetHovered().Slinq().FirstOrDefault();
		WardController.WardHoverGroup wardHoverGroup;
		if (wardController != null)
		{
			RendererEntry valueOrDefault = _entries.GetValueOrDefault(wardController.gameObject);
			if (valueOrDefault == null)
			{
				return 0;
			}
			_wardColor = valueOrDefault.Color;
			wardHoverGroup = valueOrDefault.Group;
			if (wardHoverGroup == WardController.WardHoverGroup.Lanterns)
			{
				return 0;
			}
		}
		else if (WardLink.LinkVisibles)
		{
			_wardColor = Factory.Peek(ObjetsDynamiques.PylonGround).GetComponent<WardController>().HoverDisplayColor;
			wardHoverGroup = WardController.WardHoverGroup.Pylons;
		}
		else
		{
			if (_temporaryEntries.Count <= 0)
			{
				return 0;
			}
			RendererEntry value = _temporaryEntries.Slinq().First().Value;
			_wardColor = value.Color;
			wardHoverGroup = value.Group;
		}
		Vector3 position = Camera.main.transform.position;
		float num = cameraFadeoutDistanceMax * cameraFadeoutDistanceMax;
		int num2 = 0;
		foreach (KeyValuePair<GameObject, RendererEntry> item in _entries.Slinq().Concat(_temporaryEntries.Slinq()).NoLeak())
		{
			RendererEntry value2 = item.Value;
			if (value2.Component != null && value2.Range > 0f && value2.Group == wardHoverGroup && value2.Faction == faction && (position - value2.Position).sqrMagnitude < num)
			{
				Vector4 vector = value2.Position;
				vector.w = value2.Range;
				_wardArray[num2] = vector;
				num2++;
				if (num2 >= _wardArray.Length)
				{
					break;
				}
			}
		}
		return num2;
	}
}
