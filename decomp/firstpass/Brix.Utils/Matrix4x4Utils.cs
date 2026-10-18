using UnityEngine;

namespace Brix.Utils;

public static class Matrix4x4Utils
{
	public static readonly Quaternion IdentityQuaternion = Quaternion.identity;

	public static readonly Matrix4x4 IdentityMatrix = Matrix4x4.identity;

	public static Vector3 ExtractTranslationFromMatrix(ref Matrix4x4 matrix)
	{
		Vector3 result = default(Vector3);
		result.x = matrix.m03;
		result.y = matrix.m13;
		result.z = matrix.m23;
		return result;
	}

	public static Quaternion ExtractRotationFromMatrix(ref Matrix4x4 matrix)
	{
		Vector3 forward = default(Vector3);
		forward.x = matrix.m02;
		forward.y = matrix.m12;
		forward.z = matrix.m22;
		Vector3 upwards = default(Vector3);
		upwards.x = matrix.m01;
		upwards.y = matrix.m11;
		upwards.z = matrix.m21;
		return Quaternion.LookRotation(forward, upwards);
	}

	public static Vector3 ExtractScaleFromMatrix(ref Matrix4x4 matrix)
	{
		Vector3 result = new Vector3(matrix.GetColumn(0).magnitude, matrix.GetColumn(1).magnitude, matrix.GetColumn(2).magnitude);
		if (Vector3.Cross(matrix.GetColumn(0), matrix.GetColumn(1)).normalized != (Vector3)matrix.GetColumn(2).normalized)
		{
			result.x *= -1f;
		}
		return result;
	}

	public static void DecomposeMatrix(ref Matrix4x4 matrix, out Vector3 localPosition, out Quaternion localRotation, out Vector3 localScale)
	{
		localPosition = ExtractTranslationFromMatrix(ref matrix);
		localRotation = ExtractRotationFromMatrix(ref matrix);
		localScale = ExtractScaleFromMatrix(ref matrix);
	}

	public static void SetTransformFromMatrix(Transform transform, ref Matrix4x4 matrix)
	{
		transform.localPosition = ExtractTranslationFromMatrix(ref matrix);
		transform.localRotation = ExtractRotationFromMatrix(ref matrix);
		transform.localScale = ExtractScaleFromMatrix(ref matrix);
	}

	public static Matrix4x4 TranslationMatrix(Vector3 offset)
	{
		Matrix4x4 identityMatrix = IdentityMatrix;
		identityMatrix.m03 = offset.x;
		identityMatrix.m13 = offset.y;
		identityMatrix.m23 = offset.z;
		return identityMatrix;
	}
}
