using System.Collections.Generic;
using Brix.Game.Components;
using Brix.Util;
using Brix.Utils;
using Brix.Utils.Collections;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Engine.Blocks.MeshSelector;

public class DefaultBlockMeshSelector : GameComponent
{
	[SerializeField]
	protected List<Mesh> _meshes = new List<Mesh>();

	[SerializeField]
	private Material[] _material;

	[SerializeField]
	public Vector3 OffSet;

	public Material[] Material => _material;

	public override void Initialize()
	{
		base.Initialize();
		OffsetMeshes();
	}

	private void OffsetMeshes()
	{
		_meshes.Slinq().ForEach(delegate(Mesh mesh, DefaultBlockMeshSelector self)
		{
			self.OffsetMesh(mesh);
		}, this);
	}

	private void OffsetMesh(Mesh mesh)
	{
		Vector3[] vertices = mesh.vertices;
		for (int i = 0; i < vertices.Length; i++)
		{
			vertices[i] += OffSet;
		}
		List<Vector3> list = vertices.Slinq().ToPooledList();
		mesh.SetVertices(list);
		list.Dispose();
	}

	public virtual Mesh MeshVariantFor(BlockData bd)
	{
		return MeshVariantFor(bd, GenerateDominantAxis(bd));
	}

	public virtual Mesh MeshVariantFor(BlockData bd, XYZ dominantAxis)
	{
		return MeshVariantFor(bd.Position, dominantAxis);
	}

	public virtual Mesh MeshVariantFor(XYZ position, XYZ dominantAxis)
	{
		return GetMaterialVariation(position, dominantAxis, _meshes);
	}

	protected virtual XYZ GetCanonicalAxis(XYZ dominantAxis)
	{
		return XYZ.Abs(dominantAxis);
	}

	protected Mesh GetMaterialVariation(XYZ p, XYZ dominantAxis, List<Mesh> meshes)
	{
		dominantAxis = GetCanonicalAxis(dominantAxis);
		Vector3 vector = dominantAxis.ToVector3();
		Vector3 vector2 = Calcule.PerpendicularVector(vector);
		Vector3 v = Vector3.Cross(vector, vector2);
		XYZ b = dominantAxis;
		XYZ b2 = XYZ.FromVector3(vector2);
		XYZ b3 = XYZ.FromVector3(v);
		int seed = Calcule.VecteurToFaceIndex(vector);
		int num = XYZ.Dot(p, b);
		int seed2 = XYZ.Dot(p, b2);
		int seed3 = XYZ.Dot(p, b3);
		int num2 = num;
		int num3 = PseudoRandom(seed2) + PseudoRandom(seed3);
		int num4 = PseudoRandom(seed);
		int num5 = UnsignedMod(num2 + num3 + num4, meshes.Count);
		return meshes[meshes.Count - num5 - 1];
	}

	private static int UnsignedMod(int a, int b)
	{
		return (a % b + b) % b;
	}

	private static XYZ GenerateDominantAxis(BlockData bd)
	{
		return XYZ.FromVector3(bd.Rotation * Vector3.forward);
	}

	private int PseudoRandom(int seed)
	{
		Random.InitState(seed);
		return Random.Range(0, 32768);
	}
}
