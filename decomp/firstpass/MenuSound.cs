using System.Collections.Generic;
using Brix.Audio;
using Brix.Utils.Logging;

public static class MenuSound
{
	public enum Prefix
	{
		_None,
		Main,
		Title,
		Pie,
		Other
	}

	public enum Suffix
	{
		_None,
		Back,
		Close,
		Enter,
		Exit,
		Fade,
		Open,
		Hover,
		Select,
		Wrong,
		Submit,
		Toggle,
		RolloverOff,
		RolloverOn,
		Confirm,
		Negative,
		GameStart,
		Expand,
		Collapse,
		ChatSend,
		ChatReceive,
		PauseIn,
		PauseOut
	}

	public struct SoundId
	{
		public Prefix prefix;

		public Suffix suffix;

		public SoundId(Prefix prefix, Suffix suffix)
		{
			this.prefix = prefix;
			this.suffix = suffix;
		}
	}

	private static BrixLogger logger = new BrixLogger(typeof(MenuSound).FullName);

	private static Dictionary<SoundId, string> SoundLabels = new Dictionary<SoundId, string>
	{
		{
			new SoundId(Prefix.Main, Suffix.Back),
			"Main_Menu_Back"
		},
		{
			new SoundId(Prefix.Main, Suffix.Close),
			"Main_Menu_Close"
		},
		{
			new SoundId(Prefix.Main, Suffix.Enter),
			"Main_Menu_Enter"
		},
		{
			new SoundId(Prefix.Main, Suffix.Exit),
			"Main_Menu_Exit"
		},
		{
			new SoundId(Prefix.Main, Suffix.Fade),
			"Main_Menu_Fade"
		},
		{
			new SoundId(Prefix.Main, Suffix.Open),
			"Main_Menu_Open"
		},
		{
			new SoundId(Prefix.Main, Suffix.Hover),
			"Main_Menu_Rollover"
		},
		{
			new SoundId(Prefix.Main, Suffix.Select),
			"Main_Menu_Select"
		},
		{
			new SoundId(Prefix.Main, Suffix.ChatSend),
			"Edit_Mode_In"
		},
		{
			new SoundId(Prefix.Main, Suffix.ChatReceive),
			"Edit_Mode_Out"
		},
		{
			new SoundId(Prefix.Main, Suffix.Wrong),
			"Main_Menu_Wrong"
		},
		{
			new SoundId(Prefix.Main, Suffix.RolloverOn),
			"Main_Menu_Rollover"
		},
		{
			new SoundId(Prefix.Title, Suffix.Back),
			"Title_Menu_Back"
		},
		{
			new SoundId(Prefix.Title, Suffix.Close),
			"Title_Menu_Close"
		},
		{
			new SoundId(Prefix.Title, Suffix.Fade),
			"Title_Menu_Fade"
		},
		{
			new SoundId(Prefix.Title, Suffix.Open),
			"Title_Menu_Open"
		},
		{
			new SoundId(Prefix.Title, Suffix.Hover),
			"Title_Menu_Rollover"
		},
		{
			new SoundId(Prefix.Title, Suffix.Select),
			"Title_Menu_Select"
		},
		{
			new SoundId(Prefix.Title, Suffix.Submit),
			"Title_Menu_Submit"
		},
		{
			new SoundId(Prefix.Title, Suffix.Toggle),
			"Main_Menu_Toggle"
		},
		{
			new SoundId(Prefix.Title, Suffix.Wrong),
			"Title_Menu_Wrong"
		},
		{
			new SoundId(Prefix.Title, Suffix.GameStart),
			"Title_Menu_Game_Start"
		},
		{
			new SoundId(Prefix.Title, Suffix.RolloverOn),
			"Title_Menu_Rollover"
		},
		{
			new SoundId(Prefix.Pie, Suffix.Wrong),
			"Pie_MenuAction_Negative"
		},
		{
			new SoundId(Prefix.Pie, Suffix.Select),
			"Pie_Menu_Action_Positive"
		},
		{
			new SoundId(Prefix.Pie, Suffix.Close),
			"Pie_Menu_Close"
		},
		{
			new SoundId(Prefix.Pie, Suffix.Expand),
			"Pie_Menu_Expand"
		},
		{
			new SoundId(Prefix.Pie, Suffix.Collapse),
			"Pie_Menu_Collapse"
		},
		{
			new SoundId(Prefix.Pie, Suffix.Confirm),
			"Pie_Menu_Confirm"
		},
		{
			new SoundId(Prefix.Pie, Suffix.Negative),
			"Pie_Menu_Negative"
		},
		{
			new SoundId(Prefix.Pie, Suffix.Open),
			"Pie_Menu_Open"
		},
		{
			new SoundId(Prefix.Pie, Suffix.RolloverOff),
			"Pie_Menu_Rollover_Off"
		},
		{
			new SoundId(Prefix.Pie, Suffix.RolloverOn),
			"Pie_Menu_Rollover_On"
		},
		{
			new SoundId(Prefix.Other, Suffix.PauseIn),
			"Pause_In"
		},
		{
			new SoundId(Prefix.Other, Suffix.PauseOut),
			"Pause_Out"
		}
	};

	public static string GetSoundLabel(Prefix prefix, Suffix suffix)
	{
		SoundId key = new SoundId(prefix, suffix);
		string result = string.Empty;
		if (SoundLabels.ContainsKey(key) && SoundLabels[key] != null)
		{
			result = SoundLabels[key];
		}
		else if (suffix == Suffix._None)
		{
		}
		return result;
	}

	public static string GetSoundLabel(Suffix suffix, Containee containee)
	{
		if (containee != null)
		{
			return GetSoundLabel(containee.container.soundPrefix, suffix);
		}
		return string.Empty;
	}

	public static string GetSoundLabel(Suffix suffix, Container container)
	{
		if (container != null)
		{
			return GetSoundLabel(container.soundPrefix, suffix);
		}
		return string.Empty;
	}

	public static bool HasSound(Suffix suffix, Containee containee)
	{
		return GetSoundLabel(containee.container.soundPrefix, suffix) != string.Empty;
	}

	public static bool HasSound(Suffix suffix, Container container)
	{
		return GetSoundLabel(container.soundPrefix, suffix) != string.Empty;
	}

	public static bool PlaySound(Prefix prefix, Suffix suffix)
	{
		string soundLabel = GetSoundLabel(prefix, suffix);
		if (soundLabel == string.Empty)
		{
			logger.WarnFormat("Missing sound\nprefix = {0}, suffix = {1}", prefix, suffix);
			return false;
		}
		SoundEngine.Play(soundLabel);
		return true;
	}

	public static bool PlaySound(SoundId soundId)
	{
		return PlaySound(soundId.prefix, soundId.suffix);
	}
}
