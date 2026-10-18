using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Brix.Lua;
using Brix.Utils.Logging;
using MoonSharp.Interpreter;
using Smooth.Slinq;
using UnityEngine.Networking;

namespace Brix.Network;

public class UNetLuaMessageHandler : UNetAbstractMessageHandler
{
	public class LuaDataMessage : MessageBase
	{
		public byte[] _data;

		public LuaDataMessage()
		{
		}

		public LuaDataMessage(byte[] d)
		{
			_data = d;
		}

		public override void Serialize(NetworkWriter writer)
		{
			writer.WriteBytesFull(_data);
		}

		public override void Deserialize(NetworkReader reader)
		{
			_data = reader.ReadBytesAndSize();
		}
	}

	private static BrixLogger logger = new BrixLogger(typeof(UNetLuaMessageHandler).FullName);

	protected override void RegisterHandlers()
	{
		RegisterHandler(55, ReceivedLuaDataMessage);
	}

	public void BroadcastData(Table table)
	{
		if (NetworkServer.active)
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			MemoryStream memoryStream = new MemoryStream();
			binaryFormatter.Serialize(memoryStream, LuaLibs.SerialiseTable(table).ToArray());
			long length = memoryStream.Length;
			if (length <= 1024)
			{
				memoryStream.Position = 0L;
				byte[] array = new byte[length];
				memoryStream.Read(array, 0, (int)length);
				NetworkServer.SendToAll(55, new LuaDataMessage(array));
			}
			else
			{
				logger.ErrorFormat("Maximimum message size reached for lua network message. Maximum size is {0} bytes, message was {1} bytes.", 1024, length);
			}
		}
	}

	public override void Initialize()
	{
		RegisterHandlers();
		ConnectToSignal();
	}

	private void ConnectToSignal()
	{
	}

	private void ReceivedLuaDataMessage(NetworkMessage message)
	{
		if (!NetworkServer.active)
		{
			Stream stream = new MemoryStream();
			LuaDataMessage luaDataMessage = message.ReadMessage<LuaDataMessage>();
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			stream.Write(luaDataMessage._data, 0, luaDataMessage._data.Length);
			stream.Position = 0L;
			KeyValuePair<object, object>[] list = (KeyValuePair<object, object>[])binaryFormatter.Deserialize(stream);
			Dictionary<object, object> dictionary = new Dictionary<object, object>();
			list.Slinq().ForEach(delegate(KeyValuePair<object, object> kvp)
			{
				dictionary.Add(kvp.Key, kvp.Value);
			});
			Table a = LuaLibs.DeserialiseTable(dictionary, null);
			LuaRegistry.sig_serverMessage.Invoke(a);
		}
	}
}
