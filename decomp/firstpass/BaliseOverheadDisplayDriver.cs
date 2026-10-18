using System;
using System.Collections.Generic;
using System.Linq;
using Brix.Audio;
using Brix.Game.AI;
using Brix.Input;
using Brix.Legacy;
using Brix.NewUI.OverheadDisplay;
using Brix.UI;
using Brix.UI.GameSelector;
using Brix.Utils.Signals;
using Smooth.Algebraics;

public class BaliseOverheadDisplayDriver : OverheadDisplayDriver
{
	[Flags]
	private enum MessageType
	{
		NoStorage = 1,
		NoGoals = 2,
		NoResource = 4,
		All = 7
	}

	public Beacon balise;

	private static readonly string[] MessageStrings = (from m in Enumerable.Range(0, 8)
		select BuildString((MessageType)m)).ToArray();

	private static Dictionary<Tuple<int, int>, string> nametags = new Dictionary<Tuple<int, int>, string>();

	private string zero = " x 0";

	public override void Initialize()
	{
		base.Initialize();
		balise = GetComponent<Beacon>();
	}

	protected override void InitValues()
	{
		base.InitValues();
		overheadDisplay.NameTag.Spawn();
		overheadDisplay.gameObject.name = base.gameObject.name + " - Overhead";
		IgnoreHover = true;
		overheadDisplay.NameTag.Get.WithIcon(balise.Meta.sprite, forceRefresh: true);
		overheadDisplay.NameTag.Get.SetName(BuildString(), forceRefresh: true);
		overheadDisplay.NameTag.Get.TextColor(OverheadDisplay.ownedColor);
		overheadDisplay.NameTag.Get.Show = true;
		overheadDisplay.SetInteractible(value: false);
		overheadDisplay.NameTag.Get.Interactible = true;
		overheadDisplay.gameObject.SetActive(value: true);
		overheadDisplay.showByDefault = true;
		overheadDisplay.showOnHover = false;
		overheadDisplay.showOnHover = false;
	}

	protected override void ConnectToSignals()
	{
		Picking.Instance.PickingObserver.OverridePickerActivated.Connect(OnOverridePickerActivated).DisconnectWhen(Disconnected);
		Picking.Instance.PickingObserver.OverridePickerDeactivated.Connect(OnOverridePickerDeactivated).DisconnectWhen(Disconnected);
		if (Picking.Instance.OverridePickerActive())
		{
			OnOverridePickerActivated();
		}
		overheadDisplay.NameTag.Get.RightClick.Connect(OnRightClick).DisconnectWhen(Disconnected);
		overheadDisplay.NameTag.Get.LeftClick.Connect(OnLeftClick).DisconnectWhen(Disconnected);
		overheadDisplay.NameTag.Get.Hover.Connect(OnHover).DisconnectWhen(Disconnected);
		overheadDisplay.NameTag.Get.EndHover.Connect(OnUnhover).DisconnectWhen(Disconnected);
		UIGameObserver.projects.Selected.Connect(OnProjectSelected).DisconnectWhen(Disconnected);
		UIGameObserver.projects.Unselected.Connect(OnProjectDeselected).DisconnectWhen(Disconnected);
		if (balise.isMini)
		{
			OnMadeMini();
		}
		else
		{
			balise.MadeMini.ConnectOnce(OnMadeMini).DisconnectWhen(Disconnected);
		}
		balise.VisibilityChanged.Connect(OnBaliseVisibilityChanged).DisconnectWhen(Disconnected);
		if ((bool)balise.project)
		{
			balise.project.PopulationIncreased.Connect(OnCrewChanged).DisconnectWhen(Disconnected);
			balise.project.PopulationDecreased.Connect(OnCrewChanged).DisconnectWhen(Disconnected);
		}
	}

	private void OnMadeMini()
	{
		overheadDisplay.NameTag.Get.JustIcon(value: true);
	}

	private void OnProjectSelected(Project project)
	{
		if (!(balise == null) && !(project != balise.project))
		{
			base.OnSelect();
		}
	}

	private void OnProjectDeselected(Project project)
	{
		if (!(balise == null) && !(project != balise.project))
		{
			base.OnUnselect();
		}
	}

	private string BuildString()
	{
		if (balise.project == null)
		{
			return zero;
		}
		Tuple<int, int> key = Tuple.Create(balise.project.CrewCount(), balise.project.MaximumCrewSize);
		if (!nametags.ContainsKey(key))
		{
			string text = " x " + balise.project.CrewCount() + ((balise.project.MaximumCrewSize != -1) ? ("/" + balise.project.MaximumCrewSize) : string.Empty);
			nametags.Add(key, text);
			return text;
		}
		return nametags[key];
	}

	private void OnBaliseVisibilityChanged(bool showing)
	{
		overheadDisplay.showByDefault = showing;
		overheadDisplay.ShowOrHide();
	}

	private void OnOverridePickerActivated()
	{
		overheadDisplay.NameTag.Get.Interactible = false;
	}

	private void OnOverridePickerDeactivated()
	{
		overheadDisplay.NameTag.Get.Interactible = true;
	}

	protected override void OnHover()
	{
		if (balise.project != null)
		{
			UIGameSelector.Hover(balise.project.gameObject);
			overheadDisplay.NameTag.Get.HighlightForOverhead();
			SoundEngine.Play(balise.Meta.sound.OnHover);
		}
	}

	protected override void OnUnhover()
	{
		if (balise.project != null)
		{
			UIGameSelector.Unhover(balise.project.gameObject);
			overheadDisplay.NameTag.Get.RemoveHighlightForOverhead();
		}
	}

	private void OnLeftClick()
	{
		if (balise.project != null)
		{
			UIGameSelector.Select(balise.project.gameObject);
			SoundEngine.Play(balise.Meta.sound.BeaconSelect);
		}
	}

	private void OnRightClick()
	{
		if (balise.project != null)
		{
			UIGameActor.AssignSelectionToProject(balise.project);
		}
	}

	private void OnCrewChanged(Labor labor)
	{
		if (overheadDisplay != null)
		{
			overheadDisplay.NameTag.Get.SetName(BuildString());
		}
	}

	protected override void BrixUpdate()
	{
		base.BrixUpdate();
		if (!balise.project)
		{
			SetIssueOverhead(show: false);
		}
		else if (balise.project is IdleProject)
		{
			SetIssueOverhead(show: false);
			overheadDisplay.NameTag.Get.SetName(BuildString());
		}
		else
		{
			overheadDisplay.NameTag.Get.SetName(BuildString());
			SetIssueOverhead(show: true);
		}
	}

	private void SetIssueOverhead(bool show)
	{
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (overheadDisplay != null)
		{
			overheadDisplay.showByDefault = true;
			overheadDisplay.ShowOrHide();
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (overheadDisplay != null)
		{
			overheadDisplay.showByDefault = false;
			overheadDisplay.ShowOrHide();
		}
	}

	private static string BuildString(MessageType state)
	{
		string text = string.Empty;
		if ((state & MessageType.NoStorage) != 0)
		{
			text += " NoStorage ";
		}
		if ((state & MessageType.NoGoals) != 0)
		{
			text += " NoGoals ";
		}
		if ((state & MessageType.NoResource) != 0)
		{
			text += " NoResource ";
		}
		return text;
	}

	public string BuildString(ProjectState state)
	{
		MessageType messageType = ((!state.HasGoals) ? MessageType.NoGoals : ((MessageType)0));
		return MessageStrings[(int)messageType];
	}
}
