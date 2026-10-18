using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Brix.Audio;
using Brix.Components;
using Brix.External.Factories;
using Brix.External.Signals;
using Brix.Lua;
using MoonSharp.Interpreter;
using Newtonsoft.Json;
using Smooth.Slinq;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.AI;

public class Occupation : NetworkBrixComponent
{
	public enum Type
	{
		Builder,
		Knight,
		Archer,
		Corruptron,
		Biftron,
		Minitron,
		Magitron,
		Unassigned,
		Crawler
	}

	public enum Job
	{
		Builder,
		Halberdier,
		Knight,
		Archer,
		Arbalist,
		Alchemist,
		Artificer
	}

	private Job _currentJob;

	private Labor labor;

	private Type _currentOccupation;

	public Signal<Type> OccupationChanged = new Signal<Type>();

	private Profession _profession;

	private static readonly Dictionary<CategorieOutil, Type> OccupationsPerTools = new Dictionary<CategorieOutil, Type>
	{
		{
			Adjectif.knightTool,
			Type.Knight
		},
		{
			Adjectif.kikiriTool,
			Type.Knight
		},
		{
			Adjectif.archerTool,
			Type.Archer
		},
		{
			Adjectif.slingerTool,
			Type.Archer
		},
		{
			Adjectif.crossbowTool,
			Type.Archer
		},
		{
			Adjectif.alchemistTool,
			Type.Archer
		},
		{
			Adjectif.geomancerTool,
			Type.Archer
		},
		{
			Adjectif.lancerTool,
			Type.Knight
		},
		{
			Adjectif.halbardTool,
			Type.Knight
		}
	};

	private static readonly Dictionary<CategorieOutil, Job> JobPerTools = new Dictionary<CategorieOutil, Job>
	{
		{
			Adjectif.knightTool,
			Job.Knight
		},
		{
			Adjectif.archerTool,
			Job.Archer
		},
		{
			Adjectif.crossbowTool,
			Job.Arbalist
		},
		{
			Adjectif.alchemistTool,
			Job.Alchemist
		},
		{
			Adjectif.geomancerTool,
			Job.Artificer
		},
		{
			Adjectif.halbardTool,
			Job.Halberdier
		}
	};

	private static readonly Dictionary<CategorieOutil, int> AnimationArchetypePerTools = new Dictionary<CategorieOutil, int>
	{
		{
			Adjectif.knightTool,
			1
		},
		{
			Adjectif.archerTool,
			2
		},
		{
			Adjectif.crossbowTool,
			3
		},
		{
			Adjectif.alchemistTool,
			4
		},
		{
			Adjectif.geomancerTool,
			5
		},
		{
			Adjectif.halbardTool,
			6
		}
	};

	[JsonProperty]
	public Type CurrentOccupation
	{
		get
		{
			if (_currentOccupation != Type.Builder || Profession == null)
			{
				return _currentOccupation;
			}
			return GetOccupationTypeFromToolbag();
		}
		set
		{
			if (_currentOccupation != value)
			{
				_currentOccupation = value;
				labor.OnOccupationChange();
				OccupationChanged.Invoke(value);
			}
		}
	}

	public Job CurrentJob
	{
		get
		{
			return _currentJob;
		}
		set
		{
			_currentJob = value;
			OnJobChange();
		}
	}

	public Profession Profession => _profession;

	private void OnJobChange()
	{
		Locomotion4 component = Factory.Peek(Profession.AssetKey).GetComponent<Locomotion4>();
		Locomotion4 component2 = GetComponent<Locomotion4>();
		component2.RunSpeedMperS = component.RunSpeedMperS + (float)Profession.JobSpeedBonus;
	}

	public override void Initialize()
	{
		base.Initialize();
		labor = GetComponent<Labor>();
	}

	protected override void Start()
	{
		base.Start();
		SoundEngine.SetSwitch("Profession", Enum.GetName(typeof(Type), CurrentOccupation), base.gameObject);
		Profession.toolbag.OnToolStatiticChanged.Connect(delegate(Description aDescription, Occupation aOccupation)
		{
			aOccupation.UpdateOccupationTool(aDescription);
		}, this);
	}

	[OnDeserialized]
	private void SetOccupationOnDeserialized(StreamingContext sc)
	{
		SetOccupation(_currentOccupation, equipGear: false);
	}

