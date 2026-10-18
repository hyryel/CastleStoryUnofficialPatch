using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Brix.Engine;
using Brix.External.Signals;
using Brix.Utils.Logging;
using Smooth.Slinq;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Network;

public class UNetFileTransferMessageHandler : UNetAbstractMessageHandler
{
	public class FileData
	{
		public FileStream FileStream;

		public string FilePath;

		public bool UploadStarted;

		public FileData(FileStream fs, string fp)
		{
			FileStream = fs;
			FilePath = fp;
			UploadStarted = false;
		}
	}

	public class FileDataMessageStart : MessageBase
	{
		public short type;

		public short hash;

		public string name;

		public bool compressed;

		public long modifiedTime;

		public override void Serialize(NetworkWriter writer)
		{
			writer.WritePackedUInt32((uint)type);
			writer.WritePackedUInt32((uint)hash);
			writer.Write(name);
			writer.Write(compressed);
			writer.WritePackedUInt64((ulong)modifiedTime);
		}

		public override void Deserialize(NetworkReader reader)
		{
			type = (short)reader.ReadPackedUInt32();
			hash = (short)reader.ReadPackedUInt32();
			name = reader.ReadString();
			compressed = reader.ReadBoolean();
			modifiedTime = (long)reader.ReadPackedUInt64();
		}
	}

	public class FileDataMessage : MessageBase
	{
		public byte[] _data;

		public FileDataMessage()
		{
		}

