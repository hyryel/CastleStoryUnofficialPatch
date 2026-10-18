using UnityEngine;

namespace Brix.Legacy;

public class AutoParentRootObject : MonoBehaviour
{
	public string targetName;

	private GameObject target;

	public bool unparentOnDestroy;

	private void Start()
	{
		if (!string.IsNullOrEmpty(targetName))
		{
			target = GameObject.Find("/" + targetName);
			if (target != null)
			{
				target.transform.parent = base.transform;
			}
		}
	}

	private void OnApplicationQuit()
	{
		target = null;
	}

	private void OnDestroy()
	{
		if (unparentOnDestroy && target != null)
		{
			target.transform.parent = null;
		}
	}
}
