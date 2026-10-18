using System;
using System.Collections.Generic;
using Brix.Engine;
using Brix.External.Factories;
using Brix.External.Signals;
using Brix.Game.Components.Skybox;
using Brix.Game.Rules;
using Brix.Lifecycle.Pooling;
using Brix.Lua;
using Brix.UI.GameSelector;
using Brix.Utils;
using Brix.Utils.Factories;
using Brix.Utils.Signals;
using MoonSharp.Interpreter;
using Newtonsoft.Json;
using Rewired;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Input;

[JsonObject(MemberSerialization.OptIn)]
public class CameraScript : MonoBehaviour
{
	public Bounds BoundsLimit;

	public LayerMask CollisionLayerMask;

	[SerializeField]
	public DampVector UpperVerticalBound;

	[SerializeField]
	public DampVector LowerVerticalBound;

	[SerializeField]
	public DampVector BottomCollisionVector;

	[SerializeField]
	public DampVector ZoomInVector;

	[SerializeField]
	public DampVector ZoomOutVector;

	private float _camOrientation;

	public static Signal OnSetCameraWorldUpRotationSignal = new Signal();

	public static Signal OnSetCameraLevelSignal = new Signal();

	public static CameraScript Instance;

	public static float ScrollFactor = 1f;

	public float DampMaxSpeed;

	public bool FreeCam;

	public float FixCam;

	public float Distance;

	public float AngleOfView;

	[LuaDrivable("hugTerrain")]
	public bool HugTerrain;

	public float BounceSpeed;

	[LuaDrivable("translationSpeedMod")]
	public Vector3 TranslationSpeedMod = Vector3.one;

	public Vector2 RotationSpeedMod = Vector2.one;

	public float ZoomSpeedMod = 1f;

	public GameObject GizmoTarget;

	public GameObject GizmoLevel;

	public GameObject CursorNormal;

	public GameObject CursorMove;

	public GameObject CursorRotate;

	[LuaDrivable("targetLocation")]
	public static Vector3 Target;

	public float MovementSpeed;

	public Timer DeltaTimer = new Timer();

	public float AverageDelta;

	public Classeur<float> DeltaArray = new Classeur<float>();

	public bool Cinematique;

	public bool MenuBloquant;

	public float ActiveDelai;

	public bool WaitForLevelReady = true;

	public bool AutoAdjustWhenIdle;

	public float FocusMaxDistanceBeforeTeleport;

	public float Attenuation;

	public bool UseCameraBounds;

	[LuaDrivable("canMove")]
	public bool CanMove = true;

	[LuaDrivable("canRotate")]
	public bool RotationActive = true;

	[LuaDrivable("canElevate")]
	public bool ClimbActive = true;

	[LuaDrivable("canZoom")]
	public bool ZoomActive = true;

	public bool PivotAroundTarget = true;

	public float AutoHugDelay;

	public bool pivotCanGoUnderground;

	private float _gizmoEnabled;

	private static Vector3 _moveVector;

	private static bool _moving;

	private static bool _rotating;

	private Vector3 _camSpeed;

	private Vector2 _rotSpeed;

	private float _zoomSpeed;

	private float _climbSpeed;

	private CompCache<CameraRules> _camRules;

	private Player _player;

	private bool _wasPivotingAroundTarget;

	private float _worldHeight;

	private float lastManualClimb;

	private float lastClimbHeight;

	private float _zoomInput;

	private float _autoClimbSpeed = 30f;

	private Vector2 _newRotationVector;

	private DampVector savedLowerVerticalBound;

	private DampVector savedUpperVerticalBound;

	private DampVector savedBottomCollisionVector;

	private DampVector savedZoomInBound;

	private DampVector savedZoomOutBound;

	private DampVector savedZoomOutBoundEditor;

	private bool worldEditorCamera;

	private float mouseY;

	private float mouseSensitivity = 0.5f;

	private float rotationDelta;

	private Vector3 XAxisRotation;

	private Vector3 recordedPosition;

	private Quaternion recordedRotation;

	[LuaDrivable("timelapseZoom")]
	public float timelapseZoomSpeed;

	[LuaDrivable("timelapseLatitudinalRotation")]
	public float timelapseRotationLatitudinal;

	[LuaDrivable("timelapseLongitudinalRotation")]
	public float timelapseRotationLongitudinal;

	[LuaDrivable("timelapseTranslationVector")]
	public Vector3 timelapseTranslationVec = new Vector3(0f, 0f, 0f);

	[LuaDrivable("FPSmode")]
	public bool FPSmode;

	[LuaDrivable("chosenBricktron")]
	private GameObject chosenBricktron;

	private Component[] gizmoRenderers;

	protected Signal Destroyed = new Signal();

	public bool editorCamera;

	public bool timelapseActivated;

	private float startedHoldingRotate;

	public float CamOrientation
	{
		get
		{
			return _camOrientation;
		}
		set
		{
			if (!_camOrientation.Equals(value))
			{
				_camOrientation = value;
			}
		}
	}

	[JsonProperty]
	public float EloignementEtPosition
	{
		get
		{
			return Distance;
		}
		set
		{
			Vector3 position = base.transform.position;
			Quaternion rotation = base.transform.rotation;
			Positionner(position, rotation, value);
		}
	}

	public bool ShouldAutoAdjustWhenIdle => !editorCamera && !worldEditorCamera && !InputModeController.IsInEditMode(InputModeController.GetCurrentMode());

	private bool HideGizmo
	{
		get
		{
			if (Cinematique || MenuBloquant)
			{
				GizmoTarget.transform.GetComponent<Renderer>().enabled = false;
				GizmoTarget.GetComponent<LineRenderer>().enabled = false;
				return true;
			}
			return false;
		}
	}

	public void UpdateCameraFPS()
	{
		if (chosenBricktron != null)
		{
			rotationDelta = base.transform.rotation.eulerAngles.x;
			Positionner(chosenBricktron.transform.position + new Vector3(0f, 1.5f, 0f), chosenBricktron.transform.rotation);
			mouseY = ReInput.controllers.Mouse.GetAxis(1) * mouseSensitivity;
			XAxisRotation = new Vector3(0f - mouseY + rotationDelta, 0f, 0f);
			base.transform.Rotate(XAxisRotation);
		}
	}

