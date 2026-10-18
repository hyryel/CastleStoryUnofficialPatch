using System;
using System.Collections.Generic;
using Brix.Engine;
using Brix.External.Signals;
using Brix.Game;
using Brix.Game.Components;
using Brix.Utils.Unity;
using UnityEngine;

public class FogOfWarRenderer : MonoBehaviour
{
	public static float Opacity = 0.66f;

	public static Vector4[] VisionArray;

	public Material fogMat;

	public float cameraFadeoutDistanceMin = 50f;

	public float cameraFadeoutDistanceMax = 200f;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public Faction faction;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public int allVisionCount;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public int currentVisionCount;

	private const int NVisionArray = 128;

	private const int NDistanceClasses = 20;

	private readonly Vector4[] _visionArray = new Vector4[128];

	private readonly int[] _distanceClasses = new int[21];

	private int _exI;

	private readonly HashSet<LocalFactionVision> _allVisions = new HashSet<LocalFactionVision>();

	private SignalConnection _addedForObjectConnection = SignalConnection.Empty;

	private SignalConnection _removedForObjectConnection = SignalConnection.Empty;

	private SignalConnection _factionChangedConnection = SignalConnection.Empty;

	private Material _camEffectMat;

	private CameraCustomImageEffect _camEffect;

	private int _HighlightCamera;

	private int _HighlightOpacity;

	private int _HighlightPosArray;

	private int _FadeoutDistanceMin;

	private int _FadeoutDistanceMax;

	private void Start()
	{
		_HighlightCamera = Shader.PropertyToID("_HighlightCamera");
		_HighlightOpacity = Shader.PropertyToID("_HighlightOpacity");
		_HighlightPosArray = Shader.PropertyToID("_HighlightPosArray");
		_FadeoutDistanceMin = Shader.PropertyToID("_FadeoutDistanceMin");
		_FadeoutDistanceMax = Shader.PropertyToID("_FadeoutDistanceMax");
		_camEffect = Camera.main.gameObject.AddComponent<CameraCustomImageEffect>();
		_camEffect.imageEffectMat = (_camEffectMat = new Material(fogMat));
		_camEffect.autoDetectCameraSettings = true;
		VisionArray = _visionArray;
		GameSignals.LateConnectOnce(GameSignals.LevelReady, delegate
		{
			Neo.Relais(Init, 0.1f, "fog init");
		});
	}

	private void OnDestroy()
	{
		VisionArray = null;
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
		_addedForObjectConnection = GameSignals.Connect(LocalFactionVision.AddedFogObject, delegate(LocalFactionVision o, FogOfWarRenderer self)
		{
			self.OnAddedFogObject(o);
		}, this);
		_removedForObjectConnection = GameSignals.Connect(LocalFactionVision.RemovedFogObject, delegate(LocalFactionVision o, FogOfWarRenderer self)
		{
			self.OnRemovedFogObject(o);
		}, this);
		_factionChangedConnection = GameSignals.Connect(AffiliationComponent.AnyObjectChangedFaction, delegate(GameObject o, Faction a, Faction b, FogOfWarRenderer self)
		{
			self.OnFactionChanged(o);
		}, this);
		foreach (LocalFactionVision item in UIGameObserver.fogObjects.CheckCreated())
		{
			OnAddedFogObject(item);
		}
	}

	private void UnInit()
	{
		_addedForObjectConnection.Disconnect();
		_removedForObjectConnection.Disconnect();
		_factionChangedConnection.Disconnect();
	}

	private void OnAddedFogObject(LocalFactionVision v)
	{
		if (v.LineOfSightRange > 0 && faction.IsAlliedTo(v.gameObject))
		{
			_allVisions.Add(v);
			allVisionCount = _allVisions.Count;
		}
	}

	private void OnRemovedFogObject(LocalFactionVision v)
	{
		if (v.LineOfSightRange > 0 && faction.IsAlliedTo(v.gameObject))
		{
			_allVisions.Remove(v);
			allVisionCount = _allVisions.Count;
		}
	}

	private void OnFactionChanged(GameObject o)
	{
		LocalFactionVision component = o.GetComponent<LocalFactionVision>();
		if (component != null && component.LineOfSightRange > 0)
		{
			if (faction.IsAlliedTo(o))
			{
				_allVisions.Add(component);
			}
			else
			{
				_allVisions.Remove(component);
			}
			allVisionCount = _allVisions.Count;
		}
	}

	private void UpdateProperties()
	{
		bool flag = currentVisionCount != 0;
		bool flag2 = flag || _camEffect.enabled;
		if (_camEffect.enabled != flag)
		{
			_camEffect.enabled = flag;
		}
		if (flag2)
		{
			_camEffectMat.SetVector(_HighlightCamera, Camera.main.transform.position);
			_camEffectMat.SetFloat(_HighlightOpacity, Mathf.Lerp(0.1f, 1f, Opacity));
			_camEffectMat.SetVectorArray(_HighlightPosArray, _visionArray);
			_camEffectMat.SetFloat(_FadeoutDistanceMin, cameraFadeoutDistanceMin);
			_camEffectMat.SetFloat(_FadeoutDistanceMax, cameraFadeoutDistanceMax);
		}
	}

	private void UpdateVisionArray()
	{
		int num = PopulateVisionArray();
		if (_exI > num)
		{
			Array.Clear(_visionArray, num, _exI - num);
		}
		_exI = num;
		currentVisionCount = num;
	}

	private int PopulateVisionArray()
	{
		if (LocalFactionVision.AllVisible)
		{
			return 0;
		}
		Vector3 position = Camera.main.transform.position;
		int num = 0;
		int num2 = 0;
		for (int i = 0; i <= 20; i++)
		{
			_distanceClasses[i] = 0;
		}
		foreach (LocalFactionVision allVision in _allVisions)
		{
			if (!(allVision == null) && allVision.LineOfSightRange > 0)
			{
				float num3 = (position - allVision.transform.position).magnitude - (float)allVision.LineOfSightRange;
				if (num3 >= cameraFadeoutDistanceMax)
				{
					num3 = cameraFadeoutDistanceMax + 0.01f;
				}
				int num4 = Mathf.FloorToInt(Mathf.Max(num3, 0f) * 20f / cameraFadeoutDistanceMax);
				_distanceClasses[num4]++;
				num2++;
				if (num < 128)
				{
					Vector4 vector = allVision.transform.position;
					vector.w = allVision.LineOfSightRange;
					_visionArray[num] = vector;
					num++;
				}
			}
		}
		if (num2 > 128)
		{
			int num5 = 128;
			int j;
			for (j = 0; j <= 20; j++)
			{
				int num6 = _distanceClasses[j];
				if (num6 > num5)
				{
					break;
				}
				num5 -= num6;
			}
			num = 0;
			foreach (LocalFactionVision allVision2 in _allVisions)
			{
				if (allVision2 == null || allVision2.LineOfSightRange <= 0)
				{
					continue;
				}
				float num7 = (position - allVision2.transform.position).magnitude - (float)allVision2.LineOfSightRange;
				if (num7 >= cameraFadeoutDistanceMax)
				{
					num7 = cameraFadeoutDistanceMax + 0.01f;
				}
				int num8 = Mathf.FloorToInt(Mathf.Max(num7, 0f) * 20f / cameraFadeoutDistanceMax);
				if (num8 <= j && (num8 != j || num5-- > 0))
				{
					Vector4 vector2 = allVision2.transform.position;
					vector2.w = allVision2.LineOfSightRange;
					_visionArray[num] = vector2;
					num++;
					if (num >= 128)
					{
						break;
					}
				}
			}
		}
		return num;
	}
}
