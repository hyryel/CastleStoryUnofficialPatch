using UnityEngine;

public class OverheadParenter : MonoBehaviour
{
	[SerializeField]
	private Transform BricktronsParent;

	[SerializeField]
	private Transform BarrelsParent;

	[SerializeField]
	private Transform BeaconsParent;

	[SerializeField]
	private Transform CorruptronParent;

	private void Awake()
	{
		OverheadDisplayDriver.ParentMe.Connect(ParentOverhead);
	}

	private void Start()
	{
	}

	private void ParentOverhead(OverheadDisplayDriver driver, GameObject overhead)
	{
		if (driver.GetType() == typeof(BricktronOverheadDisplayDriver))
		{
			overhead.transform.SetParent(BricktronsParent);
		}
		if (driver.GetType() == typeof(RecepteurOverheadDisplayDriver))
		{
			overhead.transform.SetParent(BarrelsParent);
		}
		if (driver.GetType() == typeof(BaliseOverheadDisplayDriver))
		{
			overhead.transform.SetParent(BeaconsParent);
		}
		if (driver.GetType() == typeof(BaliseStandaloneOverheadDisplayDriver))
		{
			overhead.transform.SetParent(BeaconsParent);
		}
		if (driver.GetType() == typeof(CorruptronOverheadDriver))
		{
			overhead.transform.SetParent(CorruptronParent);
		}
		if (driver.GetType() == typeof(FurnaceOverheadDisplayDriver))
		{
			overhead.transform.SetParent(BarrelsParent);
		}
	}

	private void Update()
	{
	}
}
