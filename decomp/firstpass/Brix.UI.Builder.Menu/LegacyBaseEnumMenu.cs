using System;
using System.Collections.Generic;
using Brix.Engine;
using Brix.External.Signals;
using Brix.NewUI.UserSettings_;
using Brix.Utils;
using Brix.Utils.Logging;
using UnityEngine;
using UnityEngine.UI;

namespace Brix.UI.Builder.Menu;

[RequireComponent(typeof(Container))]
public abstract class LegacyBaseEnumMenu : LegacyBaseMenu
{
	private static BrixLogger logger = new BrixLogger(typeof(LegacyBaseEnumMenu).FullName);

	public static bool isDebug = false;

	public Container actionContainer;

	public Container enumContainer;

	public Container preModalContainer;

	public Text text;

	public Text titleText;

	public Text descriptionText;

	public Color DefaultNormalColor;

	public Color SelectedNormalColor;

	public Color colorDefault;

	public Color colorSelected;

	public ContaineeExtended sourceEnumButton;

	private readonly List<SignalConnection> _sgs = new List<SignalConnection>();

	public static Operation SelectedOperation = Operation.None;

	private static readonly Action<int, LegacyBaseEnumMenu> OnItemClick_PreModal = delegate(int idx, LegacyBaseEnumMenu menu)
	{
		Container.Item item = menu.preModalContainer.items[idx];
		Operation containerPopulatorOperation = item.containerPopulatorOperation;
		if (containerPopulatorOperation == menu.TargetOperation)
		{
			SelectedOperation = menu.TargetOperation;
			menu.Create();
			OnRefresh(menu);
			menu.RefreshPayload();
		}
	};

	private static readonly Action<int, LegacyBaseEnumMenu> OnItemClick_Enum = delegate(int idx, LegacyBaseEnumMenu menu)
	{
		if (SelectedOperation == menu.TargetOperation && menu.ValidateSelection(idx))
		{
			menu._selectedOptionIdx = idx;
			OnRefresh(menu);
		}
	};

	private static readonly Action<int, LegacyBaseEnumMenu> OnItemClick_Action = delegate(int idx, LegacyBaseEnumMenu menu)
	{
		Container.Item item = menu.actionContainer.items[idx];
		if (SelectedOperation == menu.TargetOperation)
		{
			switch (item.containerPopulatorOperation)
			{
			case Operation.Apply:
				menu.ApplyToSettings();
				SelectedOperation = Operation.None;
				menu.RefreshPayload();
				break;
			case Operation.Cancel:
				menu.ApplyFromSettings();
				SelectedOperation = Operation.None;
				menu.RefreshPayload();
				break;
			case Operation.SendToWorkshop:
				menu.SendToWorkshop();
				break;
			case Operation.Workshop:
				menu.LocateOnWorkshop();
				break;
			}
		}
	};

	private int _selectedOptionIdx = -1;

	private static readonly Action<LegacyBaseEnumMenu> OnRefresh = delegate(LegacyBaseEnumMenu menu)
	{
		if (SelectedOperation == menu.TargetOperation)
		{
			menu.RefreshOptions();
			menu.RefreshHelper();
			OnRefreshActions(menu);
		}
	};

	private static readonly Action<LegacyBaseEnumMenu> OnRefreshActions = delegate(LegacyBaseEnumMenu menu)
	{
		for (int i = 0; i < menu.actionContainer.items.Count; i++)
		{
			Container.Item item = menu.actionContainer.items[i];
			bool flag = menu.ShownOperations.Contains(item.containerPopulatorOperation);
			if (item.containerPopulatorOperation == Operation.SendToWorkshop)
			{
				flag = menu.ShowsSendToWorkshop;
				string term = (item.titleText = menu.SendToWorkshopText);
				if (item.instance != null)
				{
					item.instance.title.text = I2Helper.TryGet(term).ToUpper();
				}
			}
			else if (item.containerPopulatorOperation == Operation.Workshop)
			{
				flag = menu.ShowsLocateOnWorkshop;
				string term2 = (item.titleText = menu.LocateOnWorkshopText);
				if (item.instance != null)
				{
					item.instance.title.text = I2Helper.TryGet(term2).ToUpper();
				}
			}
			item.currentState = (flag ? Containee.State.Opened : Containee.State.Closed);
			if ((bool)item.instance)
			{
				item.instance.gameObject.SetActive(flag);
			}
		}
	};

	protected abstract int OptionCount { get; }

	protected virtual bool ShowsSendToWorkshop => false;

	protected virtual bool ShowsLocateOnWorkshop => false;

	protected virtual string SendToWorkshopText => string.Empty;

	protected virtual string LocateOnWorkshopText => string.Empty;

	protected int SelectedOptionIndex => _selectedOptionIdx;

	protected virtual List<Operation> ShownOperations
	{
		get
		{
			List<Operation> list = new List<Operation>();
			list.Add(Operation.Apply);
			list.Add(Operation.Cancel);
			return list;
		}
	}

	protected virtual bool ShowHelperTitle => false;

	protected virtual bool ShowHelperDescription => false;

	protected abstract Operation TargetOperation { get; }

	protected abstract string GetTitleLabel();

	protected abstract string GetOptionLabel(int optionIndex);

	protected virtual string GetOptionSublabel(int optionIndex)
	{
		return string.Empty;
	}

	protected abstract string GetOptionDescription(int optionIndex);

	private void OnEnable()
	{
		Connect();
		Init();
	}

	private void OnDisable()
	{
		Disconnect();
	}

	protected override void Init()
	{
		ApplyFromSettings();
		RefreshPayload();
	}

