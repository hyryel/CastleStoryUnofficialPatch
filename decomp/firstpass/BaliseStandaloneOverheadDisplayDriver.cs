using Brix.Audio;
using Brix.Legacy;
using Brix.UI.GameSelector;
using Brix.Utils.Signals;

public class BaliseStandaloneOverheadDisplayDriver : OverheadDisplayDriver
{
	private Beacon balise;

	protected override void InitValues()
	{
		base.InitValues();
		overheadDisplay.NameTag.Spawn();
		overheadDisplay.gameObject.name = base.gameObject.name + " - Overhead";
		balise = GetComponent<Beacon>();
		IgnoreHover = true;
		overheadDisplay.NameTag.Get.WithIcon(balise.Meta.sprite, forceRefresh: true);
		overheadDisplay.NameTag.Get.SetName(balise.Meta.displayName, forceRefresh: true);
		overheadDisplay.NameTag.Get.Show = true;
		overheadDisplay.SetInteractible(value: false);
		overheadDisplay.NameTag.Get.Interactible = true;
		overheadDisplay.gameObject.SetActive(value: true);
		overheadDisplay.showByDefault = true;
		overheadDisplay.showOnHover = false;
		overheadDisplay.showByDefault = false;
		base.OnSelect();
		balise.BeaconNameChanged.Connect(delegate(string name)
		{
			overheadDisplay.NameTag.Get.SetName(name);
		}).DisconnectWhen(base.OnReleaseOrDestroySignal);
		overheadDisplay.NameTag.Get.SetName(balise.name);
	}

	protected override void ConnectToSignals()
	{
		overheadDisplay.NameTag.Get.LeftClick.Connect(OnLeftClick).DisconnectWhen(Disconnected);
		overheadDisplay.NameTag.Get.Hover.Connect(OnHover).DisconnectWhen(Disconnected);
		overheadDisplay.NameTag.Get.EndHover.Connect(OnUnhover).DisconnectWhen(Disconnected);
		if (balise.isMini)
		{
			OnMadeMini();
		}
		else
		{
			balise.MadeMini.ConnectOnce(OnMadeMini).DisconnectWhen(Disconnected);
		}
		balise.VisibilityChanged.Connect(OnBaliseVisibilityChanged).DisconnectWhen(Disconnected);
	}

	private void OnMadeMini()
	{
		overheadDisplay.NameTag.Get.JustIcon(value: true);
	}

	private void OnBaliseVisibilityChanged(bool showing)
	{
		overheadDisplay.showByDefault = showing;
		overheadDisplay.ShowOrHide();
	}

	protected override void OnHover()
	{
		UIGameSelector.Hover(base.transform.parent.gameObject);
		overheadDisplay.NameTag.Get.HighlightForOverhead();
		SoundEngine.Play(balise.Meta.sound.OnHover);
	}

	protected override void OnUnhover()
	{
		UIGameSelector.Unhover(base.transform.parent.gameObject);
		overheadDisplay.NameTag.Get.RemoveHighlightForOverhead();
	}

	private void OnLeftClick()
	{
		UIGameSelector.Select(base.transform.parent.gameObject);
		SoundEngine.Play(balise.Meta.sound.BeaconSelect);
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
}
