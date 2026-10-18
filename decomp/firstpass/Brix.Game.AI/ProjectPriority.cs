using Brix.Game.Components.CatapultSpace;

namespace Brix.Game.AI;

public struct ProjectPriority
{
	public enum Bias
	{
		Low = 0,
		Medium = 4,
		High = 8,
		Locked = -10
	}

	public float bias;

	public float crewSize;

	public float globalCrewSize;

	public float remainingCrewSpace;

	public float isCurrent;

	public float goalCount;

	public float distance;

	public float deathCount;

	public float crewAggro;

	public float hasCleanup;

	public float manualBias;

	public float connectedOrigins;

	public static ProjectPriority Default = default(ProjectPriority);

	public static ProjectPriority Idle = new ProjectPriority
	{
		bias = -10000f
	};

	public static ProjectPriority Build = new ProjectPriority
	{
		crewSize = -1f,
		globalCrewSize = -0.5f,
		isCurrent = 2f,
		distance = -1f,
		manualBias = 1f
	};

	public static ProjectPriority Repair = new ProjectPriority
	{
		crewSize = -1f,
		globalCrewSize = -0.5f,
		isCurrent = 1f,
		distance = -1f,
		manualBias = 1f
	};

	public static ProjectPriority Tunnel = new ProjectPriority
	{
		crewSize = -1f,
		globalCrewSize = -0.5f,
		isCurrent = 1f,
		distance = -1f,
		manualBias = 1f
	};

	public static ProjectPriority Quarry = new ProjectPriority
	{
		globalCrewSize = -0.5f,
		crewSize = -1f,
		isCurrent = 1f,
		distance = -1f,
		manualBias = 1f
	};

	public static ProjectPriority Harvest = new ProjectPriority
	{
		crewSize = -1f,
		globalCrewSize = -0.5f,
		isCurrent = 1f,
		distance = -1f,
		hasCleanup = 3f,
		manualBias = 1f
	};

	public static ProjectPriority Landscape = new ProjectPriority
	{
		crewSize = -1f,
		globalCrewSize = -0.5f,
		isCurrent = 1f,
		distance = -1f,
		manualBias = 1f
	};

	public static ProjectPriority CleanUp = new ProjectPriority
	{
		globalCrewSize = -0.5f,
		crewSize = -1f,
		isCurrent = 1f,
		distance = -1f,
		hasCleanup = 3f,
		manualBias = 1f
	};

	public static ProjectPriority BricktronRaid = new ProjectPriority
	{
		bias = 18f,
		crewSize = -1f,
		isCurrent = 1f,
		distance = -1f,
		manualBias = 1f
	};

	public static ProjectPriority CorruptronDefensiveRaid = new ProjectPriority
	{
		bias = 18f,
		crewAggro = -1.5f,
		deathCount = -1f,
		distance = -0.1f,
		isCurrent = 5f,
		connectedOrigins = 100f
	};

	public static ProjectPriority CorruptronOffensiveRaid = new ProjectPriority
	{
		bias = 18f,
		crewAggro = -1f,
		deathCount = -0.5f,
		isCurrent = 5f,
		connectedOrigins = 100f
	};

	public static ProjectPriority CorruptronHomeRaid = new ProjectPriority
	{
		bias = 18f,
		crewAggro = -0.5f,
		deathCount = -0.25f,
		isCurrent = 5f,
		connectedOrigins = 100f
	};

	public static ProjectPriority Catapult = new ProjectPriority
	{
		crewSize = -1f,
		distance = -1f,
		manualBias = 1f
	};

	public static ProjectPriority Furnace = new ProjectPriority
	{
		bias = 2f,
		isCurrent = 3f,
		hasCleanup = 2f,
		manualBias = 1f
	};

	public static ProjectPriority WorkBench = new ProjectPriority
	{
		bias = 2f,
		isCurrent = 3f,
		hasCleanup = 10f,
		manualBias = 1f
	};

	public static float operator *(ProjectPriority input, ProjectPriority weights)
	{
		return input.bias * weights.bias + input.crewSize * weights.crewSize + input.globalCrewSize * weights.globalCrewSize + input.remainingCrewSpace * weights.remainingCrewSpace + input.isCurrent * weights.isCurrent + input.goalCount * weights.goalCount + input.distance * weights.distance + input.deathCount * weights.deathCount + input.crewAggro * weights.crewAggro + input.hasCleanup * weights.hasCleanup + input.manualBias * weights.manualBias + input.connectedOrigins * weights.connectedOrigins;
	}

	public static ProjectPriority WeightsFor(IOperable<Labor> project)
	{
		if (project is IdleProject)
		{
			return Idle;
		}
		if (project is BuildProject)
		{
			return Build;
		}
		if (project is RepairProject)
		{
			return Repair;
		}
		if (project is TunnelProject)
		{
			return Tunnel;
		}
		if (project is MineProject)
		{
			return Quarry;
		}
		if (project is HarvestProject)
		{
			return Harvest;
		}
		if (project is CleanupProject)
		{
			return CleanUp;
		}
		if (project is LandscapeProject)
		{
			return Landscape;
		}
		if (project is CatapultOperations)
		{
			return Catapult;
		}
		if (project is RaidProject)
		{
			return (project as RaidProject).RaidType switch
			{
				RaidType.Defensive => CorruptronDefensiveRaid, 
				RaidType.Offensive => CorruptronOffensiveRaid, 
				RaidType.Home => CorruptronHomeRaid, 
				_ => CorruptronDefensiveRaid, 
			};
		}
		if (project is RaidProject2)
		{
			return BricktronRaid;
		}
		return Default;
	}
}
