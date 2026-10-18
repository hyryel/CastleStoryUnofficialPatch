using System.Collections.Generic;
using System.IO;
using Brix.Components;
using Brix.Utils.Logging;
using Newtonsoft.Json;
using UnityEngine;

namespace Brix.IO.Serialization;

[JsonObject(MemberSerialization.OptIn)]
public class SerializationComponent : BrixComponent
{
	private static BrixLogger logger = new BrixLogger();

	private LinkedListNode<SerializationComponent> _saveSlot;

	public bool DoNotSerialize = false;

	public bool DoNotCreateOnDeserialized = false;

	public bool FactorylessGameObject = false;

	private string _gameObjectName = "";

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (string.IsNullOrEmpty(_gameObjectName))
		{
			_gameObjectName = base.name;
		}
	}

	public void Save(TextWriter sw, JsonSerializer serializer)
	{
		if (this == null)
		{
			logger.ErrorFormat("The GameObject {0} was asked to be serialized even though it was previously destroyed", _gameObjectName);
		}
		else if (!DoNotSerialize)
		{
			SerializeGameObject(sw, serializer);
			sw.Write(",");
		}
	}

	public string GetSerializedGameObject()
	{
		TextWriter textWriter = new StringWriter();
		SerializeGameObject(textWriter, JsonSerializer.Create(Io.GetSaveSerializerSettings("save")));
		return textWriter.ToString();
	}

	public string GetSerializedGameObject(JsonSerializerSettings settings)
	{
		TextWriter textWriter = new StringWriter();
		SerializeGameObject(textWriter, JsonSerializer.Create(settings));
		return textWriter.ToString();
	}

	public string GetShallowSerializedGameObject()
	{
		TextWriter textWriter = new StringWriter();
		SerializeGameObject(textWriter, JsonSerializer.Create(Io.GetShallowSerializerSettings("save", base.gameObject)));
		return textWriter.ToString();
	}

	public void SerializeGameObject(TextWriter writer, JsonSerializer serializer)
	{
		serializer.Serialize(writer, base.gameObject, typeof(GameObject));
	}

	public void Subscribe()
	{
		if (!DoNotSerialize && _saveSlot == null)
		{
			logger.DebugFormat("GameObject with ID#{0} is registering to the GameObjectSerializer.saveSignal", GetInstanceID());
			_saveSlot = BrixSingleton<GameObjectSerializer>.Instance.AddObjectToSaveQueue(this);
		}
	}

	public void Unsubscribe()
	{
		if (_saveSlot != null && BrixSingleton<GameObjectSerializer>.Instance != null)
		{
			BrixSingleton<GameObjectSerializer>.Instance.RemoveFromSaveQueue(_saveSlot);
			_saveSlot = null;
		}
	}

	public override void OnRequested()
	{
		base.OnRequested();
		_gameObjectName = base.name;
		Subscribe();
	}

	public override void ResetComponent()
	{
		base.ResetComponent();
		Unsubscribe();
	}
}
