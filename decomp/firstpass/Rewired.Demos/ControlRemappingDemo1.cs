using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rewired.Demos;

[AddComponentMenu("")]
public class ControlRemappingDemo1 : MonoBehaviour
{
	private class ControllerSelection
	{
		private int _id;

		private int _idPrev;

		private ControllerType _type;

		private ControllerType _typePrev;

		public int id
		{
			get
			{
				return _id;
			}
			set
			{
				_idPrev = _id;
				_id = value;
			}
		}

		public ControllerType type
		{
			get
			{
				return _type;
			}
			set
			{
				_typePrev = _type;
				_type = value;
			}
		}

		public int idPrev => _idPrev;

		public ControllerType typePrev => _typePrev;

		public bool hasSelection => _id >= 0;

		public ControllerSelection()
		{
			Clear();
		}

		public void Set(int id, ControllerType type)
		{
			this.id = id;
			this.type = type;
		}

		public void Clear()
		{
			_id = -1;
			_idPrev = -1;
			_type = ControllerType.Joystick;
			_typePrev = ControllerType.Joystick;
		}
	}

	private class DialogHelper
	{
		public enum DialogType
		{
			None = 0,
			JoystickConflict = 1,
			ElementConflict = 2,
			KeyConflict = 3,
			DeleteAssignmentConfirmation = 10,
			AssignElement = 11
		}

		private const float openBusyDelay = 0.25f;

		private const float closeBusyDelay = 0.1f;

		private DialogType _type;

		private bool _enabled;

		private float _closeTime;

		private bool _closeTimerRunning;

		private float _busyTime;

		private bool _busyTimerRunning;

		private Action<int> drawWindowDelegate;

		private GUI.WindowFunction drawWindowFunction;

		private WindowProperties windowProperties;

		private int currentActionId;

		private Action<int, UserResponse> resultCallback;

		private float busyTimer
		{
			get
			{
				if (!_busyTimerRunning)
				{
					return 0f;
				}
				return _busyTime - Time.realtimeSinceStartup;
			}
		}

		public bool enabled
		{
			get
			{
				return _enabled;
			}
			set
			{
				if (value)
				{
					if (_type != DialogType.None)
					{
						StateChanged(0.25f);
					}
				}
				else
				{
					_enabled = value;
					_type = DialogType.None;
					StateChanged(0.1f);
				}
			}
		}

		public DialogType type
		{
			get
			{
				if (!_enabled)
				{
					return DialogType.None;
				}
				return _type;
			}
			set
			{
				if (value == DialogType.None)
				{
					_enabled = false;
					StateChanged(0.1f);
				}
				else
				{
					_enabled = true;
					StateChanged(0.25f);
				}
				_type = value;
			}
		}

		public float closeTimer
		{
			get
			{
				if (!_closeTimerRunning)
				{
					return 0f;
				}
				return _closeTime - Time.realtimeSinceStartup;
			}
		}

		public bool busy => _busyTimerRunning;

		public DialogHelper()
		{
			drawWindowDelegate = DrawWindow;
			drawWindowFunction = drawWindowDelegate.Invoke;
		}

		public void StartModal(int queueActionId, DialogType type, WindowProperties windowProperties, Action<int, UserResponse> resultCallback)
		{
			StartModal(queueActionId, type, windowProperties, resultCallback, 0f, -1f);
		}

		public void StartModal(int queueActionId, DialogType type, WindowProperties windowProperties, Action<int, UserResponse> resultCallback, float closeTimer)
		{
			StartModal(queueActionId, type, windowProperties, resultCallback, closeTimer, -1f);
		}

		public void StartModal(int queueActionId, DialogType type, WindowProperties windowProperties, Action<int, UserResponse> resultCallback, float closeTimer, float openBusyDelay)
		{
			currentActionId = queueActionId;
			this.windowProperties = windowProperties;
			this.type = type;
			this.resultCallback = resultCallback;
			if (closeTimer > 0f)
			{
				StartCloseTimer(closeTimer);
			}
			if (openBusyDelay >= 0f)
			{
				StateChanged(openBusyDelay);
			}
		}

		public void Update()
		{
			Draw();
			UpdateTimers();
		}

		public void Draw()
		{
			if (_enabled)
			{
				bool flag = GUI.enabled;
				GUI.enabled = true;
				GUILayout.Window(windowProperties.windowId, windowProperties.rect, drawWindowFunction, windowProperties.title);
				GUI.FocusWindow(windowProperties.windowId);
				if (GUI.enabled != flag)
				{
					GUI.enabled = flag;
				}
			}
		}

		public void DrawConfirmButton()
		{
			DrawConfirmButton("Confirm");
		}

		public void DrawConfirmButton(string title)
		{
			bool flag = GUI.enabled;
			if (busy)
			{
				GUI.enabled = false;
			}
			if (GUILayout.Button(title))
			{
				Confirm(UserResponse.Confirm);
			}
			if (GUI.enabled != flag)
			{
				GUI.enabled = flag;
			}
		}

		public void DrawConfirmButton(UserResponse response)
		{
			DrawConfirmButton(response, "Confirm");
		}

		public void DrawConfirmButton(UserResponse response, string title)
		{
			bool flag = GUI.enabled;
			if (busy)
			{
				GUI.enabled = false;
			}
			if (GUILayout.Button(title))
			{
				Confirm(response);
			}
			if (GUI.enabled != flag)
			{
				GUI.enabled = flag;
			}
		}

		public void DrawCancelButton()
		{
			DrawCancelButton("Cancel");
		}

		public void DrawCancelButton(string title)
		{
			bool flag = GUI.enabled;
			if (busy)
			{
				GUI.enabled = false;
			}
			if (GUILayout.Button(title))
			{
				Cancel();
			}
			if (GUI.enabled != flag)
			{
				GUI.enabled = flag;
			}
		}

		public void Confirm()
		{
			Confirm(UserResponse.Confirm);
		}

		public void Confirm(UserResponse response)
		{
			resultCallback(currentActionId, response);
			Close();
		}

		public void Cancel()
		{
			resultCallback(currentActionId, UserResponse.Cancel);
			Close();
		}

		private void DrawWindow(int windowId)
		{
			windowProperties.windowDrawDelegate(windowProperties.title, windowProperties.message);
		}

		private void UpdateTimers()
		{
			if (_closeTimerRunning && closeTimer <= 0f)
			{
				Cancel();
			}
			if (_busyTimerRunning && busyTimer <= 0f)
			{
				_busyTimerRunning = false;
			}
		}

		public void StartCloseTimer(float time)
		{
			_closeTime = time + Time.realtimeSinceStartup;
			_closeTimerRunning = true;
		}

		private void StartBusyTimer(float time)
		{
			_busyTime = time + Time.realtimeSinceStartup;
			_busyTimerRunning = true;
		}

		private void Close()
		{
			Reset();
			StateChanged(0.1f);
		}

		private void StateChanged(float delay)
		{
			StartBusyTimer(delay);
		}

		private void Reset()
		{
			_enabled = false;
			_type = DialogType.None;
			currentActionId = -1;
			resultCallback = null;
			_closeTimerRunning = false;
			_closeTime = 0f;
		}

		private void ResetTimers()
		{
			_busyTimerRunning = false;
			_closeTimerRunning = false;
		}

		public void FullReset()
		{
			Reset();
			ResetTimers();
		}
	}

	private abstract class QueueEntry
	{
		public enum State
		{
			Waiting,
			Confirmed,
			Canceled
		}

		private static int uidCounter;

		public int id { get; protected set; }

		public QueueActionType queueActionType { get; protected set; }

		public State state { get; protected set; }

		public UserResponse response { get; protected set; }

		protected static int nextId
		{
			get
			{
				int result = uidCounter;
				uidCounter++;
				return result;
			}
		}

		public QueueEntry(QueueActionType queueActionType)
		{
			id = nextId;
			this.queueActionType = queueActionType;
		}

		public void Confirm(UserResponse response)
		{
			state = State.Confirmed;
			this.response = response;
		}

		public void Cancel()
		{
			state = State.Canceled;
		}
	}

	private class JoystickAssignmentChange : QueueEntry
	{
		public int playerId { get; private set; }

		public int joystickId { get; private set; }

		public bool assign { get; private set; }

		public JoystickAssignmentChange(int newPlayerId, int joystickId, bool assign)
			: base(QueueActionType.JoystickAssignment)
		{
			playerId = newPlayerId;
			this.joystickId = joystickId;
			this.assign = assign;
		}
	}

	private class ElementAssignmentChange : QueueEntry
	{
		public int playerId { get; private set; }

		public int controllerId { get; private set; }

		public ControllerType controllerType { get; private set; }

		public ControllerMap controllerMap { get; private set; }

		public int actionElementMapId { get; private set; }

