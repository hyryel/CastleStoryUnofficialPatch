using Brix.Engine;
using Brix.External.Signals;
using Brix.Input;
using Brix.Utils.Logging;
using UnityEngine;

namespace Brix.UI.Builder.Menu;

public class MenuToggle : MonoBehaviour
{
	private static BrixLogger logger = new BrixLogger(typeof(MenuToggle).FullName);

	private SignalConnection toggleConnection = SignalConnection.Empty;

	private bool connected;

	private InputMode outMode = InputMode.blueprint;

	public Container container;

	private InputMode modeBeforeMenu;

	private Containee menu;

	private void Connect()
	{
		Disconnect();
		if (!connected)
		{
			toggleConnection = GameSignals.Connect(GameSignals.OnMenuToggleRequest, AutoToggleInputMode);
			connected = true;
		}
	}

	private void Disconnect()
	{
		if (connected)
		{
			toggleConnection.Disconnect();
			connected = false;
		}
	}

	private void Awake()
	{
		Connect();
	}

	private void Start()
	{
		Connect();
		CreateInputLinks();
	}

	private void OnEnable()
	{
		Connect();
	}

	private void OnDisable()
	{
		Disconnect();
	}

	private void Update()
	{
		Curseur.instance.disableLockBySystemMenu = outMode == InputMode.menu;
		Curseur.instance.disableNudgeBySystemMenu = outMode == InputMode.menu;
		outMode = InputModeController.GetCurrentMode();
	}

	private void CreateInputLinks()
	{
		ControlContext.CreateLink("Menu", "Camera");
		ControlContext.CreateLink("Menu", "General");
	}

	public void AutoToggleInputMode()
	{
		UIGameActor.SelectIdleProject();
		InputMode mode = NextMode(InputModeController.GetCurrentMode());
		InputModeController.ForceCurrentMode(mode);
		UpdateSystems(InputModeController.IsInMenu());
	}

	private InputMode NextMode(InputMode current)
	{
		if (current == InputMode.menu)
		{
			return modeBeforeMenu;
		}
		modeBeforeMenu = current;
		return InputMode.menu;
	}

	private void UpdateSystems(bool value)
	{
		ClockConfig.Paused = value;
		if (GetContainee(out menu))
		{
			menu.ToggleActivation();
		}
		ControlContext.SetContext("Menu", value);
	}

	private bool GetContainee(out Containee value)
	{
		value = null;
		if (container == null)
		{
			logger.Warn("RootMenu container is missing.\n" + base.name);
			return false;
		}
		Container.Item item = container.GetItem(LegacyBaseMenu.Operation.RootMenu);
		if (item == null)
		{
			return false;
		}
		if (item.instance == null)
		{
			logger.Warn("RootMenu item has not yet been instantiated.\n" + container.name);
			return false;
		}
		value = item.instance;
		return true;
	}
}
