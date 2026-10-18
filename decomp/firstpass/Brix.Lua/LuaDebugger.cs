using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Debugging;
using Smooth.Slinq;

namespace Brix.Lua;

public class LuaDebugger : IDebugger
{
	private enum RaisedAction
	{
		NONE,
		SUSPEND,
		EXIT,
		DONE,
		STEP
	}

	private struct SetBq
	{
		public string file;

		public int line;

		public SetBq(string f, int l)
		{
			file = f;
			line = l;
		}

		public override bool Equals(object obj)
		{
			SetBq setBq = (SetBq)obj;
			return file == setBq.file && line == setBq.line;
		}

		public override int GetHashCode()
		{
			return (((file != null) ? file.GetHashCode() : 0) * 397) ^ line.GetHashCode();
		}
	}

	private static readonly string _outputFilePath = "C:/Lua/file.txt";

	private TcpClient tcpClient;

	private NetworkStream tcpStream;

	private StreamReader streamReader;

	private StreamWriter streamWriter;

	private bool _pauseRequested;

	private bool _hasStarted;

	private bool _ready;

	private string _baseDir = string.Empty;

	private RaisedAction _raised;

	private ScriptRuntimeException _raisedException;

	private List<int> _breakpoints = new List<int>();

	private DebugService _service;

	private DebuggerAction _currentAction = new DebuggerAction();

	private Dictionary<string, SourceCode> _sources = new Dictionary<string, SourceCode>();

	private SourceCode _source;

	private List<SetBq> _setbQueue = new List<SetBq>();

	private IEnumerable<WatchItem> _callstack;

	private Dictionary<string, List<string>> _stack = new Dictionary<string, List<string>>();

	public LuaDebugger()
	{
		tcpClient = new TcpClient("192.168.0.15", 8172);
		tcpStream = tcpClient.GetStream();
		streamReader = new StreamReader(tcpStream);
		streamWriter = new StreamWriter(tcpStream);
		if (File.Exists(_outputFilePath))
		{
			File.Delete(_outputFilePath);
		}
		File.CreateText(_outputFilePath).Close();
	}

	DebuggerCaps IDebugger.GetDebuggerCaps()
	{
		WriteFile("-GetDebuggerCaps");
		return DebuggerCaps.CanDebugSourceCode;
	}

	void IDebugger.SetDebugService(DebugService debugService)
	{
		WriteFile("-SetDebugService");
		_service = debugService;
	}

	void IDebugger.SetByteCode(string[] byteCode)
	{
		string text = "\n";
		for (int i = 0; i != byteCode.Length; i++)
		{
			string text2 = text;
			text = text2 + i + " " + byteCode[i] + "\n";
		}
		WriteFile("-SetByteCode: " + text);
	}

	void IDebugger.SetSourceCode(SourceCode sourceCode)
	{
		string text = "\n";
		for (int i = 0; i != sourceCode.Lines.Length; i++)
		{
			string text2 = text;
			text = text2 + i + " " + sourceCode.Lines[i] + "\n";
		}
		WriteFile("-SetSourceCode: " + text);
		_sources.Add(sourceCode.Name, sourceCode);
		_source = sourceCode;
		for (int j = 0; j != _setbQueue.Count; j++)
		{
			if (sourceCode.Name == _setbQueue[j].file)
			{
			}
		}
	}

	bool IDebugger.IsPauseRequested()
	{
		if (_pauseRequested)
		{
			WriteFile("-IsPauseRequested");
		}
		return _pauseRequested;
	}

	List<DynamicExpression> IDebugger.GetWatchItems()
	{
		WriteFile("-GetWatchItems");
		return new List<DynamicExpression>();
	}

	void IDebugger.Update(WatchType watchType, IEnumerable<WatchItem> items)
	{
		WriteFile("-Update " + watchType);
		switch (watchType)
		{
		case WatchType.CallStack:
			_callstack = items;
			break;
		case WatchType.Locals:
		{
			WatchItem watchItem = _callstack.Slinq().First();
			if (watchItem.Location.IsEmpty())
			{
				break;
			}
			string key = _source.Name + ":" + watchItem.Name;
			List<string> value = null;
			if (!_stack.TryGetValue(key, out value))
			{
				value = new List<string>();
				_stack.Add(key, value);
			}
			value.Clear();
			{
				foreach (WatchItem item in items)
				{
					value.Add(LocalToString(item.Name, item.Value));
				}
				break;
			}
		}
		}
	}

