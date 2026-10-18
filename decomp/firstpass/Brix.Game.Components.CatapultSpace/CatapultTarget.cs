using Brix.Legacy;
using UnityEngine;

namespace Brix.Game.Components.CatapultSpace;

public class CatapultTarget : MonoBehaviour, PieMenuable
{
	public Catapult catapult;

	public bool CheckTag(string tag)
	{
		return "CatapulteTarget".Contains(tag);
	}
}
