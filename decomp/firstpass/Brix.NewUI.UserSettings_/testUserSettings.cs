using UnityEngine;

namespace Brix.NewUI.UserSettings_;

public class testUserSettings : MonoBehaviour
{
	public bool test;

	private void Start()
	{
	}

	private void Update()
	{
		if (test)
		{
			UserSettings settings = UserSettingsReader.CreateDefault();
			UserSettingsReader.Save(settings);
			test = false;
		}
	}
}
