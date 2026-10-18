using UnityEngine;

namespace Brix.Engine.Geometrie;

public interface ICubeMeshDescriptor
{
	Vector3[] SharedVertices { get; }

	Vector2[] SharedUvs { get; }

	Vector3[] SharedNormals { get; }

	Vector4[] SharedTangents { get; }

	Vector4[] SharedTangents2 { get; }

	int[] SharedTriangles { get; }

	int[] FaceTriangles { get; }

	int[] FaceVertexIndices { get; }

	int[] FaceEdgeIndices { get; }

	int[] VertexFaceIndices { get; }
}
