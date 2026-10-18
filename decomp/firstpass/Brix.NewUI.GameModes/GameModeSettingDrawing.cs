using UnityEngine;

namespace Brix.NewUI.GameModes;

public class GameModeSettingDrawing
{
	public GameObject drawing;

	public GameModeVariable setting;

	public GameModeSettingDrawing(GameObject drawing, GameModeVariable setting)
	{
		this.drawing = drawing;
		this.setting = setting;
	}
}