	public void SetTimelapseTranslationVector(float x, float y, float z)
	{
		timelapseTranslationVec = new Vector3(x, y, z);
	}

	public void SetTimelapseRotation(float rotLat, float rotLong)
	{
		timelapseRotationLatitudinal = rotLat;
		timelapseRotationLongitudinal = rotLong;
	}

	public void SetTimelapseZoom(float zoom)
	{
		timelapseZoomSpeed = zoom;
	}

	public void RegisterPosRot()
	{
		recordedPosition = base.transform.position;
		recordedRotation = base.transform.rotation;
	}

	public void RepositionCam()
	{
		Positionner(recordedPosition, recordedRotation);
	}

	private void GetAllGizmoRenderers()
	{
		gizmoRenderers = GetComponentsInChildren<MeshRenderer>(includeInactive: true);
	}

	public void ActivateGizmoRenderers()
	{
		Component[] array = gizmoRenderers;
		for (int i = 0; i < array.Length; i++)
		{
			MeshRenderer meshRenderer = (MeshRenderer)array[i];
			meshRenderer.enabled = true;
		}
		GizmoTarget.transform.GetComponent<Renderer>().enabled = true;
		GizmoTarget.GetComponent<LineRenderer>().enabled = true;
		Cinematique = false;
	}

	public void DeactivateGizmoRenderers()
	{
		Component[] array = gizmoRenderers;
		for (int i = 0; i < array.Length; i++)
		{
			MeshRenderer meshRenderer = (MeshRenderer)array[i];
			meshRenderer.enabled = false;
		}
		GizmoTarget.transform.GetComponent<Renderer>().enabled = false;
		GizmoTarget.GetComponent<LineRenderer>().enabled = false;
		Cinematique = true;
	}

	public void ToggleHideGizmos()
	{
		if (Cinematique)
		{
			ActivateGizmoRenderers();
		}
		else
		{
			DeactivateGizmoRenderers();
		}
	}

	private void Start()
	{
		_wasPivotingAroundTarget = PivotAroundTarget;
		FactorySignalInvoker.OnFactoriesReadySig.LateConnectOnce(GenerateSkybox);
		_moveVector = Vector3.zero;
		_player = ReInput.players.GetPlayer("local");
		if (WaitForLevelReady)
		{
			GameSignals.DisableUntil(GameSignals.LevelReady, this);
		}
		GameSignals.Connect(GameSignals.OnEditorModeChanged, OnEditorModeChanged);
		GameSignals.Connect(InputModeController.InputModeChanged, OnInputModeChanged);
		Instance = this;
		savedZoomOutBound = ZoomOutVector;
		savedZoomOutBoundEditor = ZoomOutVector;
		savedZoomOutBoundEditor.Limit = 300f;
		GetAllGizmoRenderers();
		timelapseTranslationVec = new Vector3(0f, 0f, 0f);
	}

	private void OnInputModeChanged(InputMode mode)
	{
		AutoAdjustWhenIdle = ShouldAutoAdjustWhenIdle;
	}

	private void OnEditorModeChanged(bool value)
	{
		ToggleEditorCamera(value);
	}

	private void OnDestroy()
	{
		Destroyed.Invoke();
	}

	private void Update()
	{
		if (FPSmode)
		{
			UpdateCameraFPS();
			return;
		}
		float y = base.transform.position.y;
		displayCurseur(0);
		AverageDelta = DeltaTimer.smoothDeltaTime();
		ReadSpecialKeys();
		if (!timelapseActivated)
		{
			GetMovementVectorsFromInputs();
		}
		else
		{
			GetMovementVectorsFromTimelapse();
		}
		ApplySmoothDeltaToCamSpeed();
		if (!editorCamera)
		{
			DampCameraAngle();
			TestBottomCollision();
			ApplyZoomBounce();
			ClampCameraSpeed();
			DampZoom();
			ApplyZoomBounce();
			ClampCameraSpeed();
			ClampCameraAngle();
			ApplyCameraAngleBounce();
		}
		ApplyInputToVariables();
		ClampDistance();
		if (UseCameraBounds)
		{
			Target = FitVector3InsideBounds(Target);
		}
		Target = FitVector3InsideBounds(Target);
		MoveTargetIfPivotChanged();
		ApplyChangesToTransform();
		UpdateDistanceFromGround();
		if (!Cinematique)
		{
			UpdateGizmo();
		}
		ApplyAutoClimbToTarget();
		if (y != base.transform.position.y && UIGameObserver.lua_OnSetCameraLevel != null)
		{
			LuaHookBase.InvokeFromCLR(UIGameObserver.lua_OnSetCameraLevel);
		}
		_wasPivotingAroundTarget = PivotAroundTarget;
	}

	private void SetZoomOutLimit(int value)
	{
		ZoomOutVector.Limit = value;
	}

	private void SetZoomInLimit(int value)
	{
		ZoomInVector.Limit = value;
	}

	public Vector3 GetTravelSpeed()
	{
		return TranslationSpeedMod;
	}

	public void SetTravelSpeed(float _x, float _y, float _z)
	{
		TranslationSpeedMod = new Vector3(_x, _y, _z);
	}

	public float GetOrbitSpeed()
	{
		return RotationSpeedMod.x;
	}

	public void SetOrbitSpeed(float value)
	{
		RotationSpeedMod = new Vector3(value, value, value);
	}

	public float GetZoomSpeed()
	{
		return ZoomSpeedMod;
	}

	public void SetZoomSpeed(float value)
	{
		ZoomSpeedMod = value;
	}

	public void ToggleEditorCamera(bool value)
	{
		if (value)
		{
			EnableEditorCamera();
		}
		else
		{
			DisableEditorCamera();
		}
	}

	public void ToggleTimelapse()
	{
		timelapseActivated = !timelapseActivated;
		if (!editorCamera && timelapseActivated)
		{
			EnableEditorCamera();
		}
	}

