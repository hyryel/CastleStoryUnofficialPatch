using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

public class LocalizationHelper : MonoBehaviour
{
	public Text label;

	public bool translate = true;

	private void Start()
	{
		if (translate)
		{
			label.text = ScriptLocalization.Get(label.text);
		}
	}
}
