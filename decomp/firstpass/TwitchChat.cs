using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using Brix.Utils.Logging;
using UnityEngine;
using UnityEngine.Events;

public class TwitchChat : MonoBehaviour
{
	public class MsgEvent : UnityEvent<string>
	{
	}

	protected static BrixLogger logger = new BrixLogger(typeof(TwitchChat).FullName);

	private string server = "irc.twitch.tv";

	private int port = 6667;

	public string userName = string.Empty;

	public string passName = string.Empty;

	public string chanName = string.Empty;

	public MsgEvent messageRecievedEvent = new MsgEvent();

	private string buffer = string.Empty;

	private bool stopThreads;

	private Queue<string> commandQueue = new Queue<string>();

	private List<string> recievedMsgs = new List<string>();

	private Thread inProc;

	private Thread outProc;

	public bool connected;

	public void Set(string userName, string passName, string chanName)
	{
		this.userName = userName;
		this.passName = passName;
		this.chanName = chanName;
	}

	public void TryConnect()
	{
		if (!connected)
		{
			Connect();
			connected = true;
		}
	}

	public void TryDisconnect()
	{
		if (connected)
		{
			Disconnect();
			connected = false;
		}
	}

	private void Disconnect()
	{
		if (outProc != null)
		{
			outProc.Abort();
		}
		if (inProc != null)
		{
			inProc.Abort();
		}
	}

	private void Connect()
	{
		TcpClient tcpClient = new TcpClient();
		tcpClient.Connect(server, port);
		if (!tcpClient.Connected)
		{
			logger.Info("Failed to connect!");
			return;
		}
		NetworkStream networkStream = tcpClient.GetStream();
		StreamReader input = new StreamReader(networkStream);
		StreamWriter output = new StreamWriter(networkStream);
		output.WriteLine("PASS " + passName);
		output.WriteLine("NICK " + userName);
		output.Flush();
		outProc = new Thread((ThreadStart)delegate
		{
			IRCOutputProcedure(output);
		});
		outProc.Start();
		inProc = new Thread((ThreadStart)delegate
		{
			IRCInputProcedure(input, networkStream);
		});
		inProc.Start();
	}

	private void IRCInputProcedure(TextReader input, NetworkStream networkStream)
	{
		while (!stopThreads)
		{
			if (!networkStream.DataAvailable)
			{
				continue;
			}
			buffer = input.ReadLine();
			if (buffer.Contains("PRIVMSG #"))
			{
				lock (recievedMsgs)
				{
					recievedMsgs.Add(buffer);
				}
			}
			if (buffer.StartsWith("PING "))
			{
				SendCommand(buffer.Replace("PING", "PONG"));
			}
			if (buffer.Split(' ')[1] == "001")
			{
				SendCommand("JOIN #" + chanName);
			}
		}
	}

	private void IRCOutputProcedure(TextWriter output)
	{
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		while (!stopThreads)
		{
			lock (commandQueue)
			{
				if (commandQueue.Count > 0 && stopwatch.ElapsedMilliseconds > 1750)
				{
					output.WriteLine(commandQueue.Peek());
					output.Flush();
					commandQueue.Dequeue();
					stopwatch.Reset();
					stopwatch.Start();
				}
			}
		}
	}

	public void SendCommand(string cmd)
	{
		lock (commandQueue)
		{
			commandQueue.Enqueue(cmd);
		}
	}

	public void SendMsg(string msg)
	{
		lock (commandQueue)
		{
			commandQueue.Enqueue("PRIVMSG #" + chanName + " :" + msg);
		}
	}

	private void Start()
	{
	}

	private void OnEnable()
	{
		stopThreads = false;
	}

	private void OnDisable()
	{
		stopThreads = true;
	}

	private void OnDestroy()
	{
		stopThreads = true;
	}

	private void Update()
	{
		lock (recievedMsgs)
		{
			if (recievedMsgs.Count > 0)
			{
				for (int i = 0; i < recievedMsgs.Count; i++)
				{
					messageRecievedEvent.Invoke(recievedMsgs[i]);
				}
				recievedMsgs.Clear();
			}
		}
	}
}
