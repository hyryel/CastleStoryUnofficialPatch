using System;
using System.Collections.Generic;
using UnityEngine;

namespace Brix.Game.Components.Skybox;

public class Clouds : UpdatableGameComponent<Clouds>
{
	private SkyboxConfigurator _skyboxConfigurator;

	private List<Nuage> _cloudInstances = new List<Nuage>();

	public int _cloudQuantity;

	public Material cloudMaterial;

	public GameObject[] _cloudPrefabs;

	public Mesh[] _cloudMeshesHigh;

	public Mesh[] _cloudMeshesLow;

	[Tooltip("En mètres par seconde")]
	public Vector2 _wind;

	[Tooltip("En mètres par rapport")]
	public float _eloignementLOD;

	public float _cloudsNearAltitudeMin;

	public float _cloudsNearAltitudeMax;

	public float _cloudsHorizonAltitude;

	public AnimationCurve _cloudsScaleCurve;

	public float _cloudsNearScale;

	public float _cloudsHorizonScale;

	public float _cloudsHorizonDistance;

	public float _cameraPushAwaySpeed;

	public float _cameraPushAwayDistanceMin;

	public float _cameraPushAwayDistanceMax;

	protected override float SlowUpdateRate => 0.1f;

	private Vector3 CalculerCameraDelta()
	{
		return _skyboxConfigurator.GetMappedCamera().transform.position;
	}

	private float CalculerEloignement2(Vector2 xy, Vector3 camDelta)
	{
		return new Vector2(camDelta.x - xy.x, camDelta.z - xy.y).sqrMagnitude;
	}

	private float CalculerAltitudeApparente(float rapportAltitude, float rapportEloignement)
	{
		return Mathf.Lerp(Mathf.Lerp(_cloudsNearAltitudeMin, _cloudsNearAltitudeMax, rapportAltitude), _cloudsHorizonAltitude, rapportEloignement);
	}

	private float CalculerEchelleApparente(float rapportEchelle, float rapportEloignement)
	{
		return Mathf.Lerp(_cloudsNearScale, _cloudsHorizonScale, _cloudsScaleCurve.Evaluate(rapportEloignement)) * rapportEchelle;
	}

	protected override void Start()
	{
		base.Start();
		_skyboxConfigurator = base.transform.parent.GetComponent<SkyboxConfigurator>();
		if (_skyboxConfigurator == null)
		{
			throw new Exception("There are no SkyboxConfigurator Component attached to this GameObject: " + base.gameObject.name);
		}
		for (int i = 0; i < _cloudQuantity; i++)
		{
			int num = UnityEngine.Random.Range(0, 3);
			Vector2 aleasXY = new Vector2(1f - 2f * UnityEngine.Random.value, 1f - 2f * UnityEngine.Random.value);
			float value = UnityEngine.Random.value;
			float num2 = UnityEngine.Random.value * 360f;
			GameObject gameObject = UnityEngine.Object.Instantiate(_cloudPrefabs[num], Vector3.zero, Quaternion.Euler(-90f, 0f, num2));
			gameObject.transform.parent = base.transform;
			Nuage component = gameObject.GetComponent<Nuage>();
			component.Type = num;
			component.AleasXY = aleasXY;
			component.AleasAltitude = value;
			component.AleasRotation = num2;
			component.GetComponent<Renderer>().sharedMaterial = cloudMaterial;
			_cloudInstances.Add(component);
		}
	}

	protected override void BrixSlowUpdate()
	{
		float slowUpdateRate = SlowUpdateRate;
		float num = 1f / (_cloudsHorizonDistance * _cloudsHorizonDistance);
		float num2 = _eloignementLOD * _eloignementLOD * num;
		Vector2 ventLocal = _wind * slowUpdateRate / _cloudsHorizonDistance;
		float num3 = 1f / (float)_skyboxConfigurator._skyboxCameraScale;
		Vector3 camDelta = CalculerCameraDelta();
		Vector3 scaledCam = _skyboxConfigurator._skyboxCamera.transform.position * _skyboxConfigurator._skyboxCameraScale;
		foreach (Nuage cloudInstance in _cloudInstances)
		{
			if (!cloudInstance)
			{
				continue;
			}
			AppliquerVent(cloudInstance, ventLocal);
			AppliquerCameraRepoussement(cloudInstance, scaledCam, slowUpdateRate);
			AppliquerLoopage(cloudInstance);
			Vector2 xy = cloudInstance.AleasXY * _cloudsHorizonDistance;
			float num4 = 1f - Mathf.Exp(-2f * CalculerEloignement2(xy, camDelta) * num);
			num4 = Mathf.Min(1f, num4 / (1f - Mathf.Exp(-2f)));
			float aleasAltitude = cloudInstance.AleasAltitude;
			float rapportEchelle = 1f;
			float y = CalculerAltitudeApparente(aleasAltitude, num4);
			float num5 = CalculerEchelleApparente(rapportEchelle, num4);
			cloudInstance.transform.position = base.transform.position + new Vector3(xy.x, y, xy.y) * num3;
			cloudInstance.transform.localScale = Vector3.one * (num5 * num3);
			if (num4 < num2)
			{
				if (!cloudInstance.Res)
				{
					cloudInstance.GetComponent<MeshFilter>().mesh = _cloudMeshesHigh[cloudInstance.Type];
					cloudInstance.Res = true;
				}
			}
			else if (cloudInstance.Res)
			{
				cloudInstance.GetComponent<MeshFilter>().mesh = _cloudMeshesLow[cloudInstance.Type];
				cloudInstance.Res = false;
			}
		}
	}

	private void AppliquerVent(Nuage nuage, Vector2 ventLocal)
	{
		nuage.AleasXY += ventLocal;
	}

	private void AppliquerLoopage(Nuage nuage)
	{
		if (Mathf.Abs(nuage.AleasXY.x) > 1f)
		{
			nuage.AleasXY.x *= -0.95f;
		}
		if (Mathf.Abs(nuage.AleasXY.y) > 1f)
		{
			nuage.AleasXY.y *= -0.95f;
		}
	}

	private void AppliquerCameraRepoussement(Nuage nuage, Vector3 scaledCam, float deltaTime)
	{
		Vector2 vector = nuage.AleasXY * _cloudsHorizonDistance;
		Vector2 vector2 = new Vector2(vector.x - scaledCam.x, vector.y - scaledCam.z);
		float magnitude = vector2.magnitude;
		Vector2 vector3 = vector2 / magnitude * deltaTime * _cameraPushAwaySpeed;
		nuage.AleasXY += vector3 * (1f - Mathf.InverseLerp(_cameraPushAwayDistanceMin, _cameraPushAwayDistanceMax, magnitude));
	}
}
