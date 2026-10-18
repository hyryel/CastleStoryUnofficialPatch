using System.Collections;
using UnityEngine;

namespace Brix.Network;

public class ReseauDamageInfo : ReseauBasicInfo
{
	public DamageMode mode;

	public GameObject from;

	public Hashtable data;
}
