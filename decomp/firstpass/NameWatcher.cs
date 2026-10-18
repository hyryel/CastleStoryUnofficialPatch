using Brix.Utils.Logging;
using UnityEngine;

public class NameWatcher : MonoBehaviour
{
	private static BrixLogger logger = new BrixLogger(typeof(NameWatcher).FullName);

	private string oldname;

	private void Start()
	{
		oldname = base.gameObject.name;
	}

	private void Update()
	{
		if (oldname != base.gameObject.name)
		{
			logger.Info("Some meanie changed my name from " + oldname + " to " + base.gameObject.name);
			base.gameObject.name = "Player 0";
			oldname = "Player 0";
			logger.Info("But I revert it to player 0");
		}
	}
}
