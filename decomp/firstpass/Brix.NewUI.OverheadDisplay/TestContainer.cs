using UnityEngine;

namespace Brix.NewUI.OverheadDisplay;

public class TestContainer : MonoBehaviour
{
	public ItemCounts ItemCounts;

	public int qty;

	public new string name;

	public Sprite img;

	public bool AddResource;

	public bool RemoveResource;

	public bool EnableButtons;

	public bool DisableButtons;

	private void Start()
	{
	}

	private void Update()
	{
		if (AddResource)
		{
			ItemCounts.SetOrAddResource(img, qty, name);
			AddResource = false;
		}
		if (RemoveResource)
		{
			ItemCounts.RemoveResource(name);
			RemoveResource = false;
		}
		if (EnableButtons)
		{
			ItemCounts.EnableButtons(enable: true);
			EnableButtons = false;
		}
		if (DisableButtons)
		{
			ItemCounts.EnableButtons(enable: false);
			DisableButtons = false;
		}
	}
}
