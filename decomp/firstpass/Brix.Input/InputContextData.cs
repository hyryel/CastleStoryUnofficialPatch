using Brix.External.Factories;

namespace Brix.Input;

public class InputContextData
{
	public InputMode mode;

	public Factory.AssetKey pickerKey;

	public Factory.AssetKey objectKey;

	public IPickerComponentOptions options;

	public static InputContextData GetCurrentActiveContext()
	{
		InputContextData inputContextData = new InputContextData();
		inputContextData.mode = InputModeController.GetCurrentMode();
		Picker overridePicker = Picking.Instance.OverridePicker;
		if (overridePicker != null && overridePicker.PersistsInProjectContext)
		{
			inputContextData.pickerKey = overridePicker.AssetKey;
			inputContextData.objectKey = overridePicker.LastHeldKey;
			inputContextData.options = overridePicker.LastOptions;
		}
		else
		{
			inputContextData.pickerKey = null;
			inputContextData.objectKey = null;
			inputContextData.options = null;
		}
		return inputContextData;
	}

	public void SetContextActive()
	{
		InputModeController.SetCurrentMode(mode);
		Picking.Instance.SendPicker(pickerKey, objectKey, options);
	}
}