	public void EnableWorldEditorCamera()
	{
		savedLowerVerticalBound = LowerVerticalBound;
		savedUpperVerticalBound = UpperVerticalBound;
		savedZoomInBound = ZoomInVector;
		ZoomOutVector = savedZoomOutBoundEditor;
		savedBottomCollisionVector = BottomCollisionVector;
		LowerVerticalBound.BounceThreshold = 0f;
		UpperVerticalBound.BounceThreshold = 0f;
		BottomCollisionVector.BounceThreshold = 0f;
		ZoomInVector.BounceThreshold = 0f;
		ZoomInVector.Limit = 5f;
		ZoomOutVector.Limit = 300f;
		ZoomOutVector.BounceThreshold = 0f;
		worldEditorCamera = true;
		HugTerrain = false;
		AutoAdjustWhenIdle = ShouldAutoAdjustWhenIdle;
		pivotCanGoUnderground = true;
		_autoClimbSpeed = 100f;
	}

	private void EnableEditorCamera()
	{
		if (!editorCamera)
		{
			savedLowerVerticalBound = LowerVerticalBound;
			savedUpperVerticalBound = UpperVerticalBound;
			savedZoomInBound = ZoomInVector;
			ZoomOutVector = savedZoomOutBoundEditor;
			savedBottomCollisionVector = BottomCollisionVector;
			LowerVerticalBound.BounceThreshold = 0f;
			UpperVerticalBound.BounceThreshold = 0f;
			BottomCollisionVector.BounceThreshold = 0f;
			ZoomInVector.Limit = 1f;
			ZoomInVector.BounceThreshold = 0f;
			editorCamera = true;
			pivotCanGoUnderground = true;
			AutoAdjustWhenIdle = false;
			HugTerrain = false;
		}
	}

	private void DisableEditorCamera()
	{
		if (editorCamera)
		{
			LowerVerticalBound = savedLowerVerticalBound;
			UpperVerticalBound = savedUpperVerticalBound;
			BottomCollisionVector = savedBottomCollisionVector;
			ZoomInVector = savedZoomInBound;
			ZoomOutVector = savedZoomOutBound;
			editorCamera = false;
			pivotCanGoUnderground = false;
			AutoAdjustWhenIdle = ShouldAutoAdjustWhenIdle;
			HugTerrain = true;
		}
	}

	private void TestBottomCollision()
	{
		Ray ray = new Ray(base.transform.position, Vector3.down);
		if (Physics.Raycast(ray, out var hitInfo, Mathf.Max(base.transform.position.y, 0.1f), CollisionLayerMask, QueryTriggerInteraction.Ignore))
		{
			ApplyDampFromBottomCollision(BottomCollisionVector, hitInfo);
		}
	}

	private float ScaleValueOnDistance(float value)
	{
		double num = Math.PI * 30.0;
		double num2 = Math.PI * 2.0 * (double)Distance;
		double num3 = (double)value / num;
		double num4 = num2 * num3;
		return (float)num4;
	}

	private void ApplyDampFromBottomCollision(DampVector dampVector, RaycastHit hitInfo)
	{
		float num = Vector3.Distance(hitInfo.point, base.transform.position);
		dampVector.Limit = ScaleValueOnDistance(dampVector.Limit);
		dampVector.DampThreshold = ScaleValueOnDistance(dampVector.DampThreshold);
		dampVector.BounceThreshold = ScaleValueOnDistance(dampVector.BounceThreshold);
		if (num <= dampVector.Limit && _newRotationVector.y > 0f)
		{
			_rotSpeed.y = 0f;
			_newRotationVector.y = 0f;
		}
		if (num <= dampVector.DampThreshold && _newRotationVector.y > 0f)
		{
			ApplyRotationDamp(dampVector, num);
		}
		if (num <= dampVector.BounceThreshold)
		{
			ApplyCollisionBounce(dampVector, num);
		}
	}

	private void ApplyZoomBounce()
	{
		if (_zoomInput == 0f)
		{
			float b = Mathf.Clamp(Distance, ZoomInVector.Limit + ZoomInVector.BounceThreshold, ZoomOutVector.Limit - ZoomOutVector.BounceThreshold);
			Distance = Mathf.Lerp(Distance, b, AverageDelta);
		}
	}

	private void ApplyCollisionBounce(DampVector dampVector, float distanceFromHit)
	{
		if (!_rotating)
		{
			double num = Math.PI * 2.0 * (double)Distance;
			double num2 = (double)((dampVector.BounceThreshold - distanceFromHit) * 360f) / num;
			AngleOfView = Mathf.LerpAngle(AngleOfView, AngleOfView + (float)num2, AverageDelta);
		}
	}

	private void ResetAutoClimb()
	{
		lastManualClimb = Time.fixedTime;
		lastClimbHeight = Target.y;
	}

	private void ForceTerrainHug()
	{
		HugTerrain = true;
	}

	private void ReadSpecialKeys()
	{
		if (ReInput.isReady)
		{
			if (_player.GetButtonDown("cam_toggle_autoground") && _worldHeight != 0f)
			{
				ForceTerrainHug();
			}
			if (_player.GetButtonDown("cam_focus_target"))
			{
				FocusOnTarget();
			}
		}
	}

	private void FocusOnTarget()
	{
		List<GameObject> gos = new List<GameObject>();
		UIGameSelector.GetSelectedInTrack(ref gos, SelectionTrackID.Command);
		GameObject gameObject = gos.Slinq().FirstOrDefault();
		if ((bool)gameObject)
		{
			if (Vector3.Distance(gameObject.transform.position, Target) > FocusMaxDistanceBeforeTeleport)
			{
				ChangeTarget(gameObject.transform);
				return;
			}
			Target = gameObject.transform.position;
			Quaternion r = Quaternion.LookRotation(gameObject.transform.position - base.transform.position);
			LookAt(r);
			ResetVectors();
		}
	}

	private void ChangeTarget(Transform target)
	{
		Target = target.position;
		ResetVectors();
		LookAt(FindBestAngle(target));
	}

	private void ResetVectors()
	{
		_rotSpeed = Vector2.zero;
		_camSpeed = Vector3.zero;
		_moveVector = Vector2.zero;
	}

