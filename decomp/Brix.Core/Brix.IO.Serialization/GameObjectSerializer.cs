using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Brix.Components;
using Brix.Utils.Logging;
using Newtonsoft.Json;
using UnityEngine;

namespace Brix.IO.Serialization;

public class GameObjectSerializer : BrixSingleton<GameObjectSerializer>
{
	private static BrixLogger logger = new BrixLogger();

	private readonly LinkedList<SerializationComponent> _saveQueue = new LinkedList<SerializationComponent>();

	public LinkedListNode<SerializationComponent> AddObjectToSaveQueue(SerializationComponent sc)
	{
		return _saveQueue.AddLast(sc);
	}

	public void RemoveFromSaveQueue(LinkedListNode<SerializationComponent> saveSlot)
	{
		_saveQueue.Remove(saveSlot);
	}

	internal void ForceAwake()
	{
		if (!base.Awoken)
		{
			Awake();
		}
	}

	public string GetJsonGameState(string target)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("[");
		JsonSerializer serializer = JsonSerializer.Create(Io.GetSaveSerializerSettings(target));
		StringWriter sw = new StringWriter(stringBuilder);
		foreach (SerializationComponent item in _saveQueue)
		{
			item.Save(sw, serializer);
		}
		stringBuilder.Remove(stringBuilder.Length - 1, 1);
		stringBuilder.Append("]");
		string text = stringBuilder.ToString();
		logger.Debug(text);
		return text;
	}

	public List<GameObject> LoadState(string target, string myJson)
	{
		return CreateGameObjects(target, myJson);
	}

	private List<GameObject> CreateGameObjects(string target, string json)
	{
		List<GameObject> result = null;
		try
		{
			result = JsonConvert.DeserializeObject(json, typeof(List<GameObject>), Io.GetLoadSerializerSettings(target)) as List<GameObject>;
		}
		catch (Exception message)
		{
			logger.Error(message);
		}
		return result;
	}
}
