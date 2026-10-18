using UnityEngine;

namespace Brix.NewUI.UserSettings_;

public class UserSettings
{
	public enum LODLevels
	{
		low,
		medium,
		high,
		disabled
	}

	public enum SSAOQualityLevels
	{
		high,
		medium,
		low,
		off
	}

	public class Graphic
	{
		public bool fullscreen;

		public int screenResolutionX;

		public int screenResolutionY;

		public int qualitySettings;

		public int targetFPS;

		public LODLevels terrainLodQuality;

		public SSAOQualityLevels ssao;

		public bool colorCorrection;

		public bool bloom;

		public ShadowQuality shadowQuality;

		public ShadowResolution shadowResolution;

		public float canvasScale;

		public int overrideTerrainAlgos;

		public int terrainAcceleration;

		public int terrainThreading;

		public int overrideTerrainLodMax;

		public int overrideTerrainLateAlgos;

		public bool editHeightmap;

		public float fogOpacity;

		public int _version;
	}

	public class Audio
	{
		public bool volumeMute;

		public float volumeMaster;

		public float volumeUI;

		public float volumeSon;

		public float volumeAmbiance;

		public float volumeMusique;

		public float volumeVoice;
	}

	public class Game
	{
		public bool disableSpecialAssets;

		public bool welcomeDisclaimers;

		public bool showHelpAtGameStart;

		public bool pauseOnFocusLost;

		public bool showUnsupportedGameModes;
	}

	public class Input
	{
		public bool scrollwheelInvert;

		public float scrollwheelFactor;

		public bool enableScreenLock;

		public bool enableScreenNudge;

		public int _configsVersion;
	}

	public class Network
	{
		public bool enableMultiplayer;

		public string IP;

		public string Port;

		public string Name;
	}

	public class Localization
	{
		public string language;
	}

	public class Chat
	{
		public class Twitch
		{
			public bool autoConnect;

			public string chanName;

			public string userName;

			public string passName;
		}

		public Twitch twitch;

		public Chat()
		{
			twitch = new Twitch();
		}
	}

	public class WorldEditor
	{
		public class BaseBrush
		{
			public float radius;

			public float intensity;

			public int falloffCurveIdx;
		}

		public class AddVolumeBrush : BaseBrush
		{
		}

		public class RemoveVolumeBrush : BaseBrush
		{
		}

		public class TerrainHeightBrush : BaseBrush
		{
			public bool useLevelOverride;

			public int levelOverride;
		}

		public class AddIslandBrush : TerrainHeightBrush
		{
		}

		public class RemoveIslandBrush : TerrainHeightBrush
		{
		}

		public class PlateauBrush : TerrainHeightBrush
		{
			public bool useAutoLockHeight;
		}

		public class SmoothBrush : BaseBrush
		{
			public float randomWeight;

			public float randomHeight;
		}

		public class BaseTerrainBrush : BaseBrush
		{
			public float distanceToEdge;
		}

		public class SandBrush : BaseTerrainBrush
		{
		}

		public class GrassBrush : BaseTerrainBrush
		{
		}

		public class PlantBrush : BaseTerrainBrush
		{
		}

		public class TreeBrush : BaseTerrainBrush
		{
		}

		public class MineralBrush : BaseTerrainBrush
		{
		}

		public int brushTypeIdx;

		public AddVolumeBrush addVolumeBrush;

		public RemoveVolumeBrush removeVolumeBrush;

		public AddIslandBrush addIslandBrush;

		public RemoveIslandBrush removeIslandBrush;

		public TerrainHeightBrush terrainHeightBrush;

		public PlateauBrush plateauBrush;

		public SmoothBrush smoothBrush;

		public SandBrush sandBrush;

		public GrassBrush grassBrush;

		public PlantBrush plantBrush;

		public TreeBrush treeBrush;

		public MineralBrush mineralBrush;

		public WorldEditor()
		{
			addVolumeBrush = new AddVolumeBrush();
			removeVolumeBrush = new RemoveVolumeBrush();
			addIslandBrush = new AddIslandBrush();
			removeIslandBrush = new RemoveIslandBrush();
			terrainHeightBrush = new TerrainHeightBrush();
			plateauBrush = new PlateauBrush();
			smoothBrush = new SmoothBrush();
			sandBrush = new SandBrush();
			grassBrush = new GrassBrush();
			plantBrush = new PlantBrush();
			treeBrush = new TreeBrush();
			mineralBrush = new MineralBrush();
		}
	}

	public Game game;

	public Graphic graphic;

	public Audio audio;

	public Input input;

	public Network network;

	public Localization localization;

	public Chat chat;

	public WorldEditor worldEditor;

	public UserSettings()
	{
		game = new Game();
		graphic = new Graphic();
		audio = new Audio();
		input = new Input();
		network = new Network();
		localization = new Localization();
		chat = new Chat();
		worldEditor = new WorldEditor();
	}
}