	public Quaternion FindBestAngle(Transform t)
	{
		int num = 45;
		Quaternion rotation = base.transform.rotation;
		Quaternion quaternion = rotation * Quaternion.Euler(num, 0f, 0f);
		Quaternion result = quaternion;
		float num2 = -1000000f;
		for (int i = 0; i < 8; i++)
		{
			for (int j = 0; j < 6; j++)
			{
				Quaternion quaternion2 = Quaternion.Euler(j * 90 / 6, i * 360 / 8, 0f);
				float num3 = (0f - Quaternion.Angle(quaternion, quaternion2)) / 10f;
				float distance = Distance;
				float num4 = distance / 10f;
				Vector3 vector = quaternion2 * Vector3.forward;
				Vector3 vector2 = t.position + vector * (0f - distance) + Vector3.up * num4;
				vector = (t.position - vector2).normalized;
				distance = (t.position - vector2).magnitude;
				if (Physics.SphereCast(vector2, num4, vector, out var hitInfo, distance))
				{
					distance = hitInfo.distance;
				}
				num3 += distance;
				if (num3 > num2)
				{
					num2 = num3;
					result = quaternion2;
				}
			}
		}
		return result;
	}

	private void ApplyAutoClimbToTarget()
	{
		if (ClimbActive && (HugTerrain || (AutoAdjustWhenIdle && lastManualClimb + AutoHugDelay < Time.fixedTime)) && _worldHeight != 0f)
		{
			float num = ((!(lastClimbHeight - _worldHeight < 0f)) ? _autoClimbSpeed : (0f - _autoClimbSpeed));
			float num2 = (0f - num) * AverageDelta;
			Target.y += num2;
			if ((num < 0f && Target.y - _worldHeight > 0f) || (num > 0f && Target.y - _worldHeight < 0f))
			{
				Target.y = _worldHeight;
				ResetAutoClimb();
				HugTerrain = AutoAdjustWhenIdle;
			}
		}
	}

	private void GetMovementVectorsFromInputs()
	{
		_rotating = false;
		_newRotationVector = Vector2.zero;
		float num = 0.05f * AverageDelta * 100f;
		float num2 = 0.5f * AverageDelta * 10f;
		float num3 = 8f * AverageDelta * 60f;
		float num4 = 8f * AverageDelta * 60f;
		Vector2 vector = new Vector2(ReInput.controllers.Mouse.GetAxis(0), ReInput.controllers.Mouse.GetAxis(1));
		Quaternion quaternion = Quaternion.Euler(0f, CamOrientation, 0f);
		float num5 = MovementSpeed * num * Attenuation * (AverageDelta * 60f);
		if (ReInput.isReady)
		{
			Vector3 vector2 = quaternion * Vector3.forward;
			Vector3 vector3 = quaternion * Vector3.right;
			if (!_moving)
			{
				if (_player.GetButton("cam_move_forward"))
				{
					_moveVector += Vector3.forward * TranslationSpeedMod.z;
					_moving = true;
				}
				if (_player.GetButton("cam_move_backward"))
				{
					_moveVector += Vector3.back * TranslationSpeedMod.z;
					_moving = true;
				}
				if (_player.GetButton("cam_move_left"))
				{
					_moveVector += Vector3.left * TranslationSpeedMod.x;
					_moving = true;
				}
				if (_player.GetButton("cam_move_right"))
				{
					_moveVector += Vector3.right * TranslationSpeedMod.x;
					_moving = true;
				}
			}
			if (_player.GetButton("cam_mouse_movement") && _player.GetButton("cam_action1"))
			{
				_camSpeed -= vector3 * (vector.x * num * Distance * Attenuation) * 0.2f;
				_camSpeed -= vector2 * (vector.y * num * Distance * Attenuation) * 0.2f;
				_moving = true;
			}
		}
		if (_moving)
		{
			_camSpeed += quaternion * _moveVector * num5 * Distance;
			_moveVector = Vector3.zero;
			ResetAutoClimb();
			_moving = false;
			displayCurseur(1);
		}
		if (ReInput.isReady)
		{
			if (_player.GetButton("cam_rotate_up"))
			{
				_newRotationVector += new Vector2(0f, -150f) * num5 * RotationSpeedMod.x;
				_rotating = true;
			}
			if (_player.GetButton("cam_rotate_down"))
			{
				_newRotationVector += new Vector2(0f, 150f) * num5 * RotationSpeedMod.x;
				_rotating = true;
			}
			if (_player.GetButton("cam_rotate_left"))
			{
				_newRotationVector += new Vector2(150f, 0f) * num5 * RotationSpeedMod.x;
				_rotating = true;
			}
			if (_player.GetButton("cam_rotate_right"))
			{
				_newRotationVector += new Vector2(-150f, 0f) * num5 * RotationSpeedMod.x;
				_rotating = true;
			}
			if (_player.GetButton("cam_mouse_rotate") || (_player.GetButton("cam_mouse_movement") && _player.GetButton("cam_action2")))
			{
				_newRotationVector = new Vector2(0f - vector.x, vector.y) * num2;
				_rotating = true;
			}
		}
		if (_rotating)
		{
			displayCurseur(2);
		}
		if (!ReInput.isReady)
		{
			return;
		}
		if (_player.GetButton("cam_mouse_alt"))
		{
			float num6 = _player.GetAxis("cam_scrollwheel") * 0.1f;
			if (num6 != 0f)
			{
				_climbSpeed += num6 * ScrollFactor * (Distance * 0.005f * num4) * TranslationSpeedMod.y;
				ResetAutoClimb();
				HugTerrain = false;
			}
		}
		else
		{
			float num7 = _player.GetAxis("cam_scrollwheel") * 0.1f;
			if (num7 != 0f)
			{
				_zoomInput -= num7 * ScrollFactor * (Distance * 0.02f * num3) * ZoomSpeedMod;
			}
		}
		if (_player.GetButton("cam_zoom_in"))
		{
			_zoomInput += 15f * num3 * num5 * ZoomSpeedMod;
		}
		if (_player.GetButton("cam_zoom_out"))
		{
			_zoomInput -= 15f * num3 * num5 * ZoomSpeedMod;
		}
		if (_player.GetButton("cam_move_up"))
		{
			_climbSpeed += Distance * 0.15f * num4 * num5 * TranslationSpeedMod.y;
			HugTerrain = false;
		}
		if (_player.GetButton("cam_move_down"))
		{
			_climbSpeed -= Distance * 0.15f * num4 * num5 * TranslationSpeedMod.y;
			HugTerrain = false;
		}
	}

