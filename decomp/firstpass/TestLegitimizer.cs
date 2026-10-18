using UnityEngine;

public class TestLegitimizer : MonoBehaviour
{
	public string gameMode;

	public string preset;

	public bool testGameMode;

	public bool testPreset;

	public bool result;

	private void Update()
	{
		if (testGameMode)
		{
			result = Legitimizer.GameModeIsLegit(gameMode);
			testGameMode = false;
		}
		if (testPreset)
		{
			result = Legitimizer.PresetIsLegit(preset);
			testPreset = false;
		}
	}
}
