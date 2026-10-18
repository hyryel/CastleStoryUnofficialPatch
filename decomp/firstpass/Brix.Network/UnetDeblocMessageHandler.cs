using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Brix.Engine;
using Brix.Game.Batchers;
using Brix.Utils.Logging;
using Smooth.Slinq;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Network;

public class UnetDeblocMessageHandler : UNetAbstractMessageHandler
{
	[Serializable]
	public struct DeblocData
	{
		public int index;

		public short id;

		public DeblocData(XYZ position, short id)
		{
			index = position.ToScalaire();
			this.id = id;
		}
	}

	public class DeblocDataMessage : MessageBase
	{
		public bool _endByte;

		public byte[] _data;

		public DeblocDataMessage()
		{
		}

		public DeblocDataMessage(byte[] data, bool endByte)
		{
			_data = data;
			_endByte = endByte;
		}

		public override void Serialize(NetworkWriter writer)
		{
			writer.Write(_endByte);
			writer.WriteBytesFull(_data);
		}

		public override void Deserialize(NetworkReader reader)
		{
			_endByte = reader.ReadBoolean();
			_data = reader.ReadBytesAndSize();
		}
	}

	private static BrixLogger logger = new BrixLogger(typeof(UnetDeblocMessageHandler).FullName);

	private List<DeblocData> DeblocDataListOutgoing = new List<DeblocData>();

	private List<DeblocData> DeblocDataListDelta = new List<DeblocData>();

	private List<DeblocData> DeblocDataListGameSession = new List<DeblocData>();

	private List<DeblocData> DeblocDataListIncoming = new List<DeblocData>();

	private Stream OutgoingdataStream = new MemoryStream();

	private Stream IncomingdataStream = new MemoryStream();

	public override void Initialize()
	{
		RegisterHandlers();
		OutgoingdataStream.Position = 0L;
		IncomingdataStream.Position = 0L;
		UNetManager.instance.StartCoroutine(ServerDeblocCoroutine());
	}

	private IEnumerator ServerDeblocCoroutine()
	{
		while (NetworkServer.active)
		{
			if (DeblocDataListDelta.Slinq().Any())
			{
				DeblocDataListOutgoing = new List<DeblocData>(DeblocDataListDelta);
				DeblocDataListDelta.Clear();
				BinaryFormatter bf = new BinaryFormatter();
				OutgoingdataStream = new MemoryStream();
				long savedPosition = OutgoingdataStream.Position;
				OutgoingdataStream.Position = 0L;
				bf.Serialize(OutgoingdataStream, DeblocDataListOutgoing.ToArray());
				OutgoingdataStream.Position = savedPosition;
				bool endByte;
				do
				{
					long ByteRemaining = OutgoingdataStream.Length - OutgoingdataStream.Position;
					if (ByteRemaining >= 1000)
					{
						ByteRemaining = 1000L;
					}
					byte[] buffer = new byte[ByteRemaining];
					OutgoingdataStream.Read(buffer, 0, (int)ByteRemaining);
					endByte = OutgoingdataStream.Position == OutgoingdataStream.Length;
					try
					{
						NetworkServer.SendToAll(52, new DeblocDataMessage(buffer, endByte));
					}
					catch (Exception ex)
					{
						logger.Debug(ex.Message);
					}
					yield return new WaitForSeconds(0.04f);
				}
				while (!endByte);
				DeblocDataListOutgoing.Clear();
			}
			yield return new WaitForSeconds(0.01f);
		}
	}

	private void ConnectToDeblocSignal()
	{
	}

	public void AddDeblocToSendQueue(XYZ position, int id)
	{
		DeblocDataListGameSession.Add(new DeblocData(position, (short)id));
		DeblocDataListDelta.Add(new DeblocData(position, (short)id));
	}

	protected override void RegisterHandlers()
	{
		RegisterHandler(51, ReceivedDeblocBatchBegin);
		RegisterHandler(52, ReceivedDeblocBatchData);
		RegisterHandler(53, ReceivedDeblocBatchEnd);
	}

	private void ReceivedDeblocBatchBegin(NetworkMessage message)
	{
	}

	private void ReceivedDeblocBatchData(NetworkMessage message)
	{
		DeblocDataMessage deblocDataMessage = message.ReadMessage<DeblocDataMessage>();
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		IncomingdataStream.Write(deblocDataMessage._data, 0, deblocDataMessage._data.Length);
		if (!deblocDataMessage._endByte)
		{
			return;
		}
		IncomingdataStream.Position = 0L;
		try
		{
			DeblocDataListIncoming = new List<DeblocData>((DeblocData[])binaryFormatter.Deserialize(IncomingdataStream));
			DeblocDataListIncoming.Slinq().ForEach(delegate(DeblocData debloc)
			{
				VoxelRequestBatcher.Instance.QueueDebloc(XYZ.FromScalaire(debloc.index), debloc.id);
			});
		}
		catch (Exception exception)
		{
			logger.Error(exception, "Could not deserialize debloc message from UNet");
		}
		finally
		{
			DeblocDataListIncoming.Clear();
			IncomingdataStream.SetLength(0L);
			IncomingdataStream.Position = 0L;
		}
	}

	private void ReceivedDeblocBatchEnd(NetworkMessage message)
	{
	}
}
