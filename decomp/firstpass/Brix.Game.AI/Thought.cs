using System.Collections.Generic;
using Brix.Utils.Logging;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.AI;

public class Thought
{
	public enum ThoughtType
	{
		_None,
		Info,
		Warning,
		Error
	}

	public class sounds
	{
		public const string Issue = "";

		public const string Nothing = "Bricktron_Message_Nothing";

		public const string Thinking = "Bricktron_Message_Thinking";

		public const string AllWorkIsAccountedFor = "Bricktron_Message_AllWorkAccountedFor";
	}

	public static class Reactions
	{
		public static Thought Run = new Thought("##thought_area_is_too_dangerous", ThoughtType.Warning, string.Empty);
	}

	private List<string> label;

	public readonly ThoughtType type;

	public readonly string soundLabel = string.Empty;

	public readonly Thought onResolution;

	private static BrixLogger logger = new BrixLogger(typeof(Thought).FullName);

	private static Dictionary<int, Thought> thoughts = new Dictionary<int, Thought>();

	public static Thought None = new Thought("<None>", ThoughtType._None, string.Empty);

	public int id { get; private set; }

	public Thought(string label, ThoughtType type, string soundLabel = "", Thought onResolution = null)
	{
		this.label = new List<string>();
		this.label.Add(label);
		this.type = type;
		this.soundLabel = soundLabel;
		this.onResolution = onResolution;
		Register(this);
	}

	public Thought(string label, string label2, ThoughtType type, string soundLabel = "")
		: this(label, type, soundLabel)
	{
		this.label.Add(label2);
	}

	public Thought(string label, string label2, string label3, ThoughtType type, string soundLabel = "")
		: this(label, label2, type, soundLabel)
	{
		this.label.Add(label3);
	}

	public Thought(string label, string label2, string label3, string label4, ThoughtType type, string soundLabel = "")
		: this(label, label2, label3, type, soundLabel)
	{
		this.label.Add(label4);
	}

	public string GetLabel()
	{
		if (label.Count <= 1)
		{
			return label[0];
		}
		return RandomLabel();
	}

	private string RandomLabel()
	{
		return label[Random.Range(0, label.Count)];
	}

	public static Thought FromId(int id)
	{
		if (!thoughts.TryGetValue(id, out var value))
		{
			value = Exceptions.UnknownError.Thought;
			logger.WarnFormat("Could not retrieve thoughId {0} from id", id);
		}
		return value;
	}

	private Thought Register(Thought thought)
	{
		id = thought.label[0].GetHashCode();
		thoughts[id] = thought;
		logger.Info(((!NetworkServer.active) ? "CLIENT" : "SERVER") + "registered" + thought.label[0] + " as " + id);
		return thought;
	}
}
