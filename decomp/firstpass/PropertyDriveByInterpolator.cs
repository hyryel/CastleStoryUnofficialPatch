using Brix.Utils.Logging;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(PropertyDrive))]
public class PropertyDriveByInterpolator : MonoBehaviour
{
	private static BrixLogger logger = new BrixLogger(typeof(PropertyDriveByInterpolator).FullName);

	public static bool isDebug = false;

	public Interpolator interpolator;

	public PropertyDrive propertyDrive;

	private void OnEnable()
	{
		interpolator = ((!(interpolator != null)) ? base.gameObject.GetComponent<Interpolator>() : interpolator);
		propertyDrive = ((!(propertyDrive != null)) ? base.gameObject.GetComponent<PropertyDrive>() : propertyDrive);
	}

	private void Start()
	{
		if (isDebug)
		{
			logger.Info(Mathf.Round(Time.time * 100f) / 100f + " :: " + base.name + "[PropertyDriveByInterpolator].Start()\n");
		}
		interpolator.Evaluate();
		propertyDrive.RecastOld();
		propertyDrive.Apply();
	}

	public void Update()
	{
		float traverse = propertyDrive.traverse;
		propertyDrive.Traverse = interpolator.traverse;
		float traverse2 = propertyDrive.traverse;
		propertyDrive.traverseDriver = interpolator;
		if (traverse != traverse2)
		{
			propertyDrive.Apply();
		}
	}
}
