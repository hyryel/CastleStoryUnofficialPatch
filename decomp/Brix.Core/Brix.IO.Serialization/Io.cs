using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;
using Brix.Components;
using Brix.External.Factories;
using Brix.IO.Serialization.Contracts;
using Brix.IO.Serialization.Json.Converters;
using Brix.IO.Serialization.ReferenceResolvers;
using Brix.IO.Util;
using Brix.Utils;
using Newtonsoft.Json;
using UnityEngine;

namespace Brix.IO.Serialization;

public static class Io
{
	private static Dictionary<Type, JsonConverter> _converterDictionary = new Dictionary<Type, JsonConverter>();

	private static Dictionary<string, JsonSerializerSettings> LoadSerializerSettings = new Dictionary<string, JsonSerializerSettings>();

	private static Dictionary<string, JsonSerializerSettings> SaveSerializerSettings = new Dictionary<string, JsonSerializerSettings>();

	private static Dictionary<string, JsonSerializerSettings> ShallowSaveSerializerSettings = new Dictionary<string, JsonSerializerSettings>();

	private static Dictionary<string, JsonSerializerSettings> UnitySerializerSettings = new Dictionary<string, JsonSerializerSettings>();

	private static JsonSerializerSettings __defaultUnitySerializerSettings = GetUnitySerializerSettings("save");

	public static JsonSerializer GenericSerializer => JsonSerializer.Create(__defaultUnitySerializerSettings);

	public static JsonSerializerSettings GetLoadSerializerSettings(string target)
	{
		return LoadSerializerSettings.GetValueOrCreate(target, () => GenerateLoadSerializerSettings(target));
	}

	public static JsonSerializerSettings GenerateLoadSerializerSettings(string target)
	{
		JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
		jsonSerializerSettings.TypeNameHandling = TypeNameHandling.Objects;
		jsonSerializerSettings.TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple;
		jsonSerializerSettings.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
		jsonSerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
		jsonSerializerSettings.ReferenceResolverProvider = () => new GameObjectReferenceResolver(BrixSingleton<MainGameObjectRegistry>.Instance);
		jsonSerializerSettings.Error = JsonErrorHandler.LogErrorHandler;
		jsonSerializerSettings.ObjectCreationHandling = ObjectCreationHandling.Replace;
		jsonSerializerSettings.MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead;
		jsonSerializerSettings.ContractResolver = new DefaultGameContractResolver(target);
		(jsonSerializerSettings.ContractResolver as DefaultGameContractResolver).AddConverters(_converterDictionary);
		jsonSerializerSettings.Formatting = Formatting.Indented;
		return jsonSerializerSettings;
	}

	public static JsonSerializerSettings GetSaveSerializerSettings(string target)
	{
		return SaveSerializerSettings.GetValueOrCreate(target, () => GenerateSaveSerializerSettings(target));
	}

	public static JsonSerializerSettings GenerateSaveSerializerSettings(string target)
	{
		JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
		jsonSerializerSettings.TypeNameHandling = TypeNameHandling.Objects;
		jsonSerializerSettings.TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple;
		jsonSerializerSettings.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
		jsonSerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
		jsonSerializerSettings.ReferenceResolverProvider = () => new GameObjectReferenceResolver(new GameObjectRegistry());
		jsonSerializerSettings.Error = JsonErrorHandler.LogErrorHandler;
		jsonSerializerSettings.ObjectCreationHandling = ObjectCreationHandling.Replace;
		jsonSerializerSettings.ContractResolver = new DefaultGameContractResolver(target);
		(jsonSerializerSettings.ContractResolver as DefaultGameContractResolver).AddConverters(_converterDictionary);
		jsonSerializerSettings.Formatting = Formatting.Indented;
		return jsonSerializerSettings;
	}

