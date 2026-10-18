using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Brix.External.Factories;
using Brix.IO.Serialization.Json;
using Brix.IO.Serialization.Json.Converters;
using Brix.Network.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.IO.Serialization.Contracts;

public class DefaultGameContractResolver : DefaultContractResolver
{
	public delegate JsonContract ContractGenerator(Type objectType);

	protected readonly Dictionary<Type, JsonConverter> _converterDictionary;

	protected readonly Dictionary<Type, ContractGenerator> _objectContractDictionary;

	protected HashSet<Type> _serializableWithoutReferenceHandling;

	public string Target { get; set; }

	public DefaultGameContractResolver(string target)
	{
		Target = target;
		base.SerializeCompilerGeneratedMembers = true;
		_serializableWithoutReferenceHandling = new HashSet<Type>
		{
			typeof(TransformDTO),
			typeof(Vector3DTO),
			typeof(QuaternionDTO),
			typeof(Transform),
			typeof(Vector3),
			typeof(Quaternion)
		};
		_objectContractDictionary = new Dictionary<Type, ContractGenerator>
		{
			{
				typeof(GameObject),
				GenerateGameObjectContract
			},
			{
				typeof(Component),
				GenerateDefaultMonoBehaviourContract
			},
			{
				typeof(Transform),
				GenerateTransformContract
			},
			{
				typeof(Color),
				GenerateColorContract
			},
			{
				typeof(Vector3),
				GenerateVector3Contract
			},
			{
				typeof(Vector2),
				GenerateVector2Contract
			},
			{
				typeof(Quaternion),
				GenerateQuaternionContract
			}
		};
		_converterDictionary = new Dictionary<Type, JsonConverter>
		{
			{
				typeof(GameObject),
				new GameObjectConverter()
			},
			{
				typeof(Transform),
				new TransformConverter()
			},
			{
				typeof(Color),
				new ColorConverter()
			},
			{
				typeof(Rigidbody),
				new RigidbodyConverter()
			},
			{
				typeof(Component),
				new MonoBehaviourConverter()
			},
			{
				typeof(Factory.AssetKey),
				new AssetKeyConverter()
			},
			{
				typeof(MethodInfo),
				new MethodInfoConverter()
			},
			{
				typeof(SyncList<GameObject>),
				new SyncListConverter()
			},
			{
				RearViewMirror.GetType("System.Reflection.MonoMethod"),
				new MethodInfoConverter()
			}
		};
	}

	public void AddConverters(Dictionary<Type, JsonConverter> converters)
	{
		foreach (KeyValuePair<Type, JsonConverter> converter in converters)
		{
			_converterDictionary[converter.Key] = converter.Value;
		}
	}

	public void AddConverters(Type t, JsonConverter converter)
	{
		_converterDictionary[t] = converter;
	}

	public void AddCustomContractGenerators(Dictionary<Type, ContractGenerator> contractGenerators)
	{
		foreach (KeyValuePair<Type, ContractGenerator> contractGenerator in contractGenerators)
		{
			_objectContractDictionary[contractGenerator.Key] = contractGenerator.Value;
		}
	}

	public void AddCustomContractGenerator(Type t, ContractGenerator contractGenerator)
	{
		_objectContractDictionary[t] = contractGenerator;
	}

	protected override JsonContract CreateContract(Type objectType)
	{
		JsonContract contract = GetContract(objectType);
		if (_serializableWithoutReferenceHandling.Contains(objectType) || IsStruct(objectType))
		{
			contract.IsReference = false;
		}
		return contract;
	}

	private JsonContract GetContract(Type objectType)
	{
		Type type = objectType;
		while (type != typeof(UnityEngine.Object) && type != null)
		{
			if (_objectContractDictionary.ContainsKey(type))
			{
				return _objectContractDictionary[type](objectType);
			}
			type = type.BaseType;
		}
		return base.CreateContract(objectType);
	}

	protected override JsonConverter ResolveContractConverter(Type objectType)
	{
		Type type = objectType;
		while (type != typeof(UnityEngine.Object) && type != null)
		{
			if (_converterDictionary.ContainsKey(type))
			{
				return _converterDictionary[type];
			}
			type = type.BaseType;
		}
		return base.ResolveContractConverter(objectType);
	}

	protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
	{
		JsonProperty jsonProperty = base.CreateProperty(member, memberSerialization);
		List<SerializerTarget> source = member.GetCustomAttributes(inherit: true).OfType<SerializerTarget>().ToList();
		jsonProperty.Ignored = jsonProperty.Ignored || (source.Any() && !source.Any((SerializerTarget serializerTarget) => serializerTarget.IsTargetting(Target)));
		return jsonProperty;
	}