	private void GetMovementVectorsFromTimelapse()
	{
		_rotating = false;
		_newRotationVector = Vector2.zero;
		float num = 0.016f;
		float num2 = 0.05f * AverageDelta * 100f;
		float num3 = 8f * AverageDelta * 60f;
		Quaternion quaternion = Quaternion.Euler(0f, CamOrientation, 0f);
		float num4 = MovementSpeed * num2 * Attenuation * (AverageDelta * 60f);
		if (!_moving)
		{
			_moveVector += timelapseTranslationVec * TranslationSpeedMod.x * num;
			_moving = true;
		}
		if (_moving)
		{
			_camSpeed += quaternion * _moveVector * num4 * Distance;
			_moveVector = Vector3.zero;
			ResetAutoClimb();
			_moving = false;
			displayCurseur(1);
		}
		_newRotationVector += new Vector2(timelapseRotationLatitudinal, timelapseRotationLongitudinal) * RotationSpeedMod.x * 0.1f * num;
		_rotating = true;
		if (_rotating)
		{
			displayCurseur(1);
		}
		float num5 = 0.001f;
		_zoomInput += num5 * ScrollFactor * (Distance * 0.02f * num3) * timelapseZoomSpeed * num;
	}

	private void UpdateGizmo()
	{
		if (_camSpeed != Vector3.zero)
		{
			_gizmoEnabled = Time.time;
		}
		GizmoTarget.GetComponent<LineRenderer>().enabled = _gizmoEnabled + 2f > Time.time;
		GizmoTarget.transform.position = Target;
		GizmoTarget.transform.rotation = Quaternion.identity;
		GizmoTarget.GetComponent<LineRenderer>().SetPosition(1, new Vector3(0f, _worldHeight - Target.y, 0f));
		Vector3 position = GizmoTarget.transform.position;
		position.y = 0f;
	}

	private void ApplyChangesToTransform()
	{
		Vector3 vector = CalculateTopTriangle(Target);
		Vector3 position = ((!PivotAroundTarget) ? Target : vector);
		Quaternion rotation = Quaternion.Euler(AngleOfView, CamOrientation, 0f);
		base.transform.position = position;
		base.transform.rotation = rotation;
	}

	private void MoveTargetIfPivotChanged()
	{
		if (_wasPivotingAroundTarget && !PivotAroundTarget)
		{
			Target = base.transform.position;
		}
		else if (!_wasPivotingAroundTarget && PivotAroundTarget)
		{
			Vector3 vector = CalculateTopTriangle(Target);
			Target -= vector - Target;
		}
	}

	private void ClampDistance()
	{
		Distance = Mathf.Clamp(Distance, ZoomInVector.Limit, ZoomOutVector.Limit);
	}

	private void DampZoom()
	{
		float num = Mathf.Abs(ZoomInVector.Limit - Distance);
		if (num <= ZoomInVector.DampThreshold)
		{
			if (_zoomInput < 0f)
			{
				ApplyZoomDamp(ZoomInVector);
			}
			if (Math.Abs(_zoomSpeed) > DampMaxSpeed && _zoomSpeed < 0f)
			{
				_zoomSpeed = DampMaxSpeed * (_zoomSpeed / Math.Abs(_zoomSpeed));
			}
		}
		float num2 = Mathf.Abs(ZoomOutVector.Limit - Distance);
		if (num2 <= ZoomOutVector.DampThreshold)
		{
			if (_zoomInput > 0f)
			{
				ApplyZoomDamp(ZoomOutVector);
			}
			if (Math.Abs(_zoomSpeed) > DampMaxSpeed && _zoomSpeed > 0f)
			{
				_zoomSpeed = DampMaxSpeed * (_zoomSpeed / Math.Abs(_zoomSpeed));
			}
		}
	}

	private void ApplyZoomDamp(DampVector zoomDampVector)
	{
		float num = Mathf.Abs(Distance - zoomDampVector.Limit);
		float num2 = num / Mathf.Abs(zoomDampVector.DampThreshold);
		if (float.IsNaN(num2))
		{
			num2 = 0f;
		}
		_zoomInput = Mathf.LerpAngle(0f, _zoomInput, num2 * num2);
	}

	private void ApplyInputToVariables()
	{
		_rotSpeed -= _newRotationVector;
		_zoomSpeed += _zoomInput;
		Target += _camSpeed;
		AngleOfView += _rotSpeed.y;
		CamOrientation += _rotSpeed.x;
		if (pivotCanGoUnderground || !(Target.y - _worldHeight <= 0f) || !(_climbSpeed < 0f))
		{
			Target.y += _climbSpeed;
		}
		Distance += _zoomSpeed;
		_zoomInput = 0f;
	}

	private void ClampCameraSpeed()
	{
		_camSpeed = new Vector3(Mathf.Clamp(_camSpeed.x, -5f, 5f), Mathf.Clamp(_camSpeed.y, -5f, 5f), Mathf.Clamp(_camSpeed.z, -5f, 5f));
		_rotSpeed = new Vector2(Mathf.Clamp(_rotSpeed.x, -5f, 5f), Mathf.Clamp(_rotSpeed.y, -5f, 5f));
		_climbSpeed = Mathf.Clamp(_climbSpeed, -5f, 5f);
		_zoomSpeed = Mathf.Clamp(_zoomSpeed, -5f, 5f);
		if (!CanMove)
		{
			_camSpeed = Vector3.zero;
		}
		if (!RotationActive)
		{
			_rotSpeed = Vector2.zero;
			_newRotationVector = Vector2.zero;
		}
		if (!ClimbActive)
		{
			_climbSpeed = 0f;
		}
		if (!ZoomActive)
		{
			_zoomInput = 0f;
			_zoomSpeed = 0f;
		}
	}

