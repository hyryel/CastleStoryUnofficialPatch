using System;
using System.Collections.Generic;
using Brix.External.Signals;
using Brix.UI.Builder.Panel;
using Brix.Utils;
using Steamworks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Brix.UI.Builder.Menu;

public sealed class LanguageWorkshopReleaseNotesModalMenu : BaseWorkshopReleaseNotesModalMenu
{
	private static string _languageReleaseNote = string.Empty;

	private static ERemoteStoragePublishedFileVisibility _currentVisibility = ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPrivate;

	private static readonly Dictionary<ERemoteStoragePublishedFileVisibility, string> VisibilityNameMap = new Dictionary<ERemoteStoragePublishedFileVisibility, string>(FastEnumIntEqualityComparer<ERemoteStoragePublishedFileVisibility>.Instance)
	{
		{
			ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPublic,
			"##mode_worldeditor_workshop_visibility_public"
		},
		{
			ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPrivate,
			"##mode_worldeditor_workshop_visibility_private"
		},
		{
			ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityFriendsOnly,
			"##mode_worldeditor_workshop_visibility_friends"
		}
	};

	private static readonly List<ERemoteStoragePublishedFileVisibility> VisibilityList = new List<ERemoteStoragePublishedFileVisibility>
	{
		ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPublic,
		ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPrivate,
		ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityFriendsOnly
	};

	public static string LanguageReleaseNote
	{
		get
		{
			return _languageReleaseNote;
		}
		set
		{
			_languageReleaseNote = value;
		}
	}

	protected override string ReleaseNote
	{
		get
		{
			return LanguageReleaseNote;
		}
		set
		{
			LanguageReleaseNote = value;
		}
	}

	public static Signal OpenSignal => BaseWorkshopReleaseNotesModalMenu.BaseOpenSignal;

	public static Signal CloseSignal => BaseWorkshopReleaseNotesModalMenu.BaseCloseSignal;

	public static Signal RefreshSignal => BaseWorkshopReleaseNotesModalMenu.BaseRefreshSignal;

	private LanguageWorkshopReleaseNotesModalMenu(Func<RectTransform> getParent)
		: base(getParent)
	{
	}

	public static LanguageWorkshopReleaseNotesModalMenu Build(Func<RectTransform> binGetter)
	{
		return new LanguageWorkshopReleaseNotesModalMenu(binGetter);
	}

	protected override void Publish()
	{
		PublishLanguage();
	}

	private static void PublishLanguage()
	{
		LanguageWorkshopWaitingModalMenu.OpenSignal.Invoke();
		LanguageWorkshopPublishMenu.SendLanguageToWorkshop(_currentVisibility);
		CloseSignal.Invoke();
	}

	private static void ResetCurrentVisibility()
	{
		_currentVisibility = ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPrivate;
	}

	protected override PanelList GetContentBuilders()
	{
		PanelList contentBuilders = base.GetContentBuilders();
		Action<DropdownPanelComponent> action = delegate(DropdownPanelComponent panel)
		{
			panel._keyLabel.text = I2Helper.TryGet("##mode_worldeditor_workshop_visibility");
			panel._dropdown.options = new List<Dropdown.OptionData>();
			foreach (ERemoteStoragePublishedFileVisibility visibility in VisibilityList)
			{
				panel._dropdown.options.Add(new Dropdown.OptionData(I2Helper.TryGet(VisibilityNameMap[visibility]).ToUpper()));
			}
			panel._dropdown.onValueChanged.RemoveAllListeners();
			panel._dropdown.value = VisibilityList.IndexOf(_currentVisibility);
			panel._dropdown.onValueChanged.AddListener(delegate(int dstIndex)
			{
				_currentVisibility = VisibilityList[dstIndex];
				panel._valueLabel.text = I2Helper.TryGet(VisibilityNameMap[_currentVisibility]).ToUpper();
				MenuSound.PlaySound(SoundPrefix, MenuSound.Suffix.Select);
			});
			panel._itemToggle.onValueChanged.RemoveAllListeners();
			panel._itemToggle.onValueChanged.AddListener(delegate
			{
				MenuSound.PlaySound(SoundPrefix, MenuSound.Suffix.Select);
			});
			panel._itemEventTrigger.triggers.Clear();
			EventTrigger.Entry entry = new EventTrigger.Entry
			{
				eventID = EventTriggerType.PointerEnter,
				callback = new EventTrigger.TriggerEvent()
			};
			entry.callback.AddListener(delegate
			{
				MenuSound.PlaySound(SoundPrefix, MenuSound.Suffix.RolloverOn);
			});
			panel._itemEventTrigger.triggers.Add(entry);
			EventTrigger.Entry entry2 = new EventTrigger.Entry
			{
				eventID = EventTriggerType.PointerExit,
				callback = new EventTrigger.TriggerEvent()
			};
			entry2.callback.AddListener(delegate
			{
				MenuSound.PlaySound(SoundPrefix, MenuSound.Suffix.RolloverOff);
			});
			panel._itemEventTrigger.triggers.Add(entry2);
			panel._valueLabel.text = I2Helper.TryGet(VisibilityNameMap[_currentVisibility]).ToUpper();
		};
		DropdownPanel item = DropdownPanel.New(action, action).As(DropdownPanel.Variant.StandardBigReskin);
		contentBuilders.Add(item);
		return contentBuilders;
	}

	protected override void OpenSelf()
	{
		ResetCurrentVisibility();
		base.OpenSelf();
	}
}
