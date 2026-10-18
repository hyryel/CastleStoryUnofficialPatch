using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Loaders;
using Smooth.Slinq;

namespace Brix.Lua;

public class BrixScriptLoader : IScriptLoader
{
	private struct FileInfo
	{
		public long timestamp;

		public string code;
	}

	private static Dictionary<string, FileInfo> _fileCache = new Dictionary<string, FileInfo>(128);

	private static readonly string[] _paths = new string[8] { "Info/Lua/?", "Info/Lua/?.lua", "Info/Lua/LUI/?", "Info/Lua/LUI/?.lua", "Info/Lua/Libs/?", "Info/Lua/Libs/?.lua", "?", "?.lua" };

	private string[] _personalPaths;

	public BrixScriptLoader(string customPath)
	{
		_personalPaths = UnpackStringPaths(customPath);
	}

	object IScriptLoader.LoadFile(string file, Table globalContext)
	{
		if (_fileCache.TryGetValue(file, out var value))
		{
			long num = File.GetLastWriteTimeUtc(file).ToFileTimeUtc();
			if (num > value.timestamp)
			{
				value = new FileInfo
				{
					timestamp = num,
					code = File.ReadAllText(file, Encoding.UTF8)
				};
				_fileCache[file] = value;
			}
		}
		else
		{
			long timestamp = File.GetLastWriteTimeUtc(file).ToFileTimeUtc();
			value = new FileInfo
			{
				timestamp = timestamp,
				code = File.ReadAllText(file, Encoding.UTF8)
			};
			_fileCache.Add(file, value);
		}
		return value.code;
	}

	string IScriptLoader.ResolveFileName(string filename, Table globalContext)
	{
		string text = null;
		text = ResolvePath(_personalPaths, filename);
		if (text == null)
		{
			text = ResolvePath(_paths, filename);
		}
		if (text == null && globalContext != null)
		{
			DynValue dynValue = globalContext.RawGet("LUA_PATH");
			if (dynValue.Type == DataType.String)
			{
				text = ResolvePath(_paths, dynValue.String);
			}
		}
		if (text == null)
		{
			throw new FileNotFoundException("Couldn't load file '" + filename + "' (File not found)");
		}
		return text;
	}

	string IScriptLoader.ResolveModuleName(string modname, Table globalContext)
	{
		string text = null;
		text = ResolvePath(_personalPaths, modname);
		if (text == null)
		{
			text = ResolvePath(_paths, modname);
		}
		if (text == null && globalContext != null)
		{
			DynValue dynValue = globalContext.RawGet("LUA_PATH");
			if (dynValue.Type == DataType.String)
			{
				text = ResolvePath(_paths, dynValue.String);
			}
		}
		if (text == null)
		{
			throw new FileNotFoundException("Couldn't load module '" + modname + "' (File not found)");
		}
		return text;
	}

	private string ResolvePath(string[] paths, string name)
	{
		foreach (string text in paths)
		{
			string text2 = text.Replace("?", name);
			if (_fileCache.ContainsKey(text2))
			{
				return text2;
			}
			if (File.Exists(text2))
			{
				return text2;
			}
		}
		return null;
	}

	public static string[] UnpackStringPaths(string str)
	{
		return (from s in str.Split(new char[1] { ';' }, StringSplitOptions.RemoveEmptyEntries).Slinq()
			select s.Trim() into s
			where !string.IsNullOrEmpty(s)
			select s).ToList().ToArray();
	}
}