	private void ApplySmoothDeltaToCamSpeed()
	{
		float num = Mathf.Pow(0.9f, AverageDelta * 100f);
		_camSpeed *= num;
		_rotSpeed *= num;
		_zoomSpeed *= num;
		_climbSpeed *= num;
	}

	private void DampCameraAngle()
	{
		if (IsInsideUpperAngleDampZone(UpperVerticalBound))
		{
			if (_newRotationVector.y < 0f)
			{
				ApplyRotationDamp(UpperVerticalBound, AngleOfView);
			}
			if (Math.Abs(_rotSpeed.y) > DampMaxSpeed && _rotSpeed.y > 0f)
			{
				_rotSpeed.y = DampMaxSpeed * (_rotSpeed.y / _rotSpeed.y);
			}
		}
		else if (IsInsideLowerAngleDampZone(LowerVerticalBound))
		{
			if (_newRotationVector.y > 0f)
			{
				ApplyRotationDamp(LowerVerticalBound, AngleOfView);
			}
			if (Math.Abs(_rotSpeed.y) > DampMaxSpeed && _rotSpeed.y < 0f)
			{
				_rotSpeed.y = DampMaxSpeed * (_rotSpeed.y / Math.Abs(_rotSpeed.y));
			}
		}
	}

	private bool IsInsideUpperAngleDampZone(DampVector dampVector)
	{
		return AngleOfView > dampVector.Limit - dampVector.DampThreshold;
	}

	private bool IsInsideLowerAngleDampZone(DampVector dampVector)
	{
		return AngleOfView < dampVector.Limit + dampVector.DampThreshold;
	}

	private void ApplyRotationDamp(DampVector dampVector, float currentValue)
	{
		float num = Mathf.Abs(currentValue - dampVector.Limit);
		float num2 = num / Mathf.Abs(dampVector.DampThreshold);
		if (float.IsNaN(num2))
		{
			num2 = 0f;
		}
		_newRotationVector.y = Mathf.LerpAngle(0f, _newRotationVector.y, num2 * num2);
	}

	private void ClampCameraAngle()
	{
		if (FreeCam)
		{
			AngleOfView = Mathf.Clamp(AngleOfView, LowerVerticalBound.Limit, UpperVerticalBound.Limit);
		}
		else
		{
			AngleOfView = FixCam;
		}
	}

	private void ApplyCameraAngleBounce()
	{
		float angleOfView = AngleOfView;
		float b = Mathf.Clamp(AngleOfView, LowerVerticalBound.Limit + LowerVerticalBound.BounceThreshold, UpperVerticalBound.Limit - UpperVerticalBound.BounceThreshold);
		if (!_rotating)
		{
			AngleOfView = Mathf.LerpAngle(angleOfView, b, AverageDelta);
		}
	}

	private Vector3 CalculateTopTriangle(Vector3 target)
	{
		Vector2 vector = new Vector2(Mathf.Cos(AngleOfView * ((float)Math.PI / 180f)) * Distance, Mathf.Sin(AngleOfView * ((float)Math.PI / 180f)) * Distance);
		Vector3 vector2 = target + new Vector3(Mathf.Sin(CamOrientation * ((float)Math.PI / 180f)), 0f, Mathf.Cos(CamOrientation * ((float)Math.PI / 180f))) * (0f - vector.x);
		return vector2 + new Vector3(0f, vector.y, 0f);
	}

	public Vector3 FitVector3InsideBounds(Vector3 vector3)
	{
		if (vector3.x > BoundsLimit.max.x)
		{
			vector3.x = BoundsLimit.max.x;
		}
		if (vector3.y > BoundsLimit.max.y)
		{
			vector3.y = BoundsLimit.max.y;
		}
		if (vector3.z > BoundsLimit.max.z)
		{
			vector3.z = BoundsLimit.max.z;
		}
		if (vector3.x < BoundsLimit.min.x)
		{
			vector3.x = BoundsLimit.min.x;
		}
		if (vector3.y < BoundsLimit.min.y)
		{
			vector3.y = BoundsLimit.min.y;
		}
		if (vector3.z < BoundsLimit.min.z)
		{
			vector3.z = BoundsLimit.min.z;
		}
		return vector3;
	}

	public bool IsCameraInsideBounds()
	{
		return base.transform.position.x >= BoundsLimit.min.x && base.transform.position.x <= BoundsLimit.max.x && base.transform.position.y >= BoundsLimit.min.y && base.transform.position.y <= BoundsLimit.max.y && base.transform.position.z >= BoundsLimit.min.y && base.transform.position.z <= BoundsLimit.max.z;
	}

	public void SetZoom(float zoom)
	{
		Vector3 p = Target - Camera.main.transform.forward * zoom;
		Positionner(p, Camera.main.transform.rotation, zoom);
	}

	public void SetRotation(float x, float y, float z)
	{
		Positionner(Camera.main.transform.position, Quaternion.Euler(x, y, z));
	}

	public void UpdateDistanceFromGround()
	{
		float num = 256f;
		if (Physics.Raycast(Target, Vector3.up, out var hitInfo, Mathf.Max(256f - Target.y, 0.1f), 1024, QueryTriggerInteraction.Ignore))
		{
			num = hitInfo.point.y - 0.1f;
		}
		float worldHeight = 0f;
		bool flag = false;
		while (!flag)
		{
			if (Physics.Raycast(new Vector3(Target.x, num, Target.z), -Vector3.up, out hitInfo, Mathf.Max(num, 0.1f), 1024, QueryTriggerInteraction.Ignore))
			{
				MegaVoxelGeo component = hitInfo.transform.GetComponent<MegaVoxelGeo>();
				if (component == null || component.lodFactor != 0)
				{
					num = hitInfo.point.y - 0.1f;
					continue;
				}
				worldHeight = hitInfo.point.y + 0.2f;
				flag = true;
			}
			else
			{
				flag = true;
			}
		}
		_worldHeight = worldHeight;
	}

