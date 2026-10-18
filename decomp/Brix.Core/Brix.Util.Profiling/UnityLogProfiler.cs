using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;

namespace Brix.Util.Profiling;

public class UnityLogProfiler : IProfiler
{
	private class Entry
	{
		public string name;

		public float time;

		public long memory;

		public int count;

		public string log => name + ((count > 1) ? (" (" + count + ")") : "") + ": " + time + " seconds total (" + PrintMemory(memory) + ").";
	}

	public float totalTime = 0f;

	public long totalMemory = 0L;

	public string name;

	private Dictionary<string, Entry> _entries = new Dictionary<string, Entry>();

	private List<string> _timerNames = new List<string>();

	private List<float> _timerTimes = new List<float>();

	private List<long> _timerMemory = new List<long>();

	public float CurrentTime => Time.realtimeSinceStartup;

	public long CurrentMemory => UnityEngine.Profiling.Profiler.GetTotalAllocatedMemory();

	public UnityLogProfiler(string name)
	{
		this.name = name;
	}

	protected void AddEntry(string name, float time, long memory, int stackHeight = 0)
	{
		if (!_entries.TryGetValue(name, out var value))
		{
			value = new Entry
			{
				name = name,
				time = 0f,
				memory = 0L,
				count = 0
			};
			_entries.Add(name, value);
		}
		value.time += time;
		value.memory += memory;
		value.count++;
		if (stackHeight == 0)
		{
			totalTime += time;
			totalMemory += memory;
		}
	}

	protected string GetReport()
	{
		if (_entries.Count == 0)
		{
			return null;
		}
		string text = Donnees.PrintAll(from t in _entries.Values
			orderby t.time descending
			select t.log, "\n");
		string result = "Profiler report - " + name + ": " + totalTime + " seconds grand total (" + PrintMemory(totalMemory) + ")\n" + text;
		_entries.Clear();
		totalTime = 0f;
		totalMemory = 0L;
		return result;
	}

	private static string PrintMemory(long memory)
	{
		if (memory < 8192)
		{
			return memory + "B";
		}
		if (memory < 8388608)
		{
			return (memory + 512 >> 10) + "K";
		}
		return (memory + 524288 >> 20) + "M";
	}

	public virtual void BeginSample(string s)
	{
	}

	public virtual void EndSample()
	{
	}

	public virtual void PrintSamples()
	{
	}
}
