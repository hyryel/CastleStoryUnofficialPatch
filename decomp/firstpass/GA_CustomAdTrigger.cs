using System;
using UnityEngine;

[Serializable]
public class GA_CustomAdTrigger
{
	[SerializeField]
	public GA_AdSupport.GAEventCat eventCat;

	[SerializeField]
	public string eventID = string.Empty;

	[SerializeField]
	public GA_AdSupport.GAAdNetwork AdNetwork;
}