		public FileDataMessage(byte[] data)
		{
			_data = data;
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

	public class FileDataCompleteMessage : MessageBase
	{
	}

	public class NetworkFileTransferReady : MessageBase
	{
	}

	public class AbortDownloadMessage : MessageBase
	{
	}

	private struct fileInfo
	{
		public FileStream fileStream;

		public long fileModifiedtime;

		public fileInfo(FileStream f, long fm)
		{
			fileStream = f;
			fileModifiedtime = fm;
		}
	}

	private string DownloadPath = "Info/Downloads/Maps/";

	private Dictionary<NetworkConnection, Queue<FileData>> ConnectionFileQueue = new Dictionary<NetworkConnection, Queue<FileData>>();

	protected static BrixLogger logger = new BrixLogger(typeof(UNetFileTransferMessageHandler).FullName);

	private SignalConnection DisconnectSignalConnection;

	private List<NetworkConnection> ConnectionDeleteList = new List<NetworkConnection>();

	private Queue<fileInfo> IncomingFiles = new Queue<fileInfo>();

	public bool IsDownloading => IncomingFiles.Count > 0;

	protected override void RegisterHandlers()
	{
		RegisterHandler(48, FileDataStartedMessageReceived);
		RegisterHandler(49, FileDataReceivedMessageReceived);
		RegisterHandler(50, FileDataCompletedMessageReceived);
		RegisterHandler(63, AbortDownloadMessageReceived);
		DisconnectSignalConnection = GameSignals.Connect(UNetManager.Disconnected, Disconnected);
		if (NetworkServer.active)
		{
			UNetManager.instance.StartCoroutine(UpdateFileSending());
		}
	}

	public void CompressDir(string dirPath)
	{
		lzip.compressDir(dirPath, 100, dirPath + ".zip", includeRoot: true);
	}

	public override void Initialize()
	{
		RegisterHandlers();
		ConnectionFileQueue.Clear();
	}

	public void AbortUploads()
	{
		ConnectionFileQueue.Slinq().ForEach(delegate(KeyValuePair<NetworkConnection, Queue<FileData>> c)
		{
			sendAbortDownloadMessageToConnection(c.Key);
		});
		ConnectionFileQueue.Clear();
	}

	public void CleanupDownloads()
	{
		if (IncomingFiles.Count > 0)
		{
			IncomingFiles.Slinq().ForEach(delegate(fileInfo file)
			{
				file.fileStream.Close();
			});
			IncomingFiles.Clear();
		}
		if (Directory.Exists(DownloadPath))
		{
			Directory.Delete(DownloadPath, recursive: true);
		}
	}

	public float GetDownloadProgressionFromConnection(NetworkConnection connection)
	{
		float num = 0f;
		float num2 = 0f;
		if (ConnectionFileQueue.ContainsKey(connection) && ConnectionFileQueue[connection].Count > 0)
		{
			num += (float)ConnectionFileQueue[connection].Peek().FileStream.Length;
			num2 += (float)ConnectionFileQueue[connection].Peek().FileStream.Position;
		}
		if (num == 0f)
		{
			return -1f;
		}
		return num2 / num;
	}

	public void SendFileToClient(NetworkConnection connection, string file)
	{
		if (File.Exists(file))
		{
			SendFileToClient(connection, new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
		}
	}

	public void SendFileToClient(NetworkConnection connection, FileStream file)
	{
		if (!ConnectionFileQueue.ContainsKey(connection))
		{
			ConnectionFileQueue.Add(connection, new Queue<FileData>());
		}
		ConnectionFileQueue[connection].Enqueue(new FileData(file, file.Name));
	}

	private IEnumerator UpdateFileSending()
	{
		while (NetworkServer.active)
		{
			foreach (KeyValuePair<NetworkConnection, Queue<FileData>> item in ConnectionFileQueue)
			{
				TrySendData(item);
			}
			foreach (NetworkConnection connectionDelete in ConnectionDeleteList)
			{
				ConnectionFileQueue.Remove(connectionDelete);
			}
			ConnectionDeleteList.Clear();
			yield return new WaitForSeconds(0.04f);
		}
		yield return null;
	}

	private void sendAbortDownloadMessageToConnection(NetworkConnection connection)
	{
		AbortDownloadMessage msg = new AbortDownloadMessage();
		NetworkServer.SendToClient(connection.connectionId, 63, msg);
	}

	private void TrySendData(KeyValuePair<NetworkConnection, Queue<FileData>> connectionFile)
	{
		if (!NetworkServer.connections.Contains(connectionFile.Key))
		{
			CloseAllStreamForConnection(connectionFile.Key);
			ConnectionDeleteList.Add(connectionFile.Key);
		}
		if (connectionFile.Value.Count != 0)
		{
			FileData fileData = connectionFile.Value.Peek();
			if (fileData.FileStream.CanRead)
			{
				SendFileData(fileData, connectionFile.Key);
			}
		}
	}

	private void SendFileData(FileData file, NetworkConnection connection)
	{
		if (!file.UploadStarted)
		{
			FileDataMessageStart fileDataMessageStart = new FileDataMessageStart();
			fileDataMessageStart.name = Path.GetFileName(file.FileStream.Name);
			fileDataMessageStart.modifiedTime = File.GetLastWriteTime(file.FileStream.Name).ToBinary();
			NetworkServer.SendToClient(connection.connectionId, 48, fileDataMessageStart);
			file.UploadStarted = true;
			logger.InfoFormat("(Upload) sending file {0}", fileDataMessageStart.name);
			return;
		}
		if (file.FileStream.Position < file.FileStream.Length)
		{
			long num = file.FileStream.Length - file.FileStream.Position;
			if (num > 1400)
			{
				num = 1400L;
			}
			byte[] array = new byte[num];
			file.FileStream.Read(array, 0, array.Length);
			NetworkServer.SendToClient(connection.connectionId, 49, new FileDataMessage(array));
		}
		if (file.FileStream.Position == file.FileStream.Length)
		{
			logger.InfoFormat("(Upload) Finished sending file {0}", Path.GetFileName(file.FileStream.Name));
			file.FileStream.Close();
			ConnectionFileQueue[connection].Dequeue();
			NetworkServer.SendToClient(connection.connectionId, 50, new FileDataCompleteMessage());
			if (ConnectionFileQueue[connection].Count == 0)
			{
				ConnectionDeleteList.Add(connection);
			}
		}
	}

	private void CloseAllStreamForConnection(NetworkConnection connection)
	{
		foreach (FileData item in ConnectionFileQueue[connection])
		{
			item.FileStream.Dispose();
		}
	}

	private void FileDataStartedMessageReceived(NetworkMessage message)
	{
		FileDataMessageStart fileDataMessageStart = message.ReadMessage<FileDataMessageStart>();
		Directory.CreateDirectory(DownloadPath);
		IncomingFiles.Enqueue(new fileInfo(new FileStream(DownloadPath + fileDataMessageStart.name + ".zip", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite), fileDataMessageStart.modifiedTime));
		logger.InfoFormat("(Download) Added new file to queue at path {0}", fileDataMessageStart.name);
	}

	private void FileDataReceivedMessageReceived(NetworkMessage message)
	{
		FileDataMessage fileDataMessage = message.ReadMessage<FileDataMessage>();
		IncomingFiles.Peek().fileStream.Write(fileDataMessage._data, 0, fileDataMessage._data.Length);
	}

	private void FileDataCompletedMessageReceived(NetworkMessage message)
	{
		fileInfo fileInfo = IncomingFiles.Dequeue();
		fileInfo.fileStream.Close();
		File.SetLastWriteTime(fileInfo.fileStream.Name, DateTime.FromBinary(fileInfo.fileModifiedtime));
		logger.InfoFormat("(Download) Completed {0}", fileInfo.fileStream.Name);
		logger.InfoFormat("(Download) Unzipping {0}...", fileInfo.fileStream.Name);
		int[] progress = new int[1] { 10 };
		Directory.CreateDirectory(fileInfo.fileStream.Name.Split('.').First());
		lzip.decompress_File(fileInfo.fileStream.Name, fileInfo.fileStream.Name.Split('.').First(), progress);
		File.Delete(fileInfo.fileStream.Name);
	}

	private void AbortDownloadMessageReceived(NetworkMessage message)
	{
		message.ReadMessage<AbortDownloadMessage>();
		AbortDownloads();
	}

	private void AbortDownloads()
	{
		while (IncomingFiles.Count > 0)
		{
			fileInfo fileInfo = IncomingFiles.Dequeue();
			fileInfo.fileStream.Close();
			Directory.Delete(Path.GetDirectoryName(fileInfo.fileStream.Name), recursive: true);
			IncomingFiles.Clear();
		}
	}

	private void Disconnected()
	{
		AbortDownloads();
		DisconnectSignalConnection.Disconnect();
	}
}
