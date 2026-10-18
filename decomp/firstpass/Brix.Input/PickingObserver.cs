using Brix.Engine;
using Brix.External.Signals;

namespace Brix.Input;

public class PickingObserver
{
	public Signal IsAltActionChanged = new Signal();

	private bool _isAltAction;

	public Signal IsRotateActionChanged = new Signal();

	private bool _isRotateAction;

	public Signal IsSecondaryActionChanged = new Signal();

	private bool _isSecondaryAction;

	public Signal<Picker> PickerActivated = new Signal<Picker>();

	public Signal<Picker> PickerDeactivated = new Signal<Picker>();

	public Signal<PickerComponent> PickerCompActivated = new Signal<PickerComponent>();

	public Signal<PickerComponent> PickerCompDeactivated = new Signal<PickerComponent>();

	public Signal OverridePickerActivated = new Signal();

	public Signal OverridePickerDeactivated = new Signal();

	public Signal OverridePickerSetWithOptions = new Signal();

	public Signal OverridePickerUpdateComplete = new Signal();

	public static GameSignals.GameSignal lua_OverridePickerSetWithOptions = GameSignals.CreateStaticSignal();

	public static GameSignals.GameSignal<Picker, Picker> lua_OnSetUserActivePicker = GameSignals.CreateStaticSignal<Picker, Picker>();

	public static GameSignals.GameSignal lua_BlueprintSelectionActionTaken = GameSignals.CreateStaticSignal();

	public bool IsAltAction
	{
		get
		{
			return _isAltAction;
		}
		set
		{
			if (_isAltAction != value)
			{
				_isAltAction = value;
				IsAltActionChanged.Invoke();
			}
		}
	}

	public bool IsRotateAction
	{
		get
		{
			return _isRotateAction;
		}
		set
		{
			if (_isRotateAction != value)
			{
				_isRotateAction = value;
				IsRotateActionChanged.Invoke();
			}
		}
	}

	public bool IsSecondaryAction
	{
		get
		{
			return _isSecondaryAction;
		}
		set
		{
			if (_isSecondaryAction != value)
			{
				_isSecondaryAction = value;
				IsSecondaryActionChanged.Invoke();
			}
		}
	}
}