	public void GenerateSkybox()
	{
		ObjectPoolSingleton.RequestComponent<SkyboxConfigurator>(out var t, Brix.External.Factories.Environment.Skybox3d);
		t.GetComponent<IdentityHolder>().Id = "Skybox";
		t.Inject(GetComponent<Camera>());
	}

	public static void CallMove(Vector3 direction)
	{
		_moveVector += direction;
		_moving = true;
	}

	public void LookAt(Vector3 target)
	{
		Positionner(target - base.transform.forward * Distance);
	}

	public void Positionner(Vector3 p)
	{
		Positionner(p, base.transform.rotation);
	}

	public void Positionner(Vector3 p, Quaternion r, float e)
	{
		Distance = e;
		Positionner(p, r);
	}

	public void Positionner(Vector3 p, Quaternion r)
	{
		base.transform.position = p;
		base.transform.rotation = r;
		Target = p + base.transform.forward * Distance;
		LookAt(r);
	}

	private void LookAt(Quaternion r)
	{
		AngleOfView = Mathf.Round(r.eulerAngles.x);
		if (AngleOfView > 180f)
		{
			AngleOfView -= 360f;
		}
		CamOrientation = Mathf.Round(r.eulerAngles.y);
		if (CamOrientation > 180f)
		{
			CamOrientation -= 360f;
		}
		ResetAutoClimb();
	}

	public void displayCurseur(int index)
	{
		if (index > 0)
		{
			ActiveDelai = Time.time;
		}
		if (ActiveDelai + 2f < Time.time)
		{
			index = -1;
		}
		switch (index)
		{
		case -1:
			CursorNormal.SetActive(value: false);
			CursorMove.SetActive(value: false);
			CursorRotate.SetActive(value: false);
			break;
		case 0:
			CursorNormal.SetActive(value: true);
			CursorMove.SetActive(value: false);
			CursorRotate.SetActive(value: false);
			break;
		case 1:
			CursorNormal.SetActive(value: false);
			CursorMove.SetActive(value: true);
			CursorRotate.SetActive(value: false);
			break;
		case 2:
			CursorNormal.SetActive(value: false);
			CursorMove.SetActive(value: false);
			CursorRotate.SetActive(value: true);
			break;
		case 3:
			CursorNormal.SetActive(value: true);
			CursorMove.SetActive(value: false);
			CursorRotate.SetActive(value: false);
			break;
		}
	}

	public static void SetScrollFactor(float factor)
	{
		ScrollFactor = factor;
	}

	[LuaDrivable("SetTimelapseTranslation")]
	private static DynValue lua_SetTimelapseTranslation(ScriptExecutionContext context, CallbackArguments args)
	{
		CameraScript cameraScript = args.AsComponent<CameraScript>(0, "SetTimelapseTranslation");
		float x = args.AsFloat(1, "SetTimelapseTranslation");
		float y = args.AsFloat(2, "SetTimelapseTranslation");
		float z = args.AsFloat(3, "SetTimelapseTranslation");
		cameraScript.SetTimelapseTranslationVector(x, y, z);
		return DynValue.Void;
	}

	[LuaDrivable("SetTimelapseRotation")]
	private static DynValue lua_SetTimelapseRotation(ScriptExecutionContext context, CallbackArguments args)
	{
		CameraScript cameraScript = args.AsComponent<CameraScript>(0, "SetTimelapseRotation");
		float rotLat = args.AsFloat(1, "SetTimelapseRotation");
		float rotLong = args.AsFloat(2, "SetTimelapseRotation");
		cameraScript.SetTimelapseRotation(rotLat, rotLong);
		return DynValue.Void;
	}

	[LuaDrivable("SetTimelapseZoom")]
	private static DynValue lua_SetTimelapseZoom(ScriptExecutionContext context, CallbackArguments args)
	{
		CameraScript cameraScript = args.AsComponent<CameraScript>(0, "SetTimelapseZoom");
		float timelapseZoom = args.AsFloat(1, "SetTimelapseZoom");
		cameraScript.SetTimelapseZoom(timelapseZoom);
		return DynValue.Void;
	}

	[LuaDrivable("RegisterPositionAndRotation")]
	private static DynValue lua_RegisterPositionAndRotation(ScriptExecutionContext context, CallbackArguments args)
	{
		CameraScript cameraScript = args.AsComponent<CameraScript>(0, "RegisterPositionAndRotation");
		cameraScript.RegisterPosRot();
		return DynValue.Void;
	}

	[LuaDrivable("RepositionCamera")]
	private static DynValue lua_RepositionCamera(ScriptExecutionContext context, CallbackArguments args)
	{
		CameraScript cameraScript = args.AsComponent<CameraScript>(0, "RepositionCamera");
		cameraScript.RepositionCam();
		return DynValue.Void;
	}

	[LuaDrivable("ActivateGizmos")]
	private static DynValue lua_ActivateGizmos(ScriptExecutionContext context, CallbackArguments args)
	{
		CameraScript cameraScript = args.AsComponent<CameraScript>(0, "ActivateGizmos");
		cameraScript.ActivateGizmoRenderers();
		return DynValue.Void;
	}

	[LuaDrivable("DeactivateGizmos")]
	private static DynValue lua_DeactivateGizmos(ScriptExecutionContext context, CallbackArguments args)
	{
		CameraScript cameraScript = args.AsComponent<CameraScript>(0, "DeactivateGizmos");
		cameraScript.DeactivateGizmoRenderers();
		return DynValue.Void;
	}

	[LuaDrivable("ToggleHideGizmos")]
	private static DynValue lua_ToggleHideGizmos(ScriptExecutionContext context, CallbackArguments args)
	{
		CameraScript cameraScript = args.AsComponent<CameraScript>(0, "ToggleHideGizmos");
		cameraScript.ToggleHideGizmos();
		return DynValue.Void;
	}

	[LuaDrivable("SetZoomOutLimit")]
	private static DynValue lua_SetZoomOutLimit(ScriptExecutionContext context, CallbackArguments args)
	{
		CameraScript cameraScript = args.AsComponent<CameraScript>(0, "SetZoomOutLimit");
		int zoomOutLimit = args.AsInt(1, "SetZoomOutLimit");
		cameraScript.SetZoomOutLimit(zoomOutLimit);
		return DynValue.Void;
	}

