using System.Collections.Generic;
using System.Diagnostics;
using Brix.Assets;
using Brix.NewUI.UserSettings_;
using Brix.Steam;
using Brix.ThirdParty;
using Brix.UI.Icons;
using Brix.Utils;
using I2.Loc;

namespace Brix.UI.Builder.Menu;

public class LegacyLanguageMenu : LegacyBaseEnumMenu
{
	private bool _tmpHide;

	private readonly List<Asset_Language> _languages = new List<Asset_Language>();

	private const string EnglishLanguageName = "5263688b-f726-4f38-be74-6101e1cd1566";

	private const string FrenchLanguageName = "a6fd2d3f-be77-4ed2-b02d-aa1755bb61e5";

	private const string BrowseLanguageName = "Browse";

	protected override Operation TargetOperation => Operation.ChangeLanguageTitleScreen;

	protected override int OptionCount => _languages.Count;

	protected override List<Operation> ShownOperations
	{
		get
		{
			List<Operation> list = new List<Operation>();
			list.Add(Operation.Apply);
			list.Add(Operation.Cancel);
			list.Add(Operation.SendToWorkshop);
			list.Add(Operation.Workshop);
			return list;
		}
	}

	private Asset_Language CurrentLanguage
	{
		get
		{
			TryFindLanguage(LocalizationManager.CurrentLanguage.Trim(), out var outIndex);
			return _languages[outIndex];
		}
	}

	private Asset_Language SelectedLanguage
	{
		get
		{
			if (base.SelectedOptionIndex < 0)
			{
				return null;
			}
			if (base.SelectedOptionIndex >= _languages.Count)
			{
				return null;
			}
			return _languages[base.SelectedOptionIndex];
		}
	}

	protected override string SendToWorkshopText
	{
		get
		{
			if (ExistsOnWorkshop)
			{
				return "##mainmenu_language_steambtn_resend";
			}
			return "##mainmenu_language_steambtn_send";
		}
	}

	protected override string LocateOnWorkshopText
	{
		get
		{
			if (ExistsOnWorkshop)
			{
				return "##mainmenu_language_steambtn_go";
			}
			return "##mainmenu_language_steambtn_send";
		}
	}

	protected bool ExistsOnWorkshop
	{
		get
		{
			Asset_Language selectedLanguage = SelectedLanguage;
			if (selectedLanguage == null)
			{
				return false;
			}
			if (selectedLanguage.workshopID == 0)
			{
				return false;
			}
			return true;
		}
	}

	protected override bool ShowsSendToWorkshop
	{
		get
		{
			if (_tmpHide)
			{
				return false;
			}
			if (!SteamWorkshopAgent.IsConnected)
			{
				return false;
			}
			Asset_Language selectedLanguage = SelectedLanguage;
			if (selectedLanguage == null)
			{
				return false;
			}
			if (selectedLanguage.authorType != AssetAuthorType.UGC)
			{
				return false;
			}
			return true;
		}
	}

	protected override bool ShowsLocateOnWorkshop
	{
		get
		{
			if (_tmpHide)
			{
				return false;
			}
			if (!SteamWorkshopAgent.IsConnected)
			{
				return false;
			}
			Asset_Language selectedLanguage = SelectedLanguage;
			if (selectedLanguage == null)
			{
				return false;
			}
			if (selectedLanguage.authorType == AssetAuthorType.Official)
			{
				return false;
			}
			if (!ExistsOnWorkshop)
			{
				return false;
			}
			return true;
		}
	}

	protected override void Init()
	{
		InitOptions();
		LocalizationController.UpdateLanguages();
		base.Init();
	}

	private void InitOptions()
	{
		_languages.Clear();
		List<Asset_Language> list = AssetDispensary.languages.SendQuery(AssetQueryParameters.AllSources | AssetQueryParameters.RefreshCache, AssetPlayableContext.Any);
		foreach (Asset_Language item in list)
		{
			if (TryFindLanguage(item.language_name, out var outIndex, defaultToCurrent: false))
			{
				Asset_Language asset_Language = _languages[outIndex];
				item.language_name = asset_Language.language_name;
				_languages.Remove(asset_Language);
				_languages.Insert(outIndex, item);
			}
			else
			{
				_languages.Add(item);
			}
		}
		if (!_tmpHide)
		{
			Asset_Language asset_Language2 = new Asset_Language(string.Empty);
			asset_Language2.language_name = "Browse";
			asset_Language2.rawDisplayName = "##mainmenu_language_steambtn_title";
			_languages.Add(asset_Language2);
		}
	}

	private bool IsEnglish(string languageDisplayName)
	{
		return languageDisplayName == "5263688b-f726-4f38-be74-6101e1cd1566";
	}

	private bool IsFrench(string languageDisplayName)
	{
		return languageDisplayName == "a6fd2d3f-be77-4ed2-b02d-aa1755bb61e5";
	}

	private bool IsBrowse(string languageDisplayName)
	{
		return languageDisplayName == "Browse";
	}

	protected override bool ValidateSelection(int optionIdx)
	{
		Asset_Language asset_Language = _languages[optionIdx];
		if (asset_Language.language_name == "Browse")
		{
			Process.Start("steam://url/SteamWorkshopPage/227860");
			return false;
		}
		return base.ValidateSelection(optionIdx);
	}

	private bool TryFindLanguage(string languageName, bool defaultToCurrent = true)
	{
		int outIndex;
		return TryFindLanguage(languageName, out outIndex, defaultToCurrent);
	}