		public int actionId { get; private set; }

		public Pole actionAxisContribution { get; private set; }

		public InputActionType actionType { get; private set; }

		public bool assignFullAxis { get; private set; }

		public bool invert { get; private set; }

		public ElementAssignmentChangeType changeType { get; set; }

		public ControllerPollingInfo pollingInfo { get; set; }

		public ModifierKeyFlags modifierKeyFlags { get; set; }

		public AxisRange AssignedAxisRange
		{
			get
			{
				if (!pollingInfo.success)
				{
					return AxisRange.Positive;
				}
				ControllerElementType elementType = pollingInfo.elementType;
				Pole axisPole = pollingInfo.axisPole;
				AxisRange result = AxisRange.Positive;
				if (elementType == ControllerElementType.Axis)
				{
					result = ((actionType != InputActionType.Axis) ? ((axisPole == Pole.Positive) ? AxisRange.Positive : AxisRange.Negative) : ((!assignFullAxis) ? ((axisPole == Pole.Positive) ? AxisRange.Positive : AxisRange.Negative) : AxisRange.Full));
				}
				return result;
			}
		}

		public string elementName
		{
			get
			{
				if (controllerType == ControllerType.Keyboard && modifierKeyFlags != ModifierKeyFlags.None)
				{
					return $"{Keyboard.ModifierKeyFlagsToString(modifierKeyFlags)} + {pollingInfo.elementIdentifierName}";
				}
				return pollingInfo.elementIdentifierName;
			}
		}

		public ElementAssignmentChange(int playerId, int controllerId, ControllerType controllerType, ControllerMap controllerMap, ElementAssignmentChangeType changeType, int actionElementMapId, int actionId, Pole actionAxisContribution, InputActionType actionType, bool assignFullAxis, bool invert)
			: base(QueueActionType.ElementAssignment)
		{
			this.playerId = playerId;
			this.controllerId = controllerId;
			this.controllerType = controllerType;
			this.controllerMap = controllerMap;
			this.changeType = changeType;
			this.actionElementMapId = actionElementMapId;
			this.actionId = actionId;
			this.actionAxisContribution = actionAxisContribution;
			this.actionType = actionType;
			this.assignFullAxis = assignFullAxis;
			this.invert = invert;
		}

		public ElementAssignmentChange(ElementAssignmentChange source)
			: base(QueueActionType.ElementAssignment)
		{
			playerId = source.playerId;
			controllerId = source.controllerId;
			controllerType = source.controllerType;
			controllerMap = source.controllerMap;
			changeType = source.changeType;
			actionElementMapId = source.actionElementMapId;
			actionId = source.actionId;
			actionAxisContribution = source.actionAxisContribution;
			actionType = source.actionType;
			assignFullAxis = source.assignFullAxis;
			invert = source.invert;
			pollingInfo = source.pollingInfo;
			modifierKeyFlags = source.modifierKeyFlags;
		}

		public void ReplaceOrCreateActionElementMap()
		{
			controllerMap.ReplaceOrCreateElementMap(ToElementAssignment());
		}

		public ElementAssignmentConflictCheck ToElementAssignmentConflictCheck()
		{
			return new ElementAssignmentConflictCheck(playerId, controllerType, controllerId, controllerMap.id, pollingInfo.elementType, pollingInfo.elementIdentifierId, AssignedAxisRange, pollingInfo.keyboardKey, modifierKeyFlags, actionId, actionAxisContribution, invert, actionElementMapId);
		}

		public ElementAssignment ToElementAssignment()
		{
			return new ElementAssignment(controllerType, pollingInfo.elementType, pollingInfo.elementIdentifierId, AssignedAxisRange, pollingInfo.keyboardKey, modifierKeyFlags, actionId, actionAxisContribution, invert, actionElementMapId);
		}
	}

	private class FallbackJoystickIdentification : QueueEntry
	{
		public int joystickId { get; private set; }

		public string joystickName { get; private set; }

		public FallbackJoystickIdentification(int joystickId, string joystickName)
			: base(QueueActionType.FallbackJoystickIdentification)
		{
			this.joystickId = joystickId;
			this.joystickName = joystickName;
		}
	}

	private class Calibration : QueueEntry
	{
		public int selectedElementIdentifierId;

		public bool recording;

		public Player player { get; private set; }

		public ControllerType controllerType { get; private set; }

		public Joystick joystick { get; private set; }

		public CalibrationMap calibrationMap { get; private set; }

		public Calibration(Player player, Joystick joystick, CalibrationMap calibrationMap)
			: base(QueueActionType.Calibrate)
		{
			this.player = player;
			this.joystick = joystick;
			this.calibrationMap = calibrationMap;
			selectedElementIdentifierId = -1;
		}
	}

	private struct WindowProperties
	{
		public int windowId;

		public Rect rect;

		public Action<string, string> windowDrawDelegate;

		public string title;

		public string message;
	}

	private enum QueueActionType
	{
		None,
		JoystickAssignment,
		ElementAssignment,
		FallbackJoystickIdentification,
		Calibrate
	}

	private enum ElementAssignmentChangeType
	{
		Add,
		Replace,
		Remove,
		ReassignOrRemove,
		ConflictCheck
	}

	public enum UserResponse
	{
		Confirm,
		Cancel,
		Custom1,
		Custom2
	}

	private const string playerPrefsBaseKey = "UserRemappingDemo";

	private const float defaultModalWidth = 250f;

	private const float defaultModalHeight = 200f;

	private const float assignmentTimeout = 5f;

	private DialogHelper dialog;

	private bool guiState;

	private bool busy;

	private bool pageGUIState;

	private Player selectedPlayer;

	private int selectedMapCategoryId;

	private ControllerSelection selectedController;

	private ControllerMap selectedMap;

	private bool showMenu;

	private Vector2 actionScrollPos;

	private Vector2 calibrateScrollPos;

	private Queue<QueueEntry> actionQueue;

	private bool setupFinished;

	[NonSerialized]
	private bool initialized;

	private bool isCompiling;

	private GUIStyle style_wordWrap;

	private GUIStyle style_centeredBox;

	private void Awake()
	{
		Initialize();
	}

	private void Initialize()
	{
		dialog = new DialogHelper();
		actionQueue = new Queue<QueueEntry>();
		selectedController = new ControllerSelection();
		ReInput.ControllerConnectedEvent += JoystickConnected;
		ReInput.ControllerPreDisconnectEvent += JoystickPreDisconnect;
		ReInput.ControllerDisconnectedEvent += JoystickDisconnected;
		Reset();
		initialized = true;
		LoadAllMaps();
		if (ReInput.unityJoystickIdentificationRequired)
		{
			IdentifyAllJoysticks();
		}
	}

	private void Setup()
	{
		if (!setupFinished)
		{
			style_wordWrap = new GUIStyle(GUI.skin.label);
			style_wordWrap.wordWrap = true;
			style_centeredBox = new GUIStyle(GUI.skin.box);
			style_centeredBox.alignment = TextAnchor.MiddleCenter;
			setupFinished = true;
		}
	}

	public void OnGUI()
	{
		if (initialized)
		{
			Setup();
			HandleMenuControl();
			if (!showMenu)
			{
				DrawInitialScreen();
				return;
			}
			SetGUIStateStart();
			ProcessQueue();
			DrawPage();
			ShowDialog();
			SetGUIStateEnd();
			busy = false;
		}
	}

	private void HandleMenuControl()
	{
		if (!dialog.enabled && Event.current.type == EventType.Layout && ReInput.players.GetSystemPlayer().GetButtonDown("Menu"))
		{
			if (showMenu)
			{
				SaveAllMaps();
				Close();
			}
			else
			{
				Open();
			}
		}
	}

	private void Close()
	{
		ClearWorkingVars();
		showMenu = false;
	}

	private void Open()
	{
		showMenu = true;
	}

	private void DrawInitialScreen()
	{
		ActionElementMap firstElementMapWithAction = ReInput.players.GetSystemPlayer().controllers.maps.GetFirstElementMapWithAction("Menu", skipDisabledMaps: true);
		GUIContent content = ((firstElementMapWithAction == null) ? new GUIContent("There is no element assigned to open the menu!") : new GUIContent("Press " + firstElementMapWithAction.elementIdentifierName + " to open the menu."));
		GUILayout.BeginArea(GetScreenCenteredRect(300f, 50f));
		GUILayout.Box(content, style_centeredBox, GUILayout.ExpandHeight(expand: true), GUILayout.ExpandWidth(expand: true));
		GUILayout.EndArea();
	}

