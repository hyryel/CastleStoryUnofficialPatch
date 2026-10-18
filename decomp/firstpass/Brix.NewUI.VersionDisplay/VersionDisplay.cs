using Brix.Engine;
using UnityEngine;
using UnityEngine.UI;

namespace Brix.NewUI.VersionDisplay;

public class VersionDisplay : MonoBehaviour
{
	public Text text;

	public string title;

	public string version;

	private void Start()
	{
		SetVersion(Neo.DisplayVersion);
	}

	public void SetVersion(string version)
	{
		this.version = version;
		Rebuild();
	}

	private void Rebuild()
	{
		if (text != null)
		{
			text.text = title + "\n" + version;
		}
	}
}
