using System;
using Brix.Engine;
using Brix.Game.AI;
using Brix.Game.Components.Volumes;
using Brix.Utils.Logging;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.Components.CatapultSpace;

public class Catapult : GameComponent
{
	private static BrixLogger logger = new BrixLogger(typeof(Catapult).FullName);

	public const int layerMask = 607232;

	[JsonProperty]
	public CatapultControl control;

	private CatapultBody _catapultBody { get; set; }

	private CatapultControl _catapultControl { get; set; }

	public Vector3 cible => control.cible;

	public Vector3 normale => control.cibleNormale;

	public GameObject boulet => control.boulet;

	protected override void Start()
	{
		base.Start();
		_catapultBody = GetComponent<CatapultBody>();
		_catapultControl = GetComponent<CatapultControl>();
		CatapultControl component = GetComponent<CatapultControl>();
		if (component != null && component.boulet != null)
		{
			_catapultBody.AjusterRotationModele();
			_catapultBody.AjusterCrinqueModele();
			_catapultBody.afficherCible = true;
			_catapultBody.afficherTrajets = true;
			_catapultBody.AjusterVoulu();
			_catapultBody.AjusterGizmos();
			SetTarget(_catapultControl.cible, _catapultControl.cibleNormale);
			_catapultControl.Apercu(_catapultControl.apercu, _catapultControl.apercuNormale);
			if (component.boulet.GetComponent<Rigidbody>() != null)
			{
				UnityEngine.Object.Destroy(component.boulet.GetComponent<Rigidbody>());
			}
		}
	}

	private void Update()
	{
		if (NetworkServer.active && !PeutDeclancher())
		{
			control.Networkfeu = false;
		}
		if (_catapultControl.boulet != null && _catapultControl.boulet.transform.parent != _catapultBody.modeleProjectile.transform)
		{
			_catapultControl.boulet.transform.parent = _catapultBody.modeleProjectile.transform;
		}
	}

	public void SetTarget(Vector3 target, Vector3 normal)
	{
		control.Networkcible = target;
		control.NetworkcibleNormale = normal;
		control.Networkfeu = false;
		control.body.HandleMessage(new CatapultState.MessageViser());
	}

	public void Armer(GameObject boulet)
	{
		control.Armer(boulet);
	}

	public void Desarmer()
	{
		control.Armer(null);
	}

	public void Piloter(GameObject ouvrier)
	{
		control.Piloter(ouvrier);
	}

	public void CesserDePiloter()
	{
		control.Piloter(null);
	}

	public bool PeutArmer()
	{
		return control.boulet == null && !control.animee;
	}

	public bool PeutDesarmer()
	{
		return control.boulet != null && !control.animee;
	}

	public bool PeutDeclancher()
	{
		return control.PeutDeclancher();
	}

	public bool PeutOperer(Labor labor)
	{
		return control.ouvrier == null || control.ouvrier == labor.gameObject;
	}

	public XYZ GetPositionArmement()
	{
		return GetPositionPilotage();
	}

	public XYZ GetPositionPilotage()
	{
		VolumePropertyRequest cases = base.volume.GetCases(VProperty.operation);
		XYZ xYZ;
		try
		{
			xYZ = cases.Slinq().First();
		}
		catch (Exception ex)
		{
			logger.Error("Catapult doesn't have proper operation square in Volume. Will use a default neighboring position. " + ex);
			xYZ = XYZ.FromVector3(base.transform.TransformPoint(-2f, 0f, 0f));
		}
		if (!Voxel.IsOpenAndWalkable(xYZ))
		{
			xYZ -= XYZ.up;
		}
		if (!Voxel.IsOpenAndWalkable(xYZ))
		{
			xYZ += XYZ.up * 2;
		}
		return xYZ;
	}

	public void Viser(Vector3 cible)
	{
		Viser(cible, Vector3.up);
	}

	public void Viser(Vector3 cible, Vector3 normale)
	{
		control.Apercu(cible, normale);
		control.Networkcible = cible;
		control.NetworkcibleNormale = normale;
	}

	public void ArreterDeViser()
	{
		control.CesserApercu();
	}

	public void Declancher()
	{
		if (PeutDeclancher())
		{
			control.Declancher();
		}
	}

	public void Deprogrammer()
	{
		control.Deprogrammer();
	}
}
