using System;
using Brix.Assets;
using Brix.External.Signals;
using Brix.Steam;
using Brix.Utils;
using Steamworks;
using UnityEngine;

namespace Brix.UI.Builder.Menu;

public sealed class LanguageWorkshopPublishMenu : BaseWorkshopPublishMenu
{
	private static Asset_Language _language;

	public static Asset_Language Language
	{
		get
		{
			return _language;
		}
		set
		{
			_language = value;
		}
	}

	public override bool IsAssetNull => IsLanguageNull;

	public static bool IsLanguageNull => Language == null;

	public override WorkshopItem AssetItem => LanguageItem;

	public static WorkshopItem LanguageItem => (_language != null) ? _language.GetWorkshopItem() : null;

	protected override ulong AssetItemId => LanguageItemId;

	public static ulong LanguageItemId => (!IsLanguageNull && _language != null) ? _language.workshopID : 0;

	protected override bool CanPublishAsset => CanPublishLanguage;

	public static bool CanPublishLanguage => !IsLanguageNull;

	public static Signal RefreshSignal => BaseWorkshopPublishMenu.BaseRefreshSignal;

	public LanguageWorkshopPublishMenu(Func<RectTransform> getParent)
		: base(getParent)
	{
	}

	public static LanguageWorkshopPublishMenu Build(Func<RectTransform> binGetter)
	{
		return new LanguageWorkshopPublishMenu(binGetter);
	}

	public override void PublishAsset()
	{
		PublishLanguage(null);
	}

	public static void PublishLanguage(Asset_Language language)
	{
		Language = language;
		if (!IsLanguageNull)
		{
			LanguageWorkshopReleaseNotesModalMenu.OpenSignal.Invoke();
		}
	}

	public static void SendLanguageToWorkshop(ERemoteStoragePublishedFileVisibility visibility)
	{
		if (!IsLanguageNull)
		{
			string text = ((!string.IsNullOrEmpty(Language.language_name)) ? Language.language_name : I2Helper.TryGet("##mainmenu_language_unnamed"));
			string text2 = ((!string.IsNullOrEmpty(Language.description)) ? Language.description : string.Empty);
			BaseMenu.logger.Info(string.Concat("SendLanguageToWorkshop()\nLanguage == ", Language, "\ntitle == ", text, "\ndescription == ", text2, "\n"));
			Language.SendToWorkshop(text, text2, LanguageWorkshopReleaseNotesModalMenu.LanguageReleaseNote, visibility);
		}
	}
}
