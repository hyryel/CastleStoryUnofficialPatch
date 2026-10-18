using System;
using System.Collections.Generic;
using Brix.Engine;
using UnityEngine;

namespace Brix.Game.Components.Tunnel;

public class TunnelUnit : MonoBehaviour
{
	public int reversed = 1;

	[SerializeField]
	public TunnelVolume tunnel;

	[SerializeField]
	private GameObject Cube;

	public TunnelDirection Left = new TunnelDirection();

	public TunnelDirection Forward = new TunnelDirection();

	public TunnelDirection Right = new TunnelDirection();

	public TunnelDirection Back = new TunnelDirection();

	public TunnelDirection[] Directions;

	public XYZ Position
	{
		get
		{
			return XYZ.FromVector3(base.transform.position);
		}
		set
		{
			base.transform.position = value.ToVector3();
		}
	}

	private void Awake()
	{
	}

	public IEnumerable<XYZ> GetVolume()
	{
		XYZ right = XYZ.FromVector3(base.transform.right);
		yield return XYZ.FromVector3(base.transform.position);
		yield return XYZ.FromVector3(base.transform.position) + right;
		yield return XYZ.FromVector3(base.transform.position) + XYZ.up;
		yield return XYZ.FromVector3(base.transform.position) + XYZ.up + right;
		yield return XYZ.FromVector3(base.transform.position) + XYZ.up * 2;
		yield return XYZ.FromVector3(base.transform.position) + XYZ.up * 2 + right;
		XYZ offset = XYZ.FromVector3(base.transform.forward);
		yield return XYZ.FromVector3(base.transform.position) + offset;
		yield return XYZ.FromVector3(base.transform.position) + offset + right;
		yield return XYZ.FromVector3(base.transform.position) + offset + XYZ.up;
		yield return XYZ.FromVector3(base.transform.position) + offset + XYZ.up + right;
		yield return XYZ.FromVector3(base.transform.position) + offset + XYZ.up * 2;
		yield return XYZ.FromVector3(base.transform.position) + offset + XYZ.up * 2 + right;
	}

	public void InitDirections()
	{
		XYZ xYZ = XYZ.FromVector3(base.transform.position);
		XYZ leadsTo = xYZ + XYZ.FromVector3(-base.transform.right) * 2;
		Left.Set(-base.transform.right, leadsTo);
		XYZ leadsTo2 = xYZ + XYZ.FromVector3(base.transform.forward) * 2;
		Forward.Set(base.transform.forward, leadsTo2);
		XYZ leadsTo3 = xYZ + XYZ.FromVector3(base.transform.right) * 2;
		Right.Set(base.transform.right, leadsTo3);
		XYZ leadsTo4 = xYZ + XYZ.FromVector3(-base.transform.forward) * 2;
		Back.Set(-base.transform.forward, leadsTo4);
		Directions = new TunnelDirection[4] { Left, Forward, Right, Back };
	}

	public TunnelDirection GetLocalDirection(Vector3 direction)
	{
		if (Directions == null)
		{
			InitDirections();
		}
		if (direction == -base.transform.right)
		{
			return Left;
		}
		if (direction == base.transform.forward)
		{
			return Forward;
		}
		if (direction == base.transform.right)
		{
			return Right;
		}
		if (direction == -base.transform.forward)
		{
			return Back;
		}
		throw new Exception("Invalid Direction Vector");
	}
}
