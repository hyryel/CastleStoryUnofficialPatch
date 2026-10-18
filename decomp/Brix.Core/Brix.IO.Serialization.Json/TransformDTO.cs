using Brix.IO.Serialization.Json.Converters;
using Newtonsoft.Json;
using UnityEngine;

namespace Brix.IO.Serialization.Json;

[JsonObject(MemberSerialization.OptIn)]
public class TransformDTO
{
	[JsonProperty(IsReference = false)]
	public Vector3DTO position;

	[JsonProperty(IsReference = false)]
	public QuaternionDTO rotation;

	[JsonProperty(IsReference = false)]
	public Vector3DTO localScale;

	public TransformDTO()
	{
		position = Vector3DTO.DefaultPositionVector3DTO();
		rotation = QuaternionDTO.DefaultRotationQuaternionDTO();
		localScale = Vector3DTO.DefaultLocalScaleVector3DTO();
	}

	public TransformDTO(Transform transform)
	{
		position = transform.position;
		rotation = transform.rotation;
		localScale = transform.localScale;
	}

	public void InjectIn(Transform transform)
	{
		transform.position = position;
		transform.rotation = rotation;
		transform.localScale = localScale;
	}

	public bool IsDirty()
	{
		return !position.IsDefaultPosition() || !rotation.IsDefaultRotation() || !localScale.IsDefaultLocalScale();
	}

	public static implicit operator TransformDTO(Transform t)
	{
		return new TransformDTO(t);
	}
}
