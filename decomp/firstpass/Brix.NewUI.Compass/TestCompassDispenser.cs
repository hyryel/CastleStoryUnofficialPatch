using UnityEngine;
using UnityEngine.Serialization;

namespace Brix.NewUI.Compass;

public class TestCompassDispenser : MonoBehaviour
{
	public CompassDispenser m_Dispenser;

	public GameObject m_TargetGo;

	public Sprite m_Icon;

	public bool m_GetCompass;

	[FormerlySerializedAs("m_DestroyAllCompassFor")]
	public bool m_KillAllCompassFor;
}