	private void DrawPage()
	{
		if (GUI.enabled != pageGUIState)
		{
			GUI.enabled = pageGUIState;
		}
		Rect screenRect = new Rect(((float)Screen.width - (float)Screen.width * 0.9f) * 0.5f, ((float)Screen.height - (float)Screen.height * 0.9f) * 0.5f, (float)Screen.width * 0.9f, (float)Screen.height * 0.9f);
		GUILayout.BeginArea(screenRect);
		DrawPlayerSelector();
		DrawJoystickSelector();
		DrawMouseAssignment();
		DrawControllerSelector();
		DrawCalibrateButton();
		DrawMapCategories();
		actionScrollPos = GUILayout.BeginScrollView(actionScrollPos);
		DrawCategoryActions();
		GUILayout.EndScrollView();
		GUILayout.EndArea();
	}

	private void DrawPlayerSelector()
	{
		if (ReInput.players.allPlayerCount == 0)
		{
			GUILayout.Label("There are no players.");
			return;
		}
		GUILayout.Space(15f);
		GUILayout.Label("Players:");
		GUILayout.BeginHorizontal();
		foreach (Player player in ReInput.players.GetPlayers(includeSystemPlayer: true))
		{
			if (selectedPlayer == null)
			{
				selectedPlayer = player;
			}
			bool flag = ((player == selectedPlayer) ? true : false);
			bool flag2 = GUILayout.Toggle(flag, (!(player.descriptiveName != string.Empty)) ? player.name : player.descriptiveName, "Button", GUILayout.ExpandWidth(expand: false));
			if (flag2 != flag && flag2)
			{
				selectedPlayer = player;
				selectedController.Clear();
				selectedMapCategoryId = -1;
			}
		}
		GUILayout.EndHorizontal();
	}

	private void DrawMouseAssignment()
	{
		bool flag = GUI.enabled;
		if (selectedPlayer == null)
		{
			GUI.enabled = false;
		}
		GUILayout.Space(15f);
		GUILayout.Label("Assign Mouse:");
		GUILayout.BeginHorizontal();
		bool flag2 = ((selectedPlayer != null && selectedPlayer.controllers.hasMouse) ? true : false);
		bool flag3 = GUILayout.Toggle(flag2, "Assign Mouse", "Button", GUILayout.ExpandWidth(expand: false));
		if (flag3 != flag2)
		{
			if (flag3)
			{
				selectedPlayer.controllers.hasMouse = true;
				foreach (Player player in ReInput.players.Players)
				{
					if (player != selectedPlayer)
					{
						player.controllers.hasMouse = false;
					}
				}
			}
			else
			{
				selectedPlayer.controllers.hasMouse = false;
			}
		}
		GUILayout.EndHorizontal();
		if (GUI.enabled != flag)
		{
			GUI.enabled = flag;
		}
	}

	private void DrawJoystickSelector()
	{
		bool flag = GUI.enabled;
		if (selectedPlayer == null)
		{
			GUI.enabled = false;
		}
		GUILayout.Space(15f);
		GUILayout.Label("Assign Joysticks:");
		GUILayout.BeginHorizontal();
		bool flag2 = ((selectedPlayer == null || selectedPlayer.controllers.joystickCount == 0) ? true : false);
		bool flag3 = GUILayout.Toggle(flag2, "None", "Button", GUILayout.ExpandWidth(expand: false));
		if (flag3 != flag2)
		{
			selectedPlayer.controllers.ClearControllersOfType(ControllerType.Joystick);
			ControllerSelectionChanged();
		}
		if (selectedPlayer != null)
		{
			foreach (Joystick joystick in ReInput.controllers.Joysticks)
			{
				flag2 = selectedPlayer.controllers.ContainsController(joystick);
				flag3 = GUILayout.Toggle(flag2, joystick.name, "Button", GUILayout.ExpandWidth(expand: false));
				if (flag3 != flag2)
				{
					EnqueueAction(new JoystickAssignmentChange(selectedPlayer.id, joystick.id, flag3));
				}
			}
		}
		GUILayout.EndHorizontal();
		if (GUI.enabled != flag)
		{
			GUI.enabled = flag;
		}
	}

	private void DrawControllerSelector()
	{
		if (selectedPlayer == null)
		{
			return;
		}
		bool flag = GUI.enabled;
		GUILayout.Space(15f);
		GUILayout.Label("Controller to Map:");
		GUILayout.BeginHorizontal();
		if (!selectedController.hasSelection)
		{
			selectedController.Set(0, ControllerType.Keyboard);
			ControllerSelectionChanged();
		}
		bool flag2 = selectedController.type == ControllerType.Keyboard;
		bool flag3 = GUILayout.Toggle(flag2, "Keyboard", "Button", GUILayout.ExpandWidth(expand: false));
		if (flag3 != flag2)
		{
			selectedController.Set(0, ControllerType.Keyboard);
			ControllerSelectionChanged();
		}
		if (!selectedPlayer.controllers.hasMouse)
		{
			GUI.enabled = false;
		}
		flag2 = selectedController.type == ControllerType.Mouse;
		flag3 = GUILayout.Toggle(flag2, "Mouse", "Button", GUILayout.ExpandWidth(expand: false));
		if (flag3 != flag2)
		{
			selectedController.Set(0, ControllerType.Mouse);
			ControllerSelectionChanged();
		}
		if (GUI.enabled != flag)
		{
			GUI.enabled = flag;
		}
		foreach (Joystick joystick in selectedPlayer.controllers.Joysticks)
		{
			flag2 = selectedController.type == ControllerType.Joystick && selectedController.id == joystick.id;
			flag3 = GUILayout.Toggle(flag2, joystick.name, "Button", GUILayout.ExpandWidth(expand: false));
			if (flag3 != flag2)
			{
				selectedController.Set(joystick.id, ControllerType.Joystick);
				ControllerSelectionChanged();
			}
		}
		GUILayout.EndHorizontal();
		if (GUI.enabled != flag)
		{
			GUI.enabled = flag;
		}
	}

	private void DrawCalibrateButton()
	{
		if (selectedPlayer == null)
		{
			return;
		}
		bool flag = GUI.enabled;
		GUILayout.Space(10f);
		Controller controller = ((!selectedController.hasSelection) ? null : selectedPlayer.controllers.GetController(selectedController.type, selectedController.id));
		if (controller == null || selectedController.type != ControllerType.Joystick)
		{
			GUI.enabled = false;
			GUILayout.Button("Select a controller to calibrate", GUILayout.ExpandWidth(expand: false));
			if (GUI.enabled != flag)
			{
				GUI.enabled = flag;
			}
		}
		else if (GUILayout.Button("Calibrate " + controller.name, GUILayout.ExpandWidth(expand: false)) && controller is Joystick { calibrationMap: { } calibrationMap } joystick)
		{
			EnqueueAction(new Calibration(selectedPlayer, joystick, calibrationMap));
		}
		if (GUI.enabled != flag)
		{
			GUI.enabled = flag;
		}
	}

	private void DrawMapCategories()
	{
		if (selectedPlayer == null || !selectedController.hasSelection)
		{
			return;
		}
		bool flag = GUI.enabled;
		GUILayout.Space(15f);
		GUILayout.Label("Categories:");
		GUILayout.BeginHorizontal();
		foreach (InputMapCategory userAssignableMapCategory in ReInput.mapping.UserAssignableMapCategories)
		{
			if (!selectedPlayer.controllers.maps.ContainsMapInCategory(selectedController.type, userAssignableMapCategory.id))
			{
				GUI.enabled = false;
			}
			else if (selectedMapCategoryId < 0)
			{
				selectedMapCategoryId = userAssignableMapCategory.id;
				selectedMap = selectedPlayer.controllers.maps.GetFirstMapInCategory(selectedController.type, selectedController.id, userAssignableMapCategory.id);
			}
			bool flag2 = ((userAssignableMapCategory.id == selectedMapCategoryId) ? true : false);
			bool flag3 = GUILayout.Toggle(flag2, (!(userAssignableMapCategory.descriptiveName != string.Empty)) ? userAssignableMapCategory.name : userAssignableMapCategory.descriptiveName, "Button", GUILayout.ExpandWidth(expand: false));
			if (flag3 != flag2)
			{
				selectedMapCategoryId = userAssignableMapCategory.id;
				selectedMap = selectedPlayer.controllers.maps.GetFirstMapInCategory(selectedController.type, selectedController.id, userAssignableMapCategory.id);
			}
			if (GUI.enabled != flag)
			{
				GUI.enabled = flag;
			}
		}
		GUILayout.EndHorizontal();
		if (GUI.enabled != flag)
		{
			GUI.enabled = flag;
		}
	}

