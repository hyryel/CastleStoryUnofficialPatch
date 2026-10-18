using System;
using System.Runtime.Serialization;
using Brix.Utils;
using UnityEngine;

namespace Brix.Core;

[Serializable]
public class TypeReference : ISerializable
{
	[AttributeUsage(AttributeTargets.Field)]
	public class ConfigAttribute : Attribute
	{
		public Type inheritsFromType;

		public bool allowAbstractClasses = false;

		public bool allowInterfaces = false;

		public Type defaultType = null;

		public Type[] excludedTypes = null;

		public TypeDropDownListingStyle dropDownStyle = TypeDropDownListingStyle.Namespace;

		public ConfigAttribute(Type inheritsFromType)
		{
			this.inheritsFromType = inheritsFromType;
		}

		public ConfigAttribute(Type inheritsFromType, params Type[] excludedTypes)
		{
			this.inheritsFromType = inheritsFromType;
			this.excludedTypes = excludedTypes;
		}
	}

	public enum TypeDropDownListingStyle
	{
		Namespace,
		Flat,
		ComponentMenu
	}

	[SerializeField]
	private string _typeHash;

	[NonSerialized]
	private Type _type;

	public Type Type
	{
		get
		{
			if (_type == null)
			{
				_type = UnHashType(_typeHash);
			}
			return _type;
		}
		set
		{
			_type = value;
			_typeHash = HashType(_type);
		}
	}

	public bool IsVoid => Type == typeof(void);

	public string Hash => _typeHash;

	public TypeReference()
	{
	}

	public TypeReference(Type tp)
	{
		Type = tp;
	}

	public TypeReference(string typeHash)
	{
		_typeHash = typeHash;
		Type = UnHashType(typeHash);
	}

	protected TypeReference(SerializationInfo info, StreamingContext context)
	{
		_typeHash = info.GetString("hash");
	}

	void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
	{
		_typeHash = HashType(_type);
		info.AddValue("hash", _typeHash);
	}

	public static implicit operator Type(TypeReference a)
	{
		return a?.Type;
	}

	public static string HashType(Type tp)
	{
		if (tp != null)
		{
			return TypeResolver.GetHash(tp);
		}
		return null;
	}

	public static Type UnHashType(string hash)
	{
		if (!string.IsNullOrEmpty(hash))
		{
			Type type = TypeResolver.GetType(hash);
			if (type == null)
			{
				type = typeof(void);
			}
			return type;
		}
		return null;
	}
}
