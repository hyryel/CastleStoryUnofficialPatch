using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Brix.IO.Serialization.Json.Converters;

public class TransformConverter : BrixJsonConverter
{
	private Vector3 _defaultPositionVector3 = new Vector3(0f, 0f, 0f);

	private Vector3 _defaultLocalScaleVector3 = new Vector3(1f, 1f, 1f);

	private Quaternion _defaultRotationQuaternion = new Quaternion(0f, 0f, 0f, 1f);

	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(Transform);
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		Transform transform = value as Transform;
		if (IsDirty(transform))
		{
			writer.WriteStartObject();
			writer.WritePropertyName("$type");
			writer.WriteValue(SerializationUtils.GetNewtonsoftTypeName(typeof(Transform), serializer));
			if (transform.localPosition != _defaultPositionVector3)
			{
				writer.WritePropertyName("position");
				serializer.Serialize(writer, transform.position);
			}
			if (transform.localRotation != _defaultRotationQuaternion)
			{
				writer.WritePropertyName("rotation");
				serializer.Serialize(writer, transform.rotation);
			}
			if (transform.localScale != _defaultLocalScaleVector3)
			{
				writer.WritePropertyName("localScale");
				serializer.Serialize(writer, transform.localScale);
			}
			writer.WriteEndObject();
		}
	}

	private bool IsDirty(Transform t)
	{
		return t.localPosition != _defaultPositionVector3 || t.localRotation != _defaultRotationQuaternion || t.localScale != _defaultLocalScaleVector3;
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		serializer.Populate(reader, existingValue);
		return existingValue;
	}
}