	private void DrawCategoryActions()
	{
		if (selectedPlayer == null || selectedMapCategoryId < 0)
		{
			return;
		}
		bool flag = GUI.enabled;
		if (selectedMap == null)
		{
			return;
		}
		GUILayout.Space(15f);
		GUILayout.Label("Actions:");
		InputMapCategory mapCategory = ReInput.mapping.GetMapCategory(selectedMapCategoryId);
		if (mapCategory == null)
		{
			return;
		}
		InputCategory actionCategory = ReInput.mapping.GetActionCategory(mapCategory.name);
		if (actionCategory == null)
		{
			return;
		}
		float width = 150f;
		foreach (InputAction item in ReInput.mapping.ActionsInCategory(actionCategory.id))
		{
			string text = ((!(item.descriptiveName != string.Empty)) ? item.name : item.descriptiveName);
			if (item.type == InputActionType.Button)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label(text, GUILayout.Width(width));
				DrawAddActionMapButton(selectedPlayer.id, item, Pole.Positive, selectedController, selectedMap, assignFullAxis: true);
				foreach (ActionElementMap allMap in selectedMap.AllMaps)
				{
					if (allMap.actionId == item.id)
					{
						DrawActionAssignmentButton(selectedPlayer.id, item, Pole.Positive, selectedController, selectedMap, assignFullAxis: true, allMap);
					}
				}
				GUILayout.EndHorizontal();
			}
			else
			{
				if (item.type != InputActionType.Axis)
				{
					continue;
				}
				if (selectedController.type != ControllerType.Keyboard)
				{
					GUILayout.BeginHorizontal();
					GUILayout.Label(text, GUILayout.Width(width));
					DrawAddActionMapButton(selectedPlayer.id, item, Pole.Positive, selectedController, selectedMap, assignFullAxis: true);
					foreach (ActionElementMap allMap2 in selectedMap.AllMaps)
					{
						if (allMap2.actionId == item.id && allMap2.elementType != ControllerElementType.Button && allMap2.axisType != AxisType.Split)
						{
							DrawActionAssignmentButton(selectedPlayer.id, item, Pole.Positive, selectedController, selectedMap, assignFullAxis: true, allMap2);
							DrawInvertButton(selectedPlayer.id, item, Pole.Positive, selectedController, selectedMap, allMap2);
						}
					}
					GUILayout.EndHorizontal();
				}
				string text2 = ((!(item.positiveDescriptiveName != string.Empty)) ? (item.descriptiveName + " +") : item.positiveDescriptiveName);
				GUILayout.BeginHorizontal();
				GUILayout.Label(text2, GUILayout.Width(width));
				DrawAddActionMapButton(selectedPlayer.id, item, Pole.Positive, selectedController, selectedMap, assignFullAxis: false);
				foreach (ActionElementMap allMap3 in selectedMap.AllMaps)
				{
					if (allMap3.actionId == item.id && allMap3.axisContribution == Pole.Positive && allMap3.axisType != AxisType.Normal)
					{
						DrawActionAssignmentButton(selectedPlayer.id, item, Pole.Positive, selectedController, selectedMap, assignFullAxis: false, allMap3);
					}
				}
				GUILayout.EndHorizontal();
				string text3 = ((!(item.negativeDescriptiveName != string.Empty)) ? (item.descriptiveName + " -") : item.negativeDescriptiveName);
				GUILayout.BeginHorizontal();
				GUILayout.Label(text3, GUILayout.Width(width));
				DrawAddActionMapButton(selectedPlayer.id, item, Pole.Negative, selectedController, selectedMap, assignFullAxis: false);
				foreach (ActionElementMap allMap4 in selectedMap.AllMaps)
				{
					if (allMap4.actionId == item.id && allMap4.axisContribution == Pole.Negative && allMap4.axisType != AxisType.Normal)
					{
						DrawActionAssignmentButton(selectedPlayer.id, item, Pole.Negative, selectedController, selectedMap, assignFullAxis: false, allMap4);
					}
				}
				GUILayout.EndHorizontal();
			}
		}
		if (GUI.enabled != flag)
		{
			GUI.enabled = flag;
		}
	}

	private void DrawActionAssignmentButton(int playerId, InputAction action, Pole actionAxisContribution, ControllerSelection controller, ControllerMap controllerMap, bool assignFullAxis, ActionElementMap elementMap)
	{
		if (GUILayout.Button(elementMap.elementIdentifierName, GUILayout.ExpandWidth(expand: false), GUILayout.MinWidth(30f)))
		{
			EnqueueAction(new ElementAssignmentChange(playerId, controller.id, controller.type, controllerMap, ElementAssignmentChangeType.ReassignOrRemove, elementMap.id, action.id, actionAxisContribution, action.type, assignFullAxis, elementMap.invert));
		}
		GUILayout.Space(4f);
	}

	private void DrawInvertButton(int playerId, InputAction action, Pole actionAxisContribution, ControllerSelection controller, ControllerMap controllerMap, ActionElementMap elementMap)
	{
		bool invert = elementMap.invert;
		bool flag = GUILayout.Toggle(invert, "Invert", GUILayout.ExpandWidth(expand: false));
		if (flag != invert)
		{
			elementMap.invert = flag;
		}
		GUILayout.Space(10f);
	}

	private void DrawAddActionMapButton(int playerId, InputAction action, Pole actionAxisContribution, ControllerSelection controller, ControllerMap controllerMap, bool assignFullAxis)
	{
		if (GUILayout.Button("Add...", GUILayout.ExpandWidth(expand: false)))
		{
			EnqueueAction(new ElementAssignmentChange(playerId, controller.id, controller.type, controllerMap, ElementAssignmentChangeType.Add, -1, action.id, actionAxisContribution, action.type, assignFullAxis, invert: false));
		}
		GUILayout.Space(10f);
	}

	private void ShowDialog()
	{
		dialog.Update();
	}

	private void DrawModalWindow(string title, string message)
	{
		if (dialog.enabled)
		{
			GUILayout.Space(5f);
			GUILayout.Label(message, style_wordWrap);
			GUILayout.FlexibleSpace();
			GUILayout.BeginHorizontal();
			dialog.DrawConfirmButton("Okay");
			GUILayout.FlexibleSpace();
			dialog.DrawCancelButton();
			GUILayout.EndHorizontal();
		}
	}

	private void DrawModalWindow_OkayOnly(string title, string message)
	{
		if (dialog.enabled)
		{
			GUILayout.Space(5f);
			GUILayout.Label(message, style_wordWrap);
			GUILayout.FlexibleSpace();
			GUILayout.BeginHorizontal();
			dialog.DrawConfirmButton("Okay");
			GUILayout.EndHorizontal();
		}
	}

	private void DrawElementAssignmentWindow(string title, string message)
	{
		if (dialog.enabled)
		{
			GUILayout.Space(5f);
			GUILayout.Label(message, style_wordWrap);
			GUILayout.FlexibleSpace();
			if (!(actionQueue.Peek() is ElementAssignmentChange entry))
			{
				dialog.Cancel();
				return;
			}
			PollControllerForAssignment(entry);
			GUILayout.Label("Assignment will be canceled in " + (int)Mathf.Ceil(dialog.closeTimer)/*cast due to .constrained prefix*/ + "...", style_wordWrap);
		}
	}

	private void DrawElementAssignmentProtectedConflictWindow(string title, string message)
	{
		if (dialog.enabled)
		{
			GUILayout.Space(5f);
			GUILayout.Label(message, style_wordWrap);
			GUILayout.FlexibleSpace();
			if (!(actionQueue.Peek() is ElementAssignmentChange))
			{
				dialog.Cancel();
				return;
			}
			GUILayout.BeginHorizontal();
			dialog.DrawConfirmButton(UserResponse.Custom1, "Add");
			GUILayout.FlexibleSpace();
			dialog.DrawCancelButton();
			GUILayout.EndHorizontal();
		}
	}

	private void DrawElementAssignmentNormalConflictWindow(string title, string message)
	{
		if (dialog.enabled)
		{
			GUILayout.Space(5f);
			GUILayout.Label(message, style_wordWrap);
			GUILayout.FlexibleSpace();
			if (!(actionQueue.Peek() is ElementAssignmentChange))
			{
				dialog.Cancel();
				return;
			}
			GUILayout.BeginHorizontal();
			dialog.DrawConfirmButton(UserResponse.Confirm, "Replace");
			GUILayout.FlexibleSpace();
			dialog.DrawConfirmButton(UserResponse.Custom1, "Add");
			GUILayout.FlexibleSpace();
			dialog.DrawCancelButton();
			GUILayout.EndHorizontal();
		}
	}

	private void DrawReassignOrRemoveElementAssignmentWindow(string title, string message)
	{
		if (dialog.enabled)
		{
			GUILayout.Space(5f);
			GUILayout.Label(message, style_wordWrap);
			GUILayout.FlexibleSpace();
			GUILayout.BeginHorizontal();
			dialog.DrawConfirmButton("Reassign");
			GUILayout.FlexibleSpace();
			dialog.DrawCancelButton("Remove");
			GUILayout.EndHorizontal();
		}
	}

	private void DrawFallbackJoystickIdentificationWindow(string title, string message)
	{
		if (!dialog.enabled)
		{
			return;
		}
		if (!(actionQueue.Peek() is FallbackJoystickIdentification fallbackJoystickIdentification))
		{
			dialog.Cancel();
			return;
		}
		GUILayout.Space(5f);
		GUILayout.Label(message, style_wordWrap);
		GUILayout.Label("Press any button or axis on \"" + fallbackJoystickIdentification.joystickName + "\" now.", style_wordWrap);
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("Skip"))
		{
			dialog.Cancel();
		}
		else if (!dialog.busy && ReInput.controllers.SetUnityJoystickIdFromAnyButtonOrAxisPress(fallbackJoystickIdentification.joystickId, 0.8f, positiveAxesOnly: false))
		{
			dialog.Confirm();
		}
	}

	private void DrawCalibrationWindow(string title, string message)
	{
		if (!dialog.enabled)
		{
			return;
		}
		if (!(actionQueue.Peek() is Calibration calibration))
		{
			dialog.Cancel();
			return;
		}
		GUILayout.Space(5f);
		GUILayout.Label(message, style_wordWrap);
		GUILayout.Space(20f);
		GUILayout.BeginHorizontal();
		bool flag = GUI.enabled;
		GUILayout.BeginVertical(GUILayout.Width(200f));
		calibrateScrollPos = GUILayout.BeginScrollView(calibrateScrollPos);
		if (calibration.recording)
		{
			GUI.enabled = false;
		}
		IList<ControllerElementIdentifier> axisElementIdentifiers = calibration.joystick.AxisElementIdentifiers;
		for (int i = 0; i < axisElementIdentifiers.Count; i++)
		{
			ControllerElementIdentifier controllerElementIdentifier = axisElementIdentifiers[i];
			bool flag2 = calibration.selectedElementIdentifierId == controllerElementIdentifier.id;
			bool flag3 = GUILayout.Toggle(flag2, controllerElementIdentifier.name, "Button", GUILayout.ExpandWidth(expand: false));
			if (flag2 != flag3)
			{
				calibration.selectedElementIdentifierId = controllerElementIdentifier.id;
			}
		}
		if (GUI.enabled != flag)
		{
			GUI.enabled = flag;
		}
		GUILayout.EndScrollView();
		GUILayout.EndVertical();
		GUILayout.BeginVertical(GUILayout.Width(200f));
		if (calibration.selectedElementIdentifierId >= 0)
		{
			float axisRawById = calibration.joystick.GetAxisRawById(calibration.selectedElementIdentifierId);
			GUILayout.Label("Raw Value: " + axisRawById);
			int axisIndexById = calibration.joystick.GetAxisIndexById(calibration.selectedElementIdentifierId);
			AxisCalibration axis = calibration.calibrationMap.GetAxis(axisIndexById);
			GUILayout.Label("Calibrated Value: " + calibration.joystick.GetAxisById(calibration.selectedElementIdentifierId));
			GUILayout.Label("Zero: " + axis.calibratedZero);
			GUILayout.Label("Min: " + axis.calibratedMin);
			GUILayout.Label("Max: " + axis.calibratedMax);
			GUILayout.Label("Dead Zone: " + axis.deadZone);
			GUILayout.Space(15f);
			bool flag4 = GUILayout.Toggle(axis.enabled, "Enabled", "Button", GUILayout.ExpandWidth(expand: false));
			if (axis.enabled != flag4)
			{
				axis.enabled = flag4;
			}
			GUILayout.Space(10f);
			bool flag5 = GUILayout.Toggle(calibration.recording, "Record Min/Max", "Button", GUILayout.ExpandWidth(expand: false));
			if (flag5 != calibration.recording)
			{
				if (flag5)
				{
					axis.calibratedMax = 0f;
					axis.calibratedMin = 0f;
				}
				calibration.recording = flag5;
			}
			if (calibration.recording)
			{
				axis.calibratedMin = Mathf.Min(axis.calibratedMin, axisRawById, axis.calibratedMin);
				axis.calibratedMax = Mathf.Max(axis.calibratedMax, axisRawById, axis.calibratedMax);
				GUI.enabled = false;
			}
			if (GUILayout.Button("Set Zero", GUILayout.ExpandWidth(expand: false)))
			{
				axis.calibratedZero = axisRawById;
			}
			if (GUILayout.Button("Set Dead Zone", GUILayout.ExpandWidth(expand: false)))
			{
				axis.deadZone = axisRawById;
			}
			bool flag6 = GUILayout.Toggle(axis.invert, "Invert", "Button", GUILayout.ExpandWidth(expand: false));
			if (axis.invert != flag6)
			{
				axis.invert = flag6;
			}
			GUILayout.Space(10f);
			if (GUILayout.Button("Reset", GUILayout.ExpandWidth(expand: false)))
			{
				axis.Reset();
			}
			if (GUI.enabled != flag)
			{
				GUI.enabled = flag;
			}
		}
		else
		{
			GUILayout.Label("Select an axis to begin.");
		}
		GUILayout.EndVertical();
		GUILayout.EndHorizontal();
		GUILayout.FlexibleSpace();
		if (calibration.recording)
		{
			GUI.enabled = false;
		}
		if (GUILayout.Button("Close"))
		{
			calibrateScrollPos = default(Vector2);
			dialog.Confirm();
		}
		if (GUI.enabled != flag)
		{
			GUI.enabled = flag;
		}
	}

	private void DialogResultCallback(int queueActionId, UserResponse response)
	{
		foreach (QueueEntry item in actionQueue)
		{
			if (item.id != queueActionId)
			{
				continue;
			}
			if (response != UserResponse.Cancel)
			{
				item.Confirm(response);
			}
			else
			{
				item.Cancel();
			}
			break;
		}
	}

	private void PollControllerForAssignment(ElementAssignmentChange entry)
	{
		if (!dialog.busy)
		{
			switch (entry.controllerType)
			{
			case ControllerType.Keyboard:
				PollKeyboardForAssignment(entry);
				break;
			case ControllerType.Joystick:
				PollJoystickForAssignment(entry);
				break;
			case ControllerType.Mouse:
				PollMouseForAssignment(entry);
				break;
			}
		}
	}

	private void PollKeyboardForAssignment(ElementAssignmentChange entry)
	{
		int num = 0;
		ControllerPollingInfo pollingInfo = default(ControllerPollingInfo);
		ControllerPollingInfo pollingInfo2 = default(ControllerPollingInfo);
		ModifierKeyFlags modifierKeyFlags = ModifierKeyFlags.None;
		foreach (ControllerPollingInfo item in ReInput.controllers.Keyboard.PollForAllKeys())
		{
			KeyCode keyboardKey = item.keyboardKey;
			if (keyboardKey == KeyCode.AltGr)
			{
				continue;
			}
			if (Keyboard.IsModifierKey(item.keyboardKey))
			{
				if (num == 0)
				{
					pollingInfo2 = item;
				}
				modifierKeyFlags |= Keyboard.KeyCodeToModifierKeyFlags(keyboardKey);
				num++;
			}
			else if (pollingInfo.keyboardKey == KeyCode.None)
			{
				pollingInfo = item;
			}
		}
		if (pollingInfo.keyboardKey != KeyCode.None)
		{
			if (num == 0)
			{
				entry.pollingInfo = pollingInfo;
				dialog.Confirm();
			}
			else
			{
				entry.pollingInfo = pollingInfo;
				entry.modifierKeyFlags = modifierKeyFlags;
				dialog.Confirm();
			}
		}
		else
		{
			if (num <= 0)
			{
				return;
			}
			dialog.StartCloseTimer(5f);
			if (num == 1)
			{
				if (ReInput.controllers.Keyboard.GetKeyTimePressed(pollingInfo2.keyboardKey) > 1f)
				{
					entry.pollingInfo = pollingInfo2;
					dialog.Confirm();
				}
				else
				{
					GUILayout.Label(Keyboard.GetKeyName(pollingInfo2.keyboardKey));
				}
			}
			else
			{
				GUILayout.Label(Keyboard.ModifierKeyFlagsToString(modifierKeyFlags));
			}
		}
	}

	private void PollJoystickForAssignment(ElementAssignmentChange entry)
	{
		Player player = ReInput.players.GetPlayer(entry.playerId);
		if (player == null)
		{
			dialog.Cancel();
			return;
		}
		entry.pollingInfo = player.controllers.polling.PollControllerForFirstElementDown(entry.controllerType, entry.controllerId);
		if (entry.pollingInfo.success)
		{
			dialog.Confirm();
		}
	}

	private void PollMouseForAssignment(ElementAssignmentChange entry)
	{
		Player player = ReInput.players.GetPlayer(entry.playerId);
		if (player == null)
		{
			dialog.Cancel();
			return;
		}
		entry.pollingInfo = player.controllers.polling.PollControllerForFirstElementDown(entry.controllerType, entry.controllerId);
		if (entry.pollingInfo.success)
		{
			dialog.Confirm();
		}
	}

	private Rect GetScreenCenteredRect(float width, float height)
	{
		return new Rect((float)Screen.width * 0.5f - width * 0.5f, (float)((double)Screen.height * 0.5 - (double)(height * 0.5f)), width, height);
	}

	private void EnqueueAction(QueueEntry entry)
	{
		if (entry != null)
		{
			busy = true;
			GUI.enabled = false;
			actionQueue.Enqueue(entry);
		}
	}

	private void ProcessQueue()
	{
		if (dialog.enabled || busy || actionQueue.Count == 0)
		{
			return;
		}
		while (actionQueue.Count > 0)
		{
			QueueEntry queueEntry = actionQueue.Peek();
			bool flag = false;
			switch (queueEntry.queueActionType)
			{
			case QueueActionType.JoystickAssignment:
				flag = ProcessJoystickAssignmentChange((JoystickAssignmentChange)queueEntry);
				break;
			case QueueActionType.ElementAssignment:
				flag = ProcessElementAssignmentChange((ElementAssignmentChange)queueEntry);
				break;
			case QueueActionType.FallbackJoystickIdentification:
				flag = ProcessFallbackJoystickIdentification((FallbackJoystickIdentification)queueEntry);
				break;
			case QueueActionType.Calibrate:
				flag = ProcessCalibration((Calibration)queueEntry);
				break;
			}
			if (!flag)
			{
				break;
			}
			actionQueue.Dequeue();
		}
	}

	private bool ProcessJoystickAssignmentChange(JoystickAssignmentChange entry)
	{
		if (entry.state == QueueEntry.State.Canceled)
		{
			return true;
		}
		Player player = ReInput.players.GetPlayer(entry.playerId);
		if (player == null)
		{
			return true;
		}
		if (!entry.assign)
		{
			player.controllers.RemoveController(ControllerType.Joystick, entry.joystickId);
			ControllerSelectionChanged();
			return true;
		}
		if (player.controllers.ContainsController(ControllerType.Joystick, entry.joystickId))
		{
			return true;
		}
		if (!ReInput.controllers.IsJoystickAssigned(entry.joystickId) || entry.state == QueueEntry.State.Confirmed)
		{
			player.controllers.AddController(ControllerType.Joystick, entry.joystickId, removeFromOtherPlayers: true);
			ControllerSelectionChanged();
			return true;
		}
		dialog.StartModal(entry.id, DialogHelper.DialogType.JoystickConflict, new WindowProperties
		{
			title = "Joystick Reassignment",
			message = "This joystick is already assigned to another player. Do you want to reassign this joystick to " + player.descriptiveName + "?",
			rect = GetScreenCenteredRect(250f, 200f),
			windowDrawDelegate = DrawModalWindow
		}, DialogResultCallback);
		return false;
	}

	private bool ProcessElementAssignmentChange(ElementAssignmentChange entry)
	{
		switch (entry.changeType)
		{
		case ElementAssignmentChangeType.ReassignOrRemove:
			return ProcessRemoveOrReassignElementAssignment(entry);
		case ElementAssignmentChangeType.Remove:
			return ProcessRemoveElementAssignment(entry);
		case ElementAssignmentChangeType.Add:
		case ElementAssignmentChangeType.Replace:
			return ProcessAddOrReplaceElementAssignment(entry);
		case ElementAssignmentChangeType.ConflictCheck:
			return ProcessElementAssignmentConflictCheck(entry);
		default:
			throw new NotImplementedException();
		}
	}

	private bool ProcessRemoveOrReassignElementAssignment(ElementAssignmentChange entry)
	{
		if (entry.controllerMap == null)
		{
			return true;
		}
		if (entry.state == QueueEntry.State.Canceled)
		{
			ElementAssignmentChange elementAssignmentChange = new ElementAssignmentChange(entry);
			elementAssignmentChange.changeType = ElementAssignmentChangeType.Remove;
			actionQueue.Enqueue(elementAssignmentChange);
			return true;
		}
		if (entry.state == QueueEntry.State.Confirmed)
		{
			ElementAssignmentChange elementAssignmentChange2 = new ElementAssignmentChange(entry);
			elementAssignmentChange2.changeType = ElementAssignmentChangeType.Replace;
			actionQueue.Enqueue(elementAssignmentChange2);
			return true;
		}
		dialog.StartModal(entry.id, DialogHelper.DialogType.AssignElement, new WindowProperties
		{
			title = "Reassign or Remove",
			message = "Do you want to reassign or remove this assignment?",
			rect = GetScreenCenteredRect(250f, 200f),
			windowDrawDelegate = DrawReassignOrRemoveElementAssignmentWindow
		}, DialogResultCallback);
		return false;
	}

	private bool ProcessRemoveElementAssignment(ElementAssignmentChange entry)
	{
		if (entry.controllerMap == null)
		{
			return true;
		}
		if (entry.state == QueueEntry.State.Canceled)
		{
			return true;
		}
		if (entry.state == QueueEntry.State.Confirmed)
		{
			entry.controllerMap.DeleteElementMap(entry.actionElementMapId);
			return true;
		}
		dialog.StartModal(entry.id, DialogHelper.DialogType.DeleteAssignmentConfirmation, new WindowProperties
		{
			title = "Remove Assignment",
			message = "Are you sure you want to remove this assignment?",
			rect = GetScreenCenteredRect(250f, 200f),
			windowDrawDelegate = DrawModalWindow
		}, DialogResultCallback);
		return false;
	}

	private bool ProcessAddOrReplaceElementAssignment(ElementAssignmentChange entry)
	{
		Player player = ReInput.players.GetPlayer(entry.playerId);
		if (player == null)
		{
			return true;
		}
		if (entry.controllerMap == null)
		{
			return true;
		}
		if (entry.state == QueueEntry.State.Canceled)
		{
			return true;
		}
		if (entry.state == QueueEntry.State.Confirmed)
		{
			if (Event.current.type != EventType.Layout)
			{
				return false;
			}
			if (!ReInput.controllers.conflictChecking.DoesElementAssignmentConflict(entry.ToElementAssignmentConflictCheck()))
			{
				entry.ReplaceOrCreateActionElementMap();
			}
			else
			{
				ElementAssignmentChange elementAssignmentChange = new ElementAssignmentChange(entry);
				elementAssignmentChange.changeType = ElementAssignmentChangeType.ConflictCheck;
				actionQueue.Enqueue(elementAssignmentChange);
			}
			return true;
		}
		string text;
		if (entry.controllerType != ControllerType.Keyboard)
		{
			text = ((entry.controllerType != ControllerType.Mouse) ? "Press any button or axis to assign it to this action." : "Press any mouse button or axis to assign it to this action.\n\nTo assign mouse movement axes, move the mouse quickly in the direction you want mapped to the action. Slow movements will be ignored.");
		}
		else
		{
			text = ((Application.platform != RuntimePlatform.OSXEditor && Application.platform != RuntimePlatform.OSXPlayer) ? "Press any key to assign it to this action. You may also use the modifier keys Control, Alt, and Shift. If you wish to assign a modifier key itself to this action, press and hold the key for 1 second." : "Press any key to assign it to this action. You may also use the modifier keys Command, Control, Alt, and Shift. If you wish to assign a modifier key itself to this action, press and hold the key for 1 second.");
			if (Application.isEditor)
			{
				text += "\n\nNOTE: Some modifier key combinations will not work in the Unity Editor, but they will work in a game build.";
			}
		}
		dialog.StartModal(entry.id, DialogHelper.DialogType.AssignElement, new WindowProperties
		{
			title = "Assign",
			message = text,
			rect = GetScreenCenteredRect(250f, 200f),
			windowDrawDelegate = DrawElementAssignmentWindow
		}, DialogResultCallback, 5f);
		return false;
	}

	private bool ProcessElementAssignmentConflictCheck(ElementAssignmentChange entry)
	{
		Player player = ReInput.players.GetPlayer(entry.playerId);
		if (player == null)
		{
			return true;
		}
		if (entry.controllerMap == null)
		{
			return true;
		}
		if (entry.state == QueueEntry.State.Canceled)
		{
			return true;
		}
		if (entry.state == QueueEntry.State.Confirmed)
		{
			entry.changeType = ElementAssignmentChangeType.Add;
			if (entry.response == UserResponse.Confirm)
			{
				ReInput.controllers.conflictChecking.RemoveElementAssignmentConflicts(entry.ToElementAssignmentConflictCheck());
				entry.ReplaceOrCreateActionElementMap();
			}
			else
			{
				if (entry.response != UserResponse.Custom1)
				{
					throw new NotImplementedException();
				}
				entry.ReplaceOrCreateActionElementMap();
			}
			return true;
		}
		bool flag = false;
		foreach (ElementAssignmentConflictInfo item in ReInput.controllers.conflictChecking.ElementAssignmentConflicts(entry.ToElementAssignmentConflictCheck()))
		{
			if (!item.isUserAssignable)
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			string message = entry.elementName + " is already in use and is protected from reassignment. You cannot remove the protected assignment, but you can still assign the action to this element. If you do so, the element will trigger multiple actions when activated.";
			dialog.StartModal(entry.id, DialogHelper.DialogType.AssignElement, new WindowProperties
			{
				title = "Assignment Conflict",
				message = message,
				rect = GetScreenCenteredRect(250f, 200f),
				windowDrawDelegate = DrawElementAssignmentProtectedConflictWindow
			}, DialogResultCallback);
		}
		else
		{
			string message2 = entry.elementName + " is already in use. You may replace the other conflicting assignments, add this assignment anyway which will leave multiple actions assigned to this element, or cancel this assignment.";
			dialog.StartModal(entry.id, DialogHelper.DialogType.AssignElement, new WindowProperties
			{
				title = "Assignment Conflict",
				message = message2,
				rect = GetScreenCenteredRect(250f, 200f),
				windowDrawDelegate = DrawElementAssignmentNormalConflictWindow
			}, DialogResultCallback);
		}
		return false;
	}

	private bool ProcessFallbackJoystickIdentification(FallbackJoystickIdentification entry)
	{
		if (entry.state == QueueEntry.State.Canceled)
		{
			return true;
		}
		if (entry.state == QueueEntry.State.Confirmed)
		{
			return true;
		}
		dialog.StartModal(entry.id, DialogHelper.DialogType.JoystickConflict, new WindowProperties
		{
			title = "Joystick Identification Required",
			message = "A joystick has been attached or removed. You will need to identify each joystick by pressing a button on the controller listed below:",
			rect = GetScreenCenteredRect(250f, 200f),
			windowDrawDelegate = DrawFallbackJoystickIdentificationWindow
		}, DialogResultCallback, 0f, 1f);
		return false;
	}

	private bool ProcessCalibration(Calibration entry)
	{
		if (entry.state == QueueEntry.State.Canceled)
		{
			return true;
		}
		if (entry.state == QueueEntry.State.Confirmed)
		{
			return true;
		}
		dialog.StartModal(entry.id, DialogHelper.DialogType.JoystickConflict, new WindowProperties
		{
			title = "Calibrate Controller",
			message = "Select an axis to calibrate on the " + entry.joystick.name + ".",
			rect = GetScreenCenteredRect(450f, 480f),
			windowDrawDelegate = DrawCalibrationWindow
		}, DialogResultCallback);
		return false;
	}

	private void PlayerSelectionChanged()
	{
		ClearControllerSelection();
	}

	private void ControllerSelectionChanged()
	{
		ClearMapSelection();
	}

	private void ClearControllerSelection()
	{
		selectedController.Clear();
		ClearMapSelection();
	}

	private void ClearMapSelection()
	{
		selectedMapCategoryId = -1;
		selectedMap = null;
	}

	private void Reset()
	{
		ClearWorkingVars();
		initialized = false;
		showMenu = false;
	}

	private void ClearWorkingVars()
	{
		selectedPlayer = null;
		ClearMapSelection();
		selectedController.Clear();
		actionScrollPos = default(Vector2);
		dialog.FullReset();
		actionQueue.Clear();
		busy = false;
	}

	private void SetGUIStateStart()
	{
		guiState = true;
		if (busy)
		{
			guiState = false;
		}
		pageGUIState = guiState && !busy && !dialog.enabled && !dialog.busy;
		if (GUI.enabled != guiState)
		{
			GUI.enabled = guiState;
		}
	}

	private void SetGUIStateEnd()
	{
		guiState = true;
		if (!GUI.enabled)
		{
			GUI.enabled = guiState;
		}
	}

	private void JoystickConnected(ControllerStatusChangedEventArgs args)
	{
		LoadJoystickMaps(args.controllerId);
		if (ReInput.unityJoystickIdentificationRequired)
		{
			IdentifyAllJoysticks();
		}
	}

	private void JoystickPreDisconnect(ControllerStatusChangedEventArgs args)
	{
		if (selectedController.hasSelection && args.controllerType == selectedController.type && args.controllerId == selectedController.id)
		{
			ClearControllerSelection();
		}
		if (showMenu)
		{
			SaveJoystickMaps(args.controllerId);
		}
	}

	private void JoystickDisconnected(ControllerStatusChangedEventArgs args)
	{
		if (showMenu)
		{
			ClearWorkingVars();
		}
		if (ReInput.unityJoystickIdentificationRequired)
		{
			IdentifyAllJoysticks();
		}
	}

	private void LoadAllMaps()
	{
		IList<Player> allPlayers = ReInput.players.AllPlayers;
		for (int i = 0; i < allPlayers.Count; i++)
		{
			Player player = allPlayers[i];
			IList<InputBehavior> inputBehaviors = ReInput.mapping.GetInputBehaviors(player.id);
			for (int j = 0; j < inputBehaviors.Count; j++)
			{
				string inputBehaviorXml = GetInputBehaviorXml(player, inputBehaviors[j].id);
				if (inputBehaviorXml != null && !(inputBehaviorXml == string.Empty))
				{
					inputBehaviors[j].ImportXmlString(inputBehaviorXml);
				}
			}
			List<string> allControllerMapsXml = GetAllControllerMapsXml(player, userAssignableMapsOnly: true, ControllerType.Keyboard, ReInput.controllers.Keyboard);
			List<string> allControllerMapsXml2 = GetAllControllerMapsXml(player, userAssignableMapsOnly: true, ControllerType.Mouse, ReInput.controllers.Mouse);
			bool flag = false;
			List<List<string>> list = new List<List<string>>();
			foreach (Joystick joystick in player.controllers.Joysticks)
			{
				List<string> allControllerMapsXml3 = GetAllControllerMapsXml(player, userAssignableMapsOnly: true, ControllerType.Joystick, joystick);
				list.Add(allControllerMapsXml3);
				if (allControllerMapsXml3.Count > 0)
				{
					flag = true;
				}
			}
			if (allControllerMapsXml.Count > 0)
			{
				player.controllers.maps.ClearMaps(ControllerType.Keyboard, userAssignableOnly: true);
			}
			player.controllers.maps.AddMapsFromXml(ControllerType.Keyboard, 0, allControllerMapsXml);
			if (flag)
			{
				player.controllers.maps.ClearMaps(ControllerType.Joystick, userAssignableOnly: true);
			}
			int num = 0;
			foreach (Joystick joystick2 in player.controllers.Joysticks)
			{
				player.controllers.maps.AddMapsFromXml(ControllerType.Joystick, joystick2.id, list[num]);
				num++;
			}
			if (allControllerMapsXml2.Count > 0)
			{
				player.controllers.maps.ClearMaps(ControllerType.Mouse, userAssignableOnly: true);
			}
			player.controllers.maps.AddMapsFromXml(ControllerType.Mouse, 0, allControllerMapsXml2);
		}
		foreach (Joystick joystick3 in ReInput.controllers.Joysticks)
		{
			joystick3.ImportCalibrationMapFromXmlString(GetJoystickCalibrationMapXml(joystick3));
		}
	}

	private void SaveAllMaps()
	{
		IList<Player> allPlayers = ReInput.players.AllPlayers;
		for (int i = 0; i < allPlayers.Count; i++)
		{
			Player player = allPlayers[i];
			PlayerSaveData saveData = player.GetSaveData(userAssignableMapsOnly: true);
			InputBehavior[] inputBehaviors = saveData.inputBehaviors;
			foreach (InputBehavior inputBehavior in inputBehaviors)
			{
				string inputBehaviorPlayerPrefsKey = GetInputBehaviorPlayerPrefsKey(player, inputBehavior);
				PlayerPrefs.SetString(inputBehaviorPlayerPrefsKey, inputBehavior.ToXmlString());
			}
			foreach (ControllerMapSaveData allControllerMapSaveDatum in saveData.AllControllerMapSaveData)
			{
				string controllerMapPlayerPrefsKey = GetControllerMapPlayerPrefsKey(player, allControllerMapSaveDatum);
				PlayerPrefs.SetString(controllerMapPlayerPrefsKey, allControllerMapSaveDatum.map.ToXmlString());
			}
		}
		foreach (Joystick joystick in ReInput.controllers.Joysticks)
		{
			JoystickCalibrationMapSaveData calibrationMapSaveData = joystick.GetCalibrationMapSaveData();
			string joystickCalibrationMapPlayerPrefsKey = GetJoystickCalibrationMapPlayerPrefsKey(calibrationMapSaveData);
			PlayerPrefs.SetString(joystickCalibrationMapPlayerPrefsKey, calibrationMapSaveData.map.ToXmlString());
		}
		PlayerPrefs.Save();
	}

	private void LoadJoystickMaps(int joystickId)
	{
		IList<Player> allPlayers = ReInput.players.AllPlayers;
		for (int i = 0; i < allPlayers.Count; i++)
		{
			Player player = allPlayers[i];
			if (!player.controllers.ContainsController(ControllerType.Joystick, joystickId))
			{
				continue;
			}
			Joystick controller = player.controllers.GetController<Joystick>(joystickId);
			if (controller != null)
			{
				List<string> allControllerMapsXml = GetAllControllerMapsXml(player, userAssignableMapsOnly: true, ControllerType.Joystick, controller);
				if (allControllerMapsXml.Count != 0)
				{
					player.controllers.maps.ClearMaps(ControllerType.Joystick, userAssignableOnly: true);
					player.controllers.maps.AddMapsFromXml(ControllerType.Joystick, joystickId, allControllerMapsXml);
					controller.ImportCalibrationMapFromXmlString(GetJoystickCalibrationMapXml(controller));
				}
			}
		}
	}

	private void SaveJoystickMaps(int joystickId)
	{
		IList<Player> allPlayers = ReInput.players.AllPlayers;
		for (int i = 0; i < allPlayers.Count; i++)
		{
			Player player = allPlayers[i];
			if (!player.controllers.ContainsController(ControllerType.Joystick, joystickId))
			{
				continue;
			}
			JoystickMapSaveData[] mapSaveData = player.controllers.maps.GetMapSaveData<JoystickMapSaveData>(joystickId, userAssignableMapsOnly: true);
			string controllerMapPlayerPrefsKey;
			if (mapSaveData != null)
			{
				for (int j = 0; j < mapSaveData.Length; j++)
				{
					controllerMapPlayerPrefsKey = GetControllerMapPlayerPrefsKey(player, mapSaveData[j]);
					PlayerPrefs.SetString(controllerMapPlayerPrefsKey, mapSaveData[j].map.ToXmlString());
				}
			}
			Joystick controller = player.controllers.GetController<Joystick>(joystickId);
			JoystickCalibrationMapSaveData calibrationMapSaveData = controller.GetCalibrationMapSaveData();
			controllerMapPlayerPrefsKey = GetJoystickCalibrationMapPlayerPrefsKey(calibrationMapSaveData);
			PlayerPrefs.SetString(controllerMapPlayerPrefsKey, calibrationMapSaveData.map.ToXmlString());
		}
		IList<Joystick> joysticks = ReInput.controllers.Joysticks;
		for (int k = 0; k < joysticks.Count; k++)
		{
			JoystickCalibrationMapSaveData calibrationMapSaveData2 = joysticks[k].GetCalibrationMapSaveData();
			string joystickCalibrationMapPlayerPrefsKey = GetJoystickCalibrationMapPlayerPrefsKey(calibrationMapSaveData2);
			PlayerPrefs.SetString(joystickCalibrationMapPlayerPrefsKey, calibrationMapSaveData2.map.ToXmlString());
		}
	}

	private string GetBasePlayerPrefsKey(Player player)
	{
		string text = "UserRemappingDemo";
		return text + "|playerName=" + player.name;
	}

	private string GetControllerMapPlayerPrefsKey(Player player, ControllerMapSaveData saveData)
	{
		string basePlayerPrefsKey = GetBasePlayerPrefsKey(player);
		basePlayerPrefsKey += "|dataType=ControllerMap";
		basePlayerPrefsKey = basePlayerPrefsKey + "|controllerMapType=" + saveData.mapTypeString;
		string text = basePlayerPrefsKey;
		basePlayerPrefsKey = text + "|categoryId=" + saveData.map.categoryId + "|layoutId=" + saveData.map.layoutId;
		basePlayerPrefsKey = basePlayerPrefsKey + "|hardwareIdentifier=" + saveData.controllerHardwareIdentifier;
		if (saveData.mapType == typeof(JoystickMap))
		{
			basePlayerPrefsKey = basePlayerPrefsKey + "|hardwareGuid=" + ((JoystickMapSaveData)saveData).joystickHardwareTypeGuid.ToString();
		}
		return basePlayerPrefsKey;
	}

	private string GetControllerMapXml(Player player, ControllerType controllerType, int categoryId, int layoutId, Controller controller)
	{
		string basePlayerPrefsKey = GetBasePlayerPrefsKey(player);
		basePlayerPrefsKey += "|dataType=ControllerMap";
		basePlayerPrefsKey = basePlayerPrefsKey + "|controllerMapType=" + controller.mapTypeString;
		string text = basePlayerPrefsKey;
		basePlayerPrefsKey = text + "|categoryId=" + categoryId + "|layoutId=" + layoutId;
		basePlayerPrefsKey = basePlayerPrefsKey + "|hardwareIdentifier=" + controller.hardwareIdentifier;
		if (controllerType == ControllerType.Joystick)
		{
			Joystick joystick = (Joystick)controller;
			basePlayerPrefsKey = basePlayerPrefsKey + "|hardwareGuid=" + joystick.hardwareTypeGuid.ToString();
		}
		if (!PlayerPrefs.HasKey(basePlayerPrefsKey))
		{
			return string.Empty;
		}
		return PlayerPrefs.GetString(basePlayerPrefsKey);
	}

	private List<string> GetAllControllerMapsXml(Player player, bool userAssignableMapsOnly, ControllerType controllerType, Controller controller)
	{
		List<string> list = new List<string>();
		IList<InputMapCategory> mapCategories = ReInput.mapping.MapCategories;
		for (int i = 0; i < mapCategories.Count; i++)
		{
			InputMapCategory inputMapCategory = mapCategories[i];
			if (userAssignableMapsOnly && !inputMapCategory.userAssignable)
			{
				continue;
			}
			IList<InputLayout> list2 = ReInput.mapping.MapLayouts(controllerType);
			for (int j = 0; j < list2.Count; j++)
			{
				InputLayout inputLayout = list2[j];
				string controllerMapXml = GetControllerMapXml(player, controllerType, inputMapCategory.id, inputLayout.id, controller);
				if (!(controllerMapXml == string.Empty))
				{
					list.Add(controllerMapXml);
				}
			}
		}
		return list;
	}

	private string GetJoystickCalibrationMapPlayerPrefsKey(JoystickCalibrationMapSaveData saveData)
	{
		string text = "UserRemappingDemo";
		text += "|dataType=CalibrationMap";
		text = text + "|controllerType=" + saveData.controllerType;
		text = text + "|hardwareIdentifier=" + saveData.hardwareIdentifier;
		return text + "|hardwareGuid=" + saveData.joystickHardwareTypeGuid.ToString();
	}

	private string GetJoystickCalibrationMapXml(Joystick joystick)
	{
		string text = "UserRemappingDemo";
		text += "|dataType=CalibrationMap";
		text = text + "|controllerType=" + joystick.type;
		text = text + "|hardwareIdentifier=" + joystick.hardwareIdentifier;
		text = text + "|hardwareGuid=" + joystick.hardwareTypeGuid.ToString();
		if (!PlayerPrefs.HasKey(text))
		{
			return string.Empty;
		}
		return PlayerPrefs.GetString(text);
	}

	private string GetInputBehaviorPlayerPrefsKey(Player player, InputBehavior saveData)
	{
		string basePlayerPrefsKey = GetBasePlayerPrefsKey(player);
		basePlayerPrefsKey += "|dataType=InputBehavior";
		return basePlayerPrefsKey + "|id=" + saveData.id;
	}

	private string GetInputBehaviorXml(Player player, int id)
	{
		string basePlayerPrefsKey = GetBasePlayerPrefsKey(player);
		basePlayerPrefsKey += "|dataType=InputBehavior";
		basePlayerPrefsKey = basePlayerPrefsKey + "|id=" + id;
		if (!PlayerPrefs.HasKey(basePlayerPrefsKey))
		{
			return string.Empty;
		}
		return PlayerPrefs.GetString(basePlayerPrefsKey);
	}

	public void IdentifyAllJoysticks()
	{
		if (ReInput.controllers.joystickCount == 0)
		{
			return;
		}
		ClearWorkingVars();
		Open();
		foreach (Joystick joystick in ReInput.controllers.Joysticks)
		{
			actionQueue.Enqueue(new FallbackJoystickIdentification(joystick.id, joystick.name));
		}
	}

	protected void CheckRecompile()
	{
	}

	private void RecompileWindow(int windowId)
	{
	}
}
