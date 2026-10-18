public class CorruptronOverheadDriver : OverheadDisplayDriver
{
	protected override void InitValues()
	{
		base.InitValues();
		IgnoreHover = true;
		overheadDisplay.BigIcon.Spawn();
		overheadDisplay.BigIcon.Get.Show = false;
		overheadDisplay.Color = BackgroundColor;
	}

	protected override void ConnectToSignals()
	{
	}
}