	public static JsonSerializerSettings GetShallowSerializerSettings(string target, GameObject serializationScope)
	{
		return ShallowSaveSerializerSettings.GetValueOrCreate(target, () => GenerateShallowSerializerSettings(target, serializationScope));
	}

	public static JsonSerializerSettings GenerateShallowSerializerSettings(string target, GameObject serializationScope)
	{
		JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
		jsonSerializerSettings.TypeNameHandling = TypeNameHandling.Objects;
		jsonSerializerSettings.TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple;
		jsonSerializerSettings.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
		jsonSerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
		jsonSerializerSettings.ReferenceResolverProvider = () => new ShallowReferenceResolver(new GameObjectRegistry(), serializationScope);
		jsonSerializerSettings.Error = JsonErrorHandler.LogErrorHandler;
		jsonSerializerSettings.ObjectCreationHandling = ObjectCreationHandling.Replace;
		jsonSerializerSettings.ContractResolver = new DefaultGameContractResolver(target);
		(jsonSerializerSettings.ContractResolver as DefaultGameContractResolver).AddConverters(_converterDictionary);
		jsonSerializerSettings.Formatting = Formatting.Indented;
		return jsonSerializerSettings;
	}

	public static JsonSerializerSettings GetUnitySerializerSettings(string target)
	{
		return UnitySerializerSettings.GetValueOrCreate(target, () => GenerateUnitySerializerSettings(target));
	}

	public static JsonSerializerSettings GenerateUnitySerializerSettings(string target)
	{
		JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
		jsonSerializerSettings.TypeNameHandling = TypeNameHandling.Auto;
		jsonSerializerSettings.TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple;
		jsonSerializerSettings.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
		jsonSerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
		jsonSerializerSettings.Error = JsonErrorHandler.LogErrorHandler;
		jsonSerializerSettings.ObjectCreationHandling = ObjectCreationHandling.Auto;
		jsonSerializerSettings.DefaultValueHandling = DefaultValueHandling.Ignore;
		jsonSerializerSettings.NullValueHandling = NullValueHandling.Ignore;
		jsonSerializerSettings.MissingMemberHandling = MissingMemberHandling.Ignore;
		jsonSerializerSettings.ContractResolver = new DefaultGameContractResolver(target);
		jsonSerializerSettings.Formatting = Formatting.Indented;
		return jsonSerializerSettings;
	}

	public static JsonSerializerSettings GetTestSerializerSettings(string target = "save")
	{
		JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
		jsonSerializerSettings.TypeNameHandling = TypeNameHandling.Objects;
		jsonSerializerSettings.TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple;
		jsonSerializerSettings.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
		jsonSerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
		jsonSerializerSettings.ReferenceResolverProvider = () => new GameObjectReferenceResolver(new GameObjectRegistry());
		jsonSerializerSettings.Error = JsonErrorHandler.LogErrorHandler;
		jsonSerializerSettings.ObjectCreationHandling = ObjectCreationHandling.Replace;
		jsonSerializerSettings.ContractResolver = new DefaultGameContractResolver(target);
		jsonSerializerSettings.Formatting = Formatting.Indented;
		GameObjectConverter gameObjectConverter = new GameObjectConverter();
		gameObjectConverter.GameObjectFactory = new DefaultGameObjectFactory();
		(jsonSerializerSettings.ContractResolver as DefaultGameContractResolver).AddConverters(typeof(GameObject), gameObjectConverter);
		return jsonSerializerSettings;
	}

	public static JsonConverter InjectConverters(Type type, JsonConverter converter)
	{
		JsonConverter value = null;
		if (_converterDictionary.TryGetValue(type, out value))
		{
			_converterDictionary.Remove(type);
		}
		_converterDictionary.Add(type, converter);
		return value;
	}

	public static void ResetSettings()
	{
		_converterDictionary = new Dictionary<Type, JsonConverter>();
	}

	public static void PopulateGameObject(string json, GameObject o)
	{
		GameObjectPopulator.Populate(json, o);
	}
}