	void IDebugger.RefreshBreakpoints(IEnumerable<SourceRef> refs)
	{
		int num = 0;
		foreach (SourceRef @ref in refs)
		{
			num++;
		}
		WriteFile("-RefreshBreakpoints " + num);
	}

	bool IDebugger.SignalRuntimeException(ScriptRuntimeException ex)
	{
		WriteFile("-SignalRuntimeException " + ex);
		WriteFile("-Message " + ex.DecoratedMessage);
		string value = Regex.Matches(ex.DecoratedMessage, "^([a-zA-Z0-9_:./\\\\]+)")[0].Value;
		value = value.Substring(0, value.Length - 1);
		string value2 = Regex.Matches(ex.DecoratedMessage, "\\(([\\d]+),")[0].Value;
		value2 = value2.Replace("(", string.Empty).Replace(",", string.Empty);
		WriteTCPPause(value, Convert.ToInt32(value2));
		_raisedException = ex;
		_pauseRequested = true;
		return _pauseRequested;
	}

	DebuggerAction IDebugger.GetAction(int ip, SourceRef sourceref)
	{
		_currentAction.Action = DebuggerAction.ActionType.None;
		_currentAction.SourceID = 0;
		_currentAction.SourceLine = 0;
		_currentAction.SourceCol = 0;
		_currentAction.Lines = null;
		if (sourceref.IsEmpty())
		{
			if (_ready && _raisedException == null)
			{
				_currentAction.Action = DebuggerAction.ActionType.StepIn;
			}
		}
		else if (!_pauseRequested && (sourceref.IsStepStop || sourceref.Breakpoint))
		{
			if (_raised == RaisedAction.SUSPEND)
			{
				sourceref.SetNoBreakPoint();
			}
			WriteTCPPause(_source.Name, sourceref.FromLine);
			WriteFile("-Paused Location:" + sourceref.FormatLocation(_service.OwnerScript));
			_pauseRequested = true;
		}
		ProcessInboundMessages(sourceref);
		return _currentAction;
	}

	void IDebugger.SignalExecutionEnded()
	{
		WriteFile("-SignalExecutionEnded");
		streamReader.Close();
		streamWriter.Close();
		tcpStream.Close();
		tcpClient.Close();
	}

	private void WriteTCPPause(string filename, int line)
	{
		if (!_hasStarted)
		{
			WriteTCP("201 Started " + filename + " " + line);
			_hasStarted = true;
		}
		else
		{
			WriteTCP("202 Paused " + filename + " " + line);
		}
		_raised = RaisedAction.NONE;
	}

	private string ReadTCP()
	{
		string text = streamReader.ReadLine();
		WriteFile("=ReadTCP    |" + text);
		return text;
	}

	private string ReadTCPBlock(int size)
	{
		char[] array = new char[size];
		streamReader.Read(array, 0, size);
		string text = new string(array);
		WriteFile("=ReadTCP " + size + "|" + text);
		return text;
	}

	private void WriteTCP(string line)
	{
		WriteFile("+WriteTCP   |" + line);
		streamWriter.WriteLine(line);
		streamWriter.Flush();
	}

	private void WriteFile(string text)
	{
		File.AppendAllText(_outputFilePath, text + "\n");
	}

