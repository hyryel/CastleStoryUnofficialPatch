using System.Collections.Generic;
using Brix.Game.Components.CatapultSpace;
using Brix.UI.Icons;
using Smooth.Algebraics;
using UnityEngine;

public class CatapultOverheadDisplayDriver : OverheadDisplayDriver
{
	[SerializeField]
	private CatapultOperations operations;

	private static Dictionary<Tuple<int, int>, string> nametags = new Dictionary<Tuple<int, int>, string>();

	protected override void InitValues()
	{
		base.InitValues();
		overheadDisplay.NameTag.Spawn();
		overheadDisplay.Issues.Spawn();
		overheadDisplay.NameTag.Get.Show = true;
	}

	protected override void ConnectToSignals()
	{
	}

	private string NameTag()
	{
		Tuple<int, int> key = Tuple.Create(operations.CrewCount(), operations.MaximumCrewSize);
		if (!nametags.ContainsKey(key))
		{
			string text = " Catapult  x " + operations.CrewCount() + "/" + operations.MaximumCrewSize;
			nametags.Add(key, text);
			return text;
		}
		return nametags[key];
	}

	protected override void BrixUpdate()
	{
		base.BrixUpdate();
		overheadDisplay.NameTag.Get.SetName(NameTag());
		overheadDisplay.NameTag.Get.WithIcon(GetIcon());
	}

	private Sprite GetIcon()
	{
		IconKey icon = operations.Icon;
		return icon.Get32();
	}

	private void Reset()
	{
		operations = GetComponent<CatapultOperations>();
	}
}