	protected virtual void Connect()
	{
		if (preModalContainer != null)
		{
			_sgs.Add(preModalContainer.OnItemClickSignal.Connect(OnItemClick_PreModal, this));
		}
		if (enumContainer != null)
		{
			_sgs.Add(enumContainer.OnItemClickSignal.Connect(OnItemClick_Enum, this));
		}
		if (actionContainer != null)
		{
			_sgs.Add(actionContainer.OnItemClickSignal.Connect(OnItemClick_Action, this));
		}
		_sgs.Add(GameSignals.Connect(UserSettingManager.TempSettings.OnTempSettingsSetDirty, OnRefresh, this));
		if (enumContainer != null)
		{
			_sgs.Add(enumContainer.OnPopulateSignal.Connect(OnRefresh, this));
		}
		if (actionContainer != null)
		{
			_sgs.Add(actionContainer.OnPopulateSignal.Connect(OnRefreshActions, this));
		}
	}

	public void Disconnect()
	{
		foreach (SignalConnection sg in _sgs)
		{
			sg.Disconnect();
		}
		_sgs.Clear();
	}

	protected virtual void SendToWorkshop()
	{
	}

	protected virtual void LocateOnWorkshop()
	{
	}

	private void OnBeforePopulate()
	{
		Create();
		OnRefresh(this);
	}

	protected virtual bool ValidateSelection(int optionIdx)
	{
		return true;
	}

	private void Create()
	{
		enumContainer.Populate(isChain: true, purgeOnly: true);
		enumContainer.items.Clear();
		for (int i = 0; i < OptionCount; i++)
		{
			Container.Item item = new Container.Item();
			item.original = sourceEnumButton;
			item.isDynamic = true;
			item.keepSpriteImageEnabled = true;
			item.currentState = Containee.State.Opened;
			enumContainer.items.Add(item);
		}
		enumContainer.Populate(isChain: true);
	}

	private void RefreshOptions()
	{
		for (int i = 0; i < OptionCount; i++)
		{
			RefreshOption(i);
		}
	}

	protected virtual void RefreshOption(int optionIdx)
	{
		if (optionIdx >= 0 && optionIdx < enumContainer.items.Count)
		{
			Container.Item item = enumContainer.items[optionIdx];
			bool flag = optionIdx == _selectedOptionIdx;
			string optionLabel = GetOptionLabel(optionIdx);
			string optionSublabel = GetOptionSublabel(optionIdx);
			item.titleText = optionLabel;
			item.localizeTitle = optionLabel;
			item.subtitleText = optionSublabel;
			item.localizeSubtitle = optionSublabel;
			item.color = ((!flag) ? colorDefault : colorSelected);
			if (item.instance != null)
			{
				item.instance.spriteImage.color = item.color;
				item.instance.title.text = item.titleText;
				item.instance.subtitle.text = item.subtitleText;
			}
		}
	}

	private void RefreshHelper()
	{
		string optionLabel = GetOptionLabel(_selectedOptionIdx);
		string optionDescription = GetOptionDescription(_selectedOptionIdx);
		text.text = GetTitleLabel();
		titleText.gameObject.SetActive(ShowHelperTitle);
		titleText.text = optionLabel;
		descriptionText.gameObject.SetActive(ShowHelperDescription);
		descriptionText.text = optionDescription;
	}

	private void RefreshPayload()
	{
		GameSignals.Invoke(UserSettingManager.TempSettings.OnTempSettingsSetDirty);
		for (int i = 0; i < preModalContainer.items.Count; i++)
		{
			Container.Item item = preModalContainer.items[i];
			if ((bool)item.instance && (bool)item.instance.button)
			{
				bool flag = item.containerPopulatorOperation == SelectedOperation && SelectedOperation != Operation.None;
				ColorBlock colors = item.instance.button.colors;
				colors.normalColor = ((!flag) ? DefaultNormalColor : SelectedNormalColor);
				item.instance.button.colors = colors;
			}
			if (item.containerPopulatorOperation == TargetOperation)
			{
				item.subsubtitleText = GetOptionLabel(_selectedOptionIdx);
				if (item.instance != null && item.instance.subsubtitle != null)
				{
					item.instance.subsubtitle.text = I2Helper.TryGet(item.subsubtitleText);
				}
			}
			if (item.instance != null)
			{
				if (item.instance.title != null)
				{
					string term = item.titleText;
					term = I2Helper.TryGet(term);
					term = (item.IsTitleCaps ? term.ToUpper() : term);
					item.instance.title.text = term;
				}
				if (item.instance.subtitle != null)
				{
					string subtitleText = item.subtitleText;
					subtitleText = I2Helper.TryGet(subtitleText);
					subtitleText = (item.IsSubtitleCaps ? subtitleText.ToUpper() : subtitleText);
					item.instance.subtitle.text = subtitleText;
				}
			}
		}
	}

	protected abstract bool CompareWithSettings(UserSettings settings, int optionIndex);

	protected abstract void GetValueFromSettings(UserSettings settings, out int optionIndex);

	private void ApplyFromSettings()
	{
		UserSettings settings = UserSettingManager.TempSettings.Settings;
		GetValueFromSettings(settings, out _selectedOptionIdx);
	}

	protected abstract void SetValueAtSettings(UserSettings settings, int optionIndex);

	private void ApplyToSettings()
	{
		logger.InfoFormat("ApplyToSettings()\n{0}", TargetOperation);
		UserSettings settings = UserSettingManager.TempSettings.Settings;
		if (!CompareWithSettings(settings, _selectedOptionIdx))
		{
			UserSettingManager.TempSettings.SetDirty();
		}
		SetValueAtSettings(settings, _selectedOptionIdx);
		UserSettingManager.TempSettings.SetIntoGame();
		ApplyFromSettings();
	}
}
