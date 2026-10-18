using System;
using Brix.Util;
using UnityEngine;

namespace Brix.Engine.Geometrie.SplitTriangles;

public class VoxelConfigurator : MonoBehaviour
{
	public XYZ position;

	private FaceConfigurator[] faces = new FaceConfigurator[7];

	private static string[] faceNames = new string[6] { "Top", "Forward", "Back", "Right", "Left", "Bottom" };

	private static string[] directions = new string[6] { "Up", "Forward", "Back", "Right", "Left", "Down" };

	private void Awake()
	{
	}

	public FaceConfigurator GetFace(int faceIndex)
	{
		if (faces[faceIndex] == null)
		{
			CreateFace(faceIndex);
		}
		return faces[faceIndex];
	}

	private void CreateFace(int k)
	{
		if (k < 6)
		{
			faces[k] = CreateFace(faceNames[k] + " (Config=" + (1 << k) + ")", CubeModele.Dirs[k] / 2f, Calcule.FaceIndexToRotationDeBase((byte)k), (VoxelizedMesh.FaceDirection)Enum.Parse(typeof(VoxelizedMesh.FaceDirection), directions[k]));
		}
		else
		{
			faces[6] = CreateFace("Other", Vector3.zero, Quaternion.identity, VoxelizedMesh.FaceDirection.None);
		}
	}

	private FaceConfigurator CreateFace(string name, Vector3 p, Quaternion q, VoxelizedMesh.FaceDirection direction)
	{
		FaceConfigurator faceConfigurator = new GameObject(name).AddComponent<FaceConfigurator>();
		faceConfigurator.transform.SetParent(base.transform);
		faceConfigurator.transform.localPosition = p;
		faceConfigurator.transform.localRotation = q;
		faceConfigurator.direction = direction;
		return faceConfigurator;
	}
}
