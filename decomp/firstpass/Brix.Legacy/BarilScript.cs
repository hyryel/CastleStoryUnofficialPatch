using Brix.Engine;
using Brix.Game.Components;
using Brix.Utils;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Legacy;

public class BarilScript : GameComponent, ISoulevable
{
	private GameObject debris;

	private CompCache<ExplodeOnDeath> _explodeOnDeath;

	protected override void Awake()
	{
		base.Awake();
		_explodeOnDeath = new CompCache<ExplodeOnDeath>(base.gameObject);
	}

	protected override void Start()
	{
		base.Start();
		Neo.ClasseMoi("ObjetsDynamiques", base.gameObject);
		_explodeOnDeath.Get.enabled = true;
	}

	protected void FixedUpdate()
	{
		Rigidbody component = GetComponent<Rigidbody>();
		if (NetworkServer.active && !Voxel.IsNonEmpty(base.voxel.position - XYZ.up) && component == null && GetComponent<CatapultAmmoComponent>() == null && Architecte.commence)
		{
			ChuteLibre();
		}
	}

	public void Soulever()
	{
		base.enabled = false;
	}

	public void Deposer()
	{
		base.enabled = true;
		Retablir();
		Object.Destroy(GetComponent<Rigidbody>());
		SurfaceSpawnFitter component = GetComponent<SurfaceSpawnFitter>();
		if ((bool)component)
		{
			component.Fit();
		}
	}

	public void Retablir()
	{
		if (!(GetComponent<CatapultAmmoComponent>() == null))
		{
			return;
		}
		base.voxel.position = XYZ.FromVector3(base.transform.position);
		if (!Voxel.IsOpenAndWalkable(base.voxel.position))
		{
			if (Voxel.IsOpenAndWalkable(base.voxel.position - XYZ.up))
			{
				base.voxel.position -= XYZ.up;
			}
			else if (Voxel.IsOpenAndWalkable(base.voxel.position - XYZ.up * 2))
			{
				base.voxel.position -= XYZ.up * 2;
			}
		}
		base.transform.position = Architecte.SurfaceRaycast(base.voxel.position.ToVector3()) + Vector3.up / 2f;
		ClasseMoi.Move(base.gameObject);
	}

	public void ChuteLibre()
	{
		base.gameObject.AddComponent<Rigidbody>();
	}
}