	private bool TryFindLanguage(string languageName, out int outIndex, bool defaultToCurrent = true)
	{
		for (int i = 0; i < _languages.Count; i++)
		{
			Asset_Language asset_Language = _languages[i];
			if (!(asset_Language.language_name == "Browse") && asset_Language.language_name == languageName)
			{
				outIndex = i;
				return true;
			}
		}
		if (defaultToCurrent)
		{
			InitOptions();
			return TryFindLanguage(LocalizationManager.CurrentLanguage.Trim(), out outIndex, defaultToCurrent: false);
		}
		outIndex = 0;
		return false;
	}

	protected override bool CompareWithSettings(UserSettings settings, int optionIndex)
	{
		GetValueFromSettings(settings, out var optionIndex2);
		return optionIndex == optionIndex2;
	}

	protected override void GetValueFromSettings(UserSettings settings, out int optionIndex)
	{
		string language = settings.localization.language;
		TryFindLanguage(language, out optionIndex);
	}

	protected override void SetValueAtSettings(UserSettings settings, int optionIndex)
	{
		Asset_Language language = _languages[optionIndex];
		settings.localization.language = ToRegisteredLanguageName(language);
	}

	private string ToRegisteredLanguageName(Asset_Language language)
	{
		List<string> allLanguages = LocalizationManager.GetAllLanguages();
		foreach (string item in allLanguages)
		{
			if (language.language_name == item.Trim())
			{
				return item;
			}
		}
		return CurrentLanguage.language_name;
	}

	protected override string GetTitleLabel()
	{
		return I2Helper.TryGet("##mainmenu_options_language").ToUpper();
	}

	protected override string GetOptionLabel(int optionIndex)
	{
		Asset_Language asset_Language = _languages[optionIndex];
		if (IsEnglish(asset_Language.language_name))
		{
			return I2Helper.TryGet("##mainmenu_options_language_english").ToUpper();
		}
		if (IsFrench(asset_Language.language_name))
		{
			return I2Helper.TryGet("##mainmenu_options_language_french").ToUpper();
		}
		if (IsBrowse(asset_Language.language_name))
		{
			return I2Helper.TryGet("##mainmenu_language_steambtn_title").ToUpper();
		}
		return asset_Language.displayName.ToUpper();
	}

	protected override string GetOptionSublabel(int optionIndex)
	{
		Asset_Language asset_Language = _languages[optionIndex];
		if (IsBrowse(asset_Language.language_name))
		{
			return I2Helper.TryGet("##mainmenu_language_steambtn_description");
		}
		Asset_Language currentLanguage = CurrentLanguage;
		if (IsEnglish(currentLanguage.language_name))
		{
			return string.Empty;
		}
		if (IsEnglish(asset_Language.language_name))
		{
			return I2Helper.TryGet("##mainmenu_options_language_english_english");
		}
		if (IsFrench(asset_Language.language_name))
		{
			return I2Helper.TryGet("##mainmenu_options_language_french_english");
		}
		string displayName = asset_Language.displayName;
		return (!(displayName.ToUpper() == GetOptionLabel(optionIndex))) ? displayName : string.Empty;
	}

	protected override string GetOptionDescription(int optionIndex)
	{
		return string.Empty;
	}

	protected override void RefreshOption(int optionIdx)
	{
		Asset_Language asset_Language = _languages[optionIdx];
		bool flag = IsBrowse(asset_Language.language_name);
		bool flag2 = !flag && (IsEnglish(asset_Language.language_name) || IsFrench(asset_Language.language_name));
		bool flag3 = !flag && !flag2 && (asset_Language.authorType == AssetAuthorType.Workshop || asset_Language.workshopID != 0);
		if (optionIdx < 0 || optionIdx >= enumContainer.items.Count)
		{
			return;
		}
		Container.Item item = enumContainer.items[optionIdx];
		bool active = optionIdx == base.SelectedOptionIndex;
		if (item.instance != null)
		{
			item.instance.spriteImage.color = BaseMenu.Colors.Black;
			if (flag)
			{
				item.instance.spriteImage.sprite = IconKeys._UI_Big_Arrow_Right.Get64();
			}
			if (flag2)
			{
				item.instance.spriteImage.sprite = IconKeys._UI_SmallBadge.Get64();
			}
			if (flag3)
			{
				item.instance.spriteImage.sprite = IconKeys._UI_Steam.Get64();
			}
			item.instance.spriteImage.gameObject.SetActive(flag || flag2 || flag3);
			item.instance.spriteImage2.color = colorSelected;
			item.instance.spriteImage2.gameObject.SetActive(active);
			item.instance.title.text = GetOptionLabel(optionIdx);
			item.instance.subtitle.text = GetOptionSublabel(optionIdx);
			item.instance.subtitle.gameObject.SetActive(item.instance.subtitle.text != string.Empty);
		}
	}

	protected override void SendToWorkshop()
	{
		if (ShowsSendToWorkshop)
		{
			Asset_Language selectedLanguage = SelectedLanguage;
			if (selectedLanguage != null)
			{
				LanguageWorkshopPublishMenu.PublishLanguage(selectedLanguage);
			}
		}
	}

	protected override void LocateOnWorkshop()
	{
		if (ShowsLocateOnWorkshop && ExistsOnWorkshop)
		{
			Process.Start("steam://url/CommunityFilePage/" + SelectedLanguage.workshopID);
		}
	}
}