	public void SetOccupation(Type o, bool equipGear = true)
	{
		SetDirtyBit(1u);
		CurrentOccupation = o;
		Profession profession = GetProfession();
		_profession = profession;
		if (equipGear && NetworkServer.active)
		{
			_profession.EquipGear();
		}
		OccupationChanged.Invoke(o);
	}

	public Type GetOccupation()
	{
		return CurrentOccupation;
	}

	public void UpdateOccupationTool(Description ignored)
	{
		Type occupationTypeFromToolbag = GetOccupationTypeFromToolbag();
		if (CurrentOccupation != occupationTypeFromToolbag)
		{
			CurrentOccupation = occupationTypeFromToolbag;
			OccupationChanged.Invoke(occupationTypeFromToolbag);
		}
		Job jobFromToolbag = GetJobFromToolbag();
		if (CurrentJob != jobFromToolbag)
		{
			CurrentJob = jobFromToolbag;
			OccupationChanged.Invoke(occupationTypeFromToolbag);
		}
	}

	private bool IsOfClass(List<CategorieOutil> classes)
	{
		return classes.Slinq().Any((CategorieOutil c, Occupation self) => self.Profession.toolbag.ToolStatistics.Has(c), this);
	}

	public Type GetOccupationTypeFromToolbag()
	{
		return (from kvp in OccupationsPerTools.Slinq().FirstOrNone((KeyValuePair<CategorieOutil, Type> kvp, Occupation self) => self.Profession.toolbag.ToolStatistics.Has(kvp.Key), this)
			select kvp.Value).ValueOr((!(Profession is Builder)) ? _currentOccupation : Type.Builder);
	}

	public Job GetJobFromToolbag()
	{
		return (from kvp in JobPerTools.Slinq().FirstOrNone((KeyValuePair<CategorieOutil, Job> kvp, Occupation self) => self.Profession.toolbag.ToolStatistics.Has(kvp.Key), this)
			select kvp.Value).ValueOr((!(Profession is Builder)) ? CurrentJob : Job.Builder);
	}

	public int GetAnimationArchetypeFromToolbag()
	{
		return (from kvp in AnimationArchetypePerTools.Slinq().FirstOrNone((KeyValuePair<CategorieOutil, int> kvp, Occupation self) => self.Profession.toolbag.ToolStatistics.Has(kvp.Key), this)
			select kvp.Value).ValueOr(0);
	}

	public Profession GetProfession()
	{
		return GetComponent<Profession>();
	}

	public Sprite GetIcon64()
	{
		return GetProfession().Icon64;
	}

	public AutonomyStatus GetDefaultAutonomyStatus()
	{
		switch (CurrentOccupation)
		{
		case Type.Corruptron:
		case Type.Biftron:
		case Type.Minitron:
		case Type.Magitron:
			return AutonomyStatus.FreeAgent;
		default:
			return AutonomyStatus.InstinctOnly;
		}
	}

	[LuaDrivable("GetOccupation")]
	private static DynValue lua_GetOccupation(ScriptExecutionContext context, CallbackArguments args)
	{
		Occupation occupation = args.AsComponent<Occupation>(0, "GetOccupation");
		return DynValue.FromObject(context.OwnerScript, occupation.CurrentOccupation);
	}

	[LuaDrivable("SetOccupation")]
	private static DynValue lua_SetOccupation(ScriptExecutionContext context, CallbackArguments args)
	{
		Occupation occupation = args.AsComponent<Occupation>(0, "SetOccupation");
		Type o = args.AsEnum<Type>(1, "SetOccupation");
		bool equipGear = args.Count < 3 || args.AsBool(2, "SetOccupation");
		occupation.SetOccupation(o, equipGear);
		return DynValue.Void;
	}

	[LuaDrivable("GetJob")]
	private static DynValue lua_GetJob(ScriptExecutionContext context, CallbackArguments args)
	{
		Occupation occupation = args.AsComponent<Occupation>(0, "GetOccupation");
		return DynValue.FromObject(context.OwnerScript, occupation.CurrentJob);
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool flag = base.OnSerialize(writer, forceAll);
		bool flag2 = default(bool);
		return flag2 || flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		base.OnDeserialize(reader, initialState);
	}
}
