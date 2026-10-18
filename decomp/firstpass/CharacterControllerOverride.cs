using Brix.Game.AI;
using Brix.Lua;
using MoonSharp.Interpreter;
using Rewired;
using UnityEngine;

public class CharacterControllerOverride : MonoBehaviour
{
	private Player _player;

	private Labor labor;

	private Locomotion4 locomotion;

	private Navigation navigation;

	private Thinker thinker;

	private SkinnedMeshRenderer skin;

	private CharacterController controller;

	public LayerMask CollisionLayerMask;

	private float speed = 10f;

	private float jumpSpeed = 10f;

	private float gravity = 5f;

	private Vector3 directionVector = Vector3.zero;

	private float mouseX;

	private Vector3 YAxisRotation;

	private float mouseSensitivity = 0.4f;

	private bool isPossessed;

	private void Start()
	{
		_player = ReInput.players.GetPlayer("local");
		if (base.gameObject.GetComponent<Labor>() != null)
		{
			labor = GetComponent<Labor>();
		}
		if (base.gameObject.GetComponent<Locomotion4>() != null)
		{
			locomotion = GetComponent<Locomotion4>();
		}
		if (base.gameObject.GetComponent<Navigation>() != null)
		{
			navigation = GetComponent<Navigation>();
		}
		if (base.gameObject.GetComponent<Thinker>() != null)
		{
			thinker = GetComponent<Thinker>();
		}
		if (base.gameObject.GetComponentInChildren<SkinnedMeshRenderer>() != null)
		{
			skin = GetComponentInChildren<SkinnedMeshRenderer>();
		}
		controller = GetComponent<CharacterController>();
	}

	private void Update()
	{
		if (isPossessed)
		{
			directionVector.x = 0f;
			directionVector.z = 0f;
			if (_player.GetButton("cam_move_forward"))
			{
				directionVector.z += speed;
			}
			if (_player.GetButton("cam_move_backward"))
			{
				directionVector.z -= speed;
			}
			if (_player.GetButton("cam_move_right"))
			{
				directionVector.x += speed;
			}
			if (_player.GetButton("cam_move_left"))
			{
				directionVector.x -= speed;
			}
			mouseX = ReInput.controllers.Mouse.GetAxis(0) * mouseSensitivity;
			YAxisRotation = new Vector3(0f, mouseX, 0f);
			base.transform.Rotate(YAxisRotation);
			directionVector = base.transform.TransformDirection(directionVector);
			if (_player.GetButton("bricktron_jump"))
			{
				directionVector.y = jumpSpeed;
			}
			directionVector.y -= gravity * Time.deltaTime;
			if (directionVector.y <= -10f)
			{
				directionVector.y = -10f;
			}
			controller.Move(directionVector * Time.deltaTime);
		}
	}

	[LuaDrivable("EnableManualOverride")]
	public static DynValue lua_EnableManualOverride(ScriptExecutionContext executionContext, CallbackArguments args)
	{
		CharacterControllerOverride characterControllerOverride = args.AsComponent<CharacterControllerOverride>(0, "EnableManualOverride");
		characterControllerOverride.labor.enabled = false;
		characterControllerOverride.locomotion.enabled = false;
		characterControllerOverride.navigation.enabled = false;
		characterControllerOverride.thinker.enabled = false;
		characterControllerOverride.skin.enabled = false;
		characterControllerOverride.controller.enabled = true;
		characterControllerOverride.isPossessed = true;
		return DynValue.Void;
	}

	[LuaDrivable("DisableManualOverride")]
	public static DynValue lua_DisableManualOverride(ScriptExecutionContext executionContext, CallbackArguments args)
	{
		CharacterControllerOverride characterControllerOverride = args.AsComponent<CharacterControllerOverride>(0, "EnableManualOverride");
		characterControllerOverride.labor.enabled = true;
		characterControllerOverride.locomotion.enabled = true;
		characterControllerOverride.navigation.enabled = true;
		characterControllerOverride.thinker.enabled = true;
		characterControllerOverride.skin.enabled = true;
		characterControllerOverride.controller.enabled = false;
		characterControllerOverride.isPossessed = false;
		return DynValue.Void;
	}
}