	[LuaDrivable("SetZoomInLimit")]
	private static DynValue lua_SetZoomInLimit(ScriptExecutionContext context, CallbackArguments args)
	{
		CameraScript cameraScript = args.AsComponent<CameraScript>(0, "SetZoomInLimit");
		int zoomInLimit = args.AsInt(1, "SetZoomInLimit");
		cameraScript.SetZoomInLimit(zoomInLimit);
		return DynValue.Void;
	}

	[LuaDrivable("GetTravelSpeed")]
	private static DynValue lua_GetTravelSpeed(ScriptExecutionContext context, CallbackArguments args)
	{
		CameraScript cameraScript = args.AsComponent<CameraScript>(0, "GetTravelSpeed");
		DynValue value = DynValue.NewNumber(cameraScript.GetTravelSpeed().x);
		DynValue value2 = DynValue.NewNumber(cameraScript.GetTravelSpeed().y);
		DynValue value3 = DynValue.NewNumber(cameraScript.GetTravelSpeed().z);
		return DynValue.NewTuple(value, value2, value3);
	}

	[LuaDrivable("SetTravelSpeed")]
	private static DynValue lua_SetTravelSpeed(ScriptExecutionContext context, CallbackArguments args)
	{
		CameraScript cameraScript = args.AsComponent<CameraScript>(0, "SetTravelSpeed");
		float x = args.AsFloat(1, "SetTravelSpeed");
		float y = args.AsFloat(2, "SetTravelSpeed");
		float z = args.AsFloat(3, "SetTravelSpeed");
		cameraScript.SetTravelSpeed(x, y, z);
		return DynValue.Void;
	}

	[LuaDrivable("GetOrbitSpeed")]
	private static DynValue lua_GetOrbitSpeed(ScriptExecutionContext context, CallbackArguments args)
	{
		CameraScript cameraScript = args.AsComponent<CameraScript>(0, "GetOrbitSpeed");
		return DynValue.NewNumber(cameraScript.GetOrbitSpeed());
	}

	[LuaDrivable("SetOrbitSpeed")]
	private static DynValue lua_SetOrbitSpeed(ScriptExecutionContext context, CallbackArguments args)
	{
		CameraScript cameraScript = args.AsComponent<CameraScript>(0, "SetOrbitSpeed");
		float orbitSpeed = args.AsFloat(1, "SetOrbitSpeed");
		cameraScript.SetOrbitSpeed(orbitSpeed);
		return DynValue.Void;
	}

	[LuaDrivable("GetZoomSpeed")]
	private static DynValue lua_GetZoomSpeed(ScriptExecutionContext context, CallbackArguments args)
	{
		CameraScript cameraScript = args.AsComponent<CameraScript>(0, "GetZoomSpeed");
		return DynValue.NewNumber(cameraScript.GetZoomSpeed());
	}

	[LuaDrivable("SetZoomSpeed")]
	private static DynValue lua_SetZoomSpeed(ScriptExecutionContext context, CallbackArguments args)
	{
		CameraScript cameraScript = args.AsComponent<CameraScript>(0, "SetZoomSpeed");
		float zoomSpeed = args.AsFloat(1, "SetOrbitSpeed");
		cameraScript.SetZoomSpeed(zoomSpeed);
		return DynValue.Void;
	}

	[LuaDrivable("ToggleTimelapse")]
	private static DynValue lua_ToggleTimelapse(ScriptExecutionContext context, CallbackArguments args)
	{
		CameraScript cameraScript = args.AsComponent<CameraScript>(0, "ToggleTimelapse");
		cameraScript.ToggleTimelapse();
		return DynValue.Void;
	}

	[LuaDrivable("ToggleEditorCamera")]
	private static DynValue lua_ToggleEditorCamera(ScriptExecutionContext context, CallbackArguments args)
	{
		CameraScript cameraScript = args.AsComponent<CameraScript>(0, "ToggleEditorCamera");
		bool value = args.AsBool(1, "ToggleEditorCamera");
		cameraScript.ToggleEditorCamera(value);
		return DynValue.Void;
	}

	[LuaDrivable("SetZoom")]
	private static DynValue lua_SetZoom(ScriptExecutionContext context, CallbackArguments args)
	{
		CameraScript cameraScript = args.AsComponent<CameraScript>(0, "SetZoom");
		float num = args.AsFloat(1, "SetZoom");
		Vector3 p = Target - Camera.main.transform.forward * num;
		cameraScript.Positionner(p, Camera.main.transform.rotation, num);
		return DynValue.Void;
	}

	[LuaDrivable("SetRotation")]
	private static DynValue lua_SetRotation(ScriptExecutionContext context, CallbackArguments args)
	{
		CameraScript cameraScript = args.AsComponent<CameraScript>(0, "SetRotation");
		float x = args.AsFloat(1, "SetRotation");
		float y = args.AsFloat(2, "SetRotation");
		float z = args.AsFloat(3, "SetRotation");
		cameraScript.Positionner(Camera.main.transform.position, Quaternion.Euler(x, y, z));
		return DynValue.Void;
	}

	[LuaDrivable("GetRotation")]
	private static DynValue lua_GetRotation(ScriptExecutionContext context, CallbackArguments args)
	{
		CameraScript cameraScript = args.AsComponent<CameraScript>(0, "GetRotation");
		Vector3 eulerAngles = cameraScript.transform.rotation.eulerAngles;
		DynValue value = DynValue.NewNumber(eulerAngles.x);
		DynValue value2 = DynValue.NewNumber(eulerAngles.y);
		DynValue value3 = DynValue.NewNumber(eulerAngles.z);
		return DynValue.NewTuple(value, value2, value3);
	}

	[LuaDrivable("LookAt")]
	private static DynValue lua_LookAt(ScriptExecutionContext context, CallbackArguments args)
	{
		CameraScript cameraScript = args.AsComponent<CameraScript>(0, "LookAt");
		Vector3 vector = args.AsUserData<Vector3>(1, "LookAt");
		cameraScript.Positionner(vector - cameraScript.transform.forward * cameraScript.Distance);
		return DynValue.Void;
	}
}
