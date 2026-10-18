public class FireflyOverheadDisplayDriver : OverheadDisplayDriver
{
	protected override void Awake()
	{
		base.Awake();
		if (!initialized)
		{
			Init();
		}
	}

	protected override void InitValues()
	{
		base.InitValues();
		overheadDisplay.NameTag.Spawn();
		overheadDisplay.NameTag.Get.ForceShow = true;
		overheadDisplay.NameTag.Get.Show = false;
		overheadDisplay.hideIfFar = false;
		overheadDisplay.showByDefault = false;
		overheadDisplay.showOnHover = false;
		overheadDisplay.showOnSelect = false;
	}

	protected override void ConnectToSignals()
	{
	}

	public void SetName(string newName)
	{
		if (!initialized)
		{
			Init();
		}
		if (string.IsNullOrEmpty(newName))
		{
			overheadDisplay.NameTag.Get.Show = false;
			overheadDisplay.NameTag.Get.SetName(string.Empty);
		}
		else
		{
			overheadDisplay.NameTag.Get.Show = true;
			overheadDisplay.NameTag.Get.SetName(newName);
		}
	}
}