	private void ProcessInboundMessages(SourceRef sourceref)
	{
		while (tcpStream.DataAvailable)
		{
			string text = ReadTCP();
			string[] array = text.Split(' ');
			_currentAction.Action = DebuggerAction.ActionType.None;
			_currentAction.SourceID = 0;
			_currentAction.SourceLine = 0;
			_currentAction.SourceCol = 0;
			_currentAction.Lines = null;
			switch (array[0])
			{
			case "BASEDIR":
				_baseDir = array[1];
				WriteTCP("200 OK");
				break;
			case "EXEC":
				WriteTCP("200 OK");
				break;
			case "LOAD":
				ReadTCPBlock(Convert.ToInt32(array[1]));
				_currentAction.Action = DebuggerAction.ActionType.Run;
				_ready = true;
				_pauseRequested = false;
				break;
			case "STACK":
				WriteTCP(BuildLuaStack(sourceref));
				break;
			case "SETB":
			{
				string text3 = _baseDir + array[1];
				if (_sources.ContainsKey(text3))
				{
					_currentAction.Action = DebuggerAction.ActionType.SetBreakpoint;
					_currentAction.SourceLine = Convert.ToInt32(array[2]);
					_currentAction.SourceID = _sources[_baseDir + array[1]].SourceID;
					_breakpoints.Add(_currentAction.SourceLine);
				}
				else
				{
					_setbQueue.Add(new SetBq(text3, Convert.ToInt32(array[2])));
				}
				WriteTCP("200 OK");
				break;
			}
			case "DELB":
				if (array[1] == "*")
				{
					_currentAction.Action = DebuggerAction.ActionType.ResetBreakpoints;
					_currentAction.Lines = _breakpoints.ToArray();
				}
				else
				{
					string text2 = _baseDir + array[1];
					if (_sources.ContainsKey(text2))
					{
						_currentAction.Action = DebuggerAction.ActionType.ClearBreakpoint;
						_currentAction.SourceLine = Convert.ToInt32(array[2]);
						_currentAction.SourceID = _sources[_baseDir + array[1]].SourceID;
						_breakpoints.Remove(_currentAction.SourceLine);
					}
					else
					{
						SetBq item = new SetBq(text2, Convert.ToInt32(array[2]));
						if (_setbQueue.Contains(item))
						{
							_setbQueue.Remove(item);
						}
					}
				}
				WriteTCP("200 OK");
				break;
			case "RUN":
				_currentAction.Action = DebuggerAction.ActionType.Run;
				_pauseRequested = false;
				WriteTCP("200 OK");
				break;
			case "SUSPEND":
				_currentAction.Action = DebuggerAction.ActionType.SetBreakpoint;
				_currentAction.SourceID = sourceref.SourceIdx;
				_currentAction.SourceLine = sourceref.FromLine;
				_currentAction.SourceCol = sourceref.FromChar;
				_raised = RaisedAction.SUSPEND;
				_pauseRequested = false;
				break;
			case "DONE":
				_currentAction.Action = DebuggerAction.ActionType.ResetBreakpoints;
				_raised = RaisedAction.DONE;
				_pauseRequested = false;
				break;
			case "EXIT":
				_currentAction.Action = DebuggerAction.ActionType.ResetBreakpoints;
				_raised = RaisedAction.EXIT;
				_pauseRequested = false;
				WriteTCP("200 OK");
				break;
			case "STEP":
				_currentAction.Action = DebuggerAction.ActionType.StepIn;
				_raised = RaisedAction.STEP;
				_pauseRequested = false;
				WriteTCP("200 OK");
				break;
			case "OVER":
				_currentAction.Action = DebuggerAction.ActionType.StepOver;
				_raised = RaisedAction.STEP;
				_pauseRequested = false;
				WriteTCP("200 OK");
				break;
			case "OUT":
				_currentAction.Action = DebuggerAction.ActionType.StepOut;
				_raised = RaisedAction.STEP;
				_pauseRequested = false;
				WriteTCP("200 OK");
				break;
			}
		}
	}

	private static string CorrectName(string name)
	{
		name = name.Replace("\"", string.Empty);
		CodeDomProvider codeDomProvider = CodeDomProvider.CreateProvider("C#");
		if (!codeDomProvider.IsValidIdentifier(name))
		{
			if (int.TryParse(name, out var _))
			{
				return "[" + name + "]";
			}
			return "[\"" + name + "\"]";
		}
		return name;
	}