	protected override JsonDictionaryContract CreateDictionaryContract(Type objectType)
	{
		JsonDictionaryContract jsonDictionaryContract = base.CreateDictionaryContract(objectType);
		if (jsonDictionaryContract.DictionaryValueType == typeof(object))
		{
			jsonDictionaryContract.ItemConverter = new TypeIdentifierConverter();
		}
		return jsonDictionaryContract;
	}

	private JsonObjectContract GenerateDefaultMonoBehaviourContract(Type objectType)
	{
		JsonObjectContract jsonObjectContract = base.CreateObjectContract(objectType);
		jsonObjectContract.MemberSerialization = MemberSerialization.OptIn;
		jsonObjectContract.Properties.Clear();
		jsonObjectContract.IsReference = false;
		jsonObjectContract.ItemReferenceLoopHandling = ReferenceLoopHandling.Serialize;
		List<SerializerTarget> list = objectType.GetCustomAttributes(inherit: true).OfType<SerializerTarget>().ToList();
		if (list.Count == 0 || list.Any((SerializerTarget serializerTarget) => serializerTarget.IsTargetting(Target)))
		{
			foreach (JsonProperty item in CreateProperties(objectType, jsonObjectContract.MemberSerialization))
			{
				jsonObjectContract.Properties.AddProperty(item);
			}
		}
		return jsonObjectContract;
	}

	private JsonContract GenerateVector3Contract(Type vector3Type)
	{
		JsonObjectContract jsonObjectContract = CreateObjectContract(vector3Type);
		jsonObjectContract.Properties.Clear();
		jsonObjectContract.Properties.Add(CreateProperty(vector3Type.GetField("x"), MemberSerialization.OptOut));
		jsonObjectContract.Properties.Add(CreateProperty(vector3Type.GetField("y"), MemberSerialization.OptOut));
		jsonObjectContract.Properties.Add(CreateProperty(vector3Type.GetField("z"), MemberSerialization.OptOut));
		return jsonObjectContract;
	}

	private JsonContract GenerateQuaternionContract(Type quaternionType)
	{
		JsonObjectContract jsonObjectContract = CreateObjectContract(quaternionType);
		jsonObjectContract.Properties.Clear();
		jsonObjectContract.Properties.Add(CreateProperty(quaternionType.GetField("w"), MemberSerialization.OptOut));
		jsonObjectContract.Properties.Add(CreateProperty(quaternionType.GetField("x"), MemberSerialization.OptOut));
		jsonObjectContract.Properties.Add(CreateProperty(quaternionType.GetField("y"), MemberSerialization.OptOut));
		jsonObjectContract.Properties.Add(CreateProperty(quaternionType.GetField("z"), MemberSerialization.OptOut));
		return jsonObjectContract;
	}

	private JsonContract GenerateVector2Contract(Type vector2Type)
	{
		JsonObjectContract jsonObjectContract = CreateObjectContract(vector2Type);
		jsonObjectContract.Properties.Clear();
		jsonObjectContract.Properties.Add(CreateProperty(vector2Type.GetField("x"), MemberSerialization.OptOut));
		jsonObjectContract.Properties.Add(CreateProperty(vector2Type.GetField("y"), MemberSerialization.OptOut));
		return jsonObjectContract;
	}

	private JsonContract GenerateTransformContract(Type transformType)
	{
		JsonObjectContract jsonObjectContract = CreateObjectContract(transformType);
		jsonObjectContract.Properties.Clear();
		jsonObjectContract.Properties.Add(CreateProperty(transformType.GetProperty("position"), MemberSerialization.OptOut));
		jsonObjectContract.Properties.Add(CreateProperty(transformType.GetProperty("rotation"), MemberSerialization.OptOut));
		jsonObjectContract.Properties.Add(CreateProperty(transformType.GetProperty("localScale"), MemberSerialization.OptOut));
		return jsonObjectContract;
	}

	private JsonContract GenerateGameObjectContract(Type gameObjectType)
	{
		JsonObjectContract jsonObjectContract = CreateObjectContract(gameObjectType);
		jsonObjectContract.Properties.Clear();
		jsonObjectContract.Converter = new GameObjectConverter();
		return jsonObjectContract;
	}

	private JsonContract GenerateColorContract(Type colorType)
	{
		JsonObjectContract jsonObjectContract = CreateObjectContract(colorType);
		jsonObjectContract.Properties.Clear();
		jsonObjectContract.Properties.Add(CreateProperty(colorType.GetField("a"), MemberSerialization.OptOut));
		jsonObjectContract.Properties.Add(CreateProperty(colorType.GetField("b"), MemberSerialization.OptOut));
		jsonObjectContract.Properties.Add(CreateProperty(colorType.GetField("g"), MemberSerialization.OptOut));
		jsonObjectContract.Properties.Add(CreateProperty(colorType.GetField("r"), MemberSerialization.OptOut));
		return jsonObjectContract;
	}

	public static bool IsStruct(Type type)
	{
		return type.IsValueType && !type.IsEnum && !type.IsPrimitive;
	}
}