	private string BuildLuaStack(SourceRef sourceref)
	{
		StringBuilder stringBuilder = new StringBuilder(1024);
		stringBuilder.Append("200 OK ");
		stringBuilder.Append("do local _={");
		foreach (WatchItem item in _callstack)
		{
			stringBuilder.Append("{");
			stringBuilder.Append("{");
			string name = item.Name;
			string text = string.Empty;
			if (string.IsNullOrEmpty(name))
			{
				stringBuilder.Append("nil,");
			}
			else
			{
				string text2 = name.Substring(0, name.IndexOf(' '));
				stringBuilder.Append("\"" + text2 + "\",");
				int num = name.IndexOf('(');
				text = name.Substring(num + 1, name.Length - num - 2);
			}
			stringBuilder.Append("\"" + _source.Name + "\",");
			stringBuilder.Append("0,");
			stringBuilder.Append((!item.Location.IsEmpty()) ? (item.Location.FromLine + ",") : "-1,");
			stringBuilder.Append((!string.IsNullOrEmpty(name)) ? "\"Lua\"," : "\"main\",");
			stringBuilder.Append("\"" + text + "\",");
			stringBuilder.Append("\"" + _source.Name + "\"");
			stringBuilder.Append("},");
			stringBuilder.Append("{");
			List<string> value = null;
			if (_stack.TryGetValue(_source.Name + ":" + item.Name, out value))
			{
				foreach (string item2 in value)
				{
					stringBuilder.Append(item2);
				}
			}
			stringBuilder.Append("},");
			stringBuilder.Append("{}");
			stringBuilder.Append("},");
		}
		stringBuilder.Append("};return _;end");
		return stringBuilder.ToString();
	}

	private static string LocalToString(string name, DynValue val)
	{
		name = CorrectName(name);
		List<int> blacklist = new List<int>();
		switch (val.Type)
		{
		case DataType.Invalid:
		case DataType.Nil:
			return name + "={nil,\"nil\"},";
		case DataType.Void:
			return name + "={void,\"void\"},";
		case DataType.Boolean:
		case DataType.Number:
			return name + "={" + val.ToString() + ",\"" + val.ToString() + "\"},";
		case DataType.String:
			return name + "={" + val.ToString() + "," + val.ToString() + "},";
		case DataType.Function:
			return name + "={function() end, \"function: 0x" + val.ReferenceID.ToString("X8") + "\"},";
		case DataType.Table:
			return name + "={" + TableToString(val.Table, blacklist) + ",\"table: 0x" + val.ReferenceID.ToString("X8") + "\"},";
		case DataType.Tuple:
		{
			string text = string.Empty;
			DynValue[] tuple = val.Tuple;
			for (int i = 0; i != tuple.Length; i++)
			{
				text += LocalToString("[\"( *vararg " + (i + 1) + ")\"]", tuple[i]);
			}
			return text;
		}
		case DataType.ClrFunction:
			return name + "={function() end, \"clrfunction\"},";
		default:
			return name + "={nil,\"NOT IMPLEMENTED\"},";
		}
	}

	private static string TableToString(Table tab, List<int> blacklist)
	{
		StringBuilder stringBuilder = new StringBuilder(512);
		stringBuilder.Append("{");
		int num = 1;
		foreach (TablePair pair in tab.Pairs)
		{
			DynValue value = pair.Value;
			string text;
			if (pair.Key.Type == DataType.Number && (int)pair.Key.Number == num)
			{
				text = string.Empty;
				num++;
			}
			else
			{
				text = CorrectName(pair.Key.ToString()) + "=";
			}
			string text2;
			if (!blacklist.Contains(value.ReferenceID))
			{
				blacklist.Add(value.ReferenceID);
				switch (value.Type)
				{
				case DataType.Invalid:
				case DataType.Nil:
					text2 = "nil,";
					break;
				case DataType.Void:
					text2 = "void,";
					break;
				case DataType.Boolean:
				case DataType.Number:
				case DataType.String:
					text2 = value.ToString() + ",";
					break;
				case DataType.Function:
					text2 = "function() end,";
					break;
				case DataType.Table:
					text2 = TableToString(value.Table, blacklist) + ",";
					break;
				case DataType.ClrFunction:
					text2 = "function(clr) end,";
					break;
				default:
					text2 = "\"NOT IMPLEMENTED\",";
					break;
				}
			}
			else
			{
				text2 = "\"...[alreadydefined]\",";
			}
			stringBuilder.Append(text + text2);
		}
		stringBuilder.Append("}");
		return stringBuilder.ToString();
	}
}
