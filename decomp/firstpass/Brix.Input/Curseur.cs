using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Brix.Engine;
using Rewired;
using UnityEngine;

namespace Brix.Input;

public class Curseur : MonoBehaviour
{
	private class CursorSpriteJoints
	{
		private Texture2D _normal;

		private Texture2D _white;

		public Texture2D normal => _normal;

		public Texture2D white
		{
			get
			{
				if (_white == null)
				{
					return _normal;
				}
				return _white;
			}
		}

		public bool Centered { get; private set; }

		public CursorSpriteJoints(Sprite Normal, Sprite White, bool centered)
		{
			_normal = textureFromSprite(Normal);
			if (White != null)
			{
				_white = textureFromSprite(White);
			}
			else
			{
				_white = null;
			}
			Centered = centered;
		}

		private static Texture2D textureFromSprite(Sprite sprite)
		{
			if (sprite == null)
			{
				return null;
			}
			if (sprite.rect.width != (float)sprite.texture.width)
			{
				Texture2D texture2D = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height, TextureFormat.ARGB32, mipmap: false);
				texture2D.filterMode = FilterMode.Point;
				Color[] pixels = sprite.texture.GetPixels((int)sprite.textureRect.x, (int)sprite.textureRect.y, (int)sprite.textureRect.width, (int)sprite.textureRect.height);
				texture2D.SetPixels(pixels);
				texture2D.Apply();
				return texture2D;
			}
			return sprite.texture;
		}
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	private struct CursorIconComparer : IEqualityComparer<CursorIcon>
	{
		public bool Equals(CursorIcon x, CursorIcon y)
		{
			return x == y;
		}

		public int GetHashCode(CursorIcon obj)
		{
			return (int)obj;
		}
	}

	public static Curseur instance;

	public Sprite Idle;

	public Sprite White_Idle;

	public Sprite Selected;

	public Sprite White_Selected;

	public Sprite Selectable;

	public Sprite White_Selectable;

	public Sprite Plus;

	public Sprite White_Plus;

	public Sprite Minus;

	public Sprite White_Minus;

	public Sprite Selection;

	public Sprite White_Selection;

	public Sprite Look;

	public Sprite White_Look;

	public Sprite Cancel;

	public Sprite White_Cancel;

	public Sprite Circle;

	public Sprite White_Circle;

	public Sprite FilledCircle;

	public Sprite White_FilledCircle;

	public Sprite Down;

	public Sprite White_Down;

	public Sprite Up;

	public Sprite White_Up;

	public Sprite MoveTo;

	public Sprite White_MoveTo;

	public Sprite DownBlue;

	public Sprite White_DownBlue;

	public Sprite UpYellow;

	public Sprite White_UpYellow;

	public Sprite Nudge_Up;

	public Sprite Nudge_UpRight;

	public Sprite Nudge_Right;

	public Sprite Nudge_DownRight;

	public Sprite Nudge_Down;

	public Sprite Nudge_DownLeft;

	public Sprite Nudge_Left;

	public Sprite Nudge_UpLeft;

	public Sprite HandleHorizontal;

	public Sprite HandleVertical;

	public Sprite HandleTerrain;

	public Sprite HandleDelete;

	public Sprite HandleHorizontal_White;

	public Sprite HandleVertical_White;

	public Sprite HandleTerrain_White;

	public Sprite HandleDelete_White;

	public Sprite CursorProgress_1;

	public Sprite CursorProgress_2;

	public Sprite CursorProgress_3;

	public Sprite CursorProgress_4;

	public Sprite CursorProgress_5;

	public Sprite CursorProgress_6;

	public Sprite CursorProgress_7;

	public Sprite PickAxe;

	public Sprite Axe;

	public Sprite Sword;

	public Sprite Bow;

	public Sprite Hand;

	public Sprite EditMode;

	public Sprite EditModeClick;

	private static Texture2D _currentCursorTexture;

	private static Dictionary<CursorIcon, CursorSpriteJoints> _allCursors;

	[SerializeField]
	private bool curseurHardware = true;

	[NonSerialized]
	public bool disableNudgeByPieMenu;

	[NonSerialized]
	public bool disableNudgeBySystemMenu;

	[NonSerialized]
	public bool disableNudgeBySettings;

	[NonSerialized]
	public bool disableLockBySystemMenu;

	[NonSerialized]
	public bool disableLockBySettings;

	public bool verbose;

	public int nudgeAreaSize = 5;

	private bool _appFocus;

	private static CursorIcon currentCursor;

	private bool disableNudgeByMMBDown => PickingUtility.Player.GetButton("cam_mouse_rotate");

	public bool enableNudge => enableLockCursorToScreen && !disableNudgeByPieMenu && !disableNudgeBySystemMenu && !disableNudgeBySettings && !disableNudgeByMMBDown;

	public bool enableLockCursorToScreen => !disableLockBySystemMenu && !disableLockBySettings;

	public bool outAppFocus => _appFocus;

	private void Awake()
	{
		Dictionary<CursorIcon, CursorSpriteJoints> dictionary = new Dictionary<CursorIcon, CursorSpriteJoints>(default(CursorIconComparer));
		dictionary.Add(CursorIcon.Idle, new CursorSpriteJoints(Idle, White_Idle, centered: false));
		dictionary.Add(CursorIcon.Selected, new CursorSpriteJoints(Selected, White_Selected, centered: false));
		dictionary.Add(CursorIcon.Selectable, new CursorSpriteJoints(Selectable, White_Selectable, centered: false));
		dictionary.Add(CursorIcon.Plus, new CursorSpriteJoints(Plus, White_Plus, centered: false));
		dictionary.Add(CursorIcon.Minus, new CursorSpriteJoints(Minus, White_Minus, centered: false));
		dictionary.Add(CursorIcon.Selection, new CursorSpriteJoints(Selection, White_Selection, centered: false));
		dictionary.Add(CursorIcon.Look, new CursorSpriteJoints(Look, White_Look, centered: false));
		dictionary.Add(CursorIcon.Cancel, new CursorSpriteJoints(Cancel, White_Cancel, centered: false));
		dictionary.Add(CursorIcon.Circle, new CursorSpriteJoints(Circle, White_Circle, centered: true));
		dictionary.Add(CursorIcon.FilledCircle, new CursorSpriteJoints(FilledCircle, White_FilledCircle, centered: true));
		dictionary.Add(CursorIcon.Down, new CursorSpriteJoints(Down, White_Down, centered: false));
		dictionary.Add(CursorIcon.Up, new CursorSpriteJoints(Up, White_Up, centered: false));
		dictionary.Add(CursorIcon.MoveTo, new CursorSpriteJoints(MoveTo, White_MoveTo, centered: false));
		dictionary.Add(CursorIcon.DownBlue, new CursorSpriteJoints(DownBlue, White_DownBlue, centered: false));
		dictionary.Add(CursorIcon.UpYellow, new CursorSpriteJoints(UpYellow, White_UpYellow, centered: false));
		dictionary.Add(CursorIcon.Nudge_Up, new CursorSpriteJoints(Nudge_Up, null, centered: false));
		dictionary.Add(CursorIcon.Nudge_UpLeft, new CursorSpriteJoints(Nudge_UpLeft, null, centered: false));
		dictionary.Add(CursorIcon.Nudge_Left, new CursorSpriteJoints(Nudge_Left, null, centered: false));
		dictionary.Add(CursorIcon.Nudge_DownLeft, new CursorSpriteJoints(Nudge_DownLeft, null, centered: false));
		dictionary.Add(CursorIcon.Nudge_Down, new CursorSpriteJoints(Nudge_Down, null, centered: false));
		dictionary.Add(CursorIcon.Nudge_DownRight, new CursorSpriteJoints(Nudge_DownRight, null, centered: false));
		dictionary.Add(CursorIcon.Nudge_Right, new CursorSpriteJoints(Nudge_Right, null, centered: false));
		dictionary.Add(CursorIcon.Nudge_UpRight, new CursorSpriteJoints(Nudge_UpRight, null, centered: false));
		dictionary.Add(CursorIcon.PickAxe, new CursorSpriteJoints(PickAxe, null, centered: false));
		dictionary.Add(CursorIcon.Axe, new CursorSpriteJoints(Axe, null, centered: false));
		dictionary.Add(CursorIcon.Sword, new CursorSpriteJoints(Sword, null, centered: false));
		dictionary.Add(CursorIcon.Bow, new CursorSpriteJoints(Bow, null, centered: false));
		dictionary.Add(CursorIcon.Hand, new CursorSpriteJoints(Hand, null, centered: false));
		dictionary.Add(CursorIcon.HandleHorizontal, new CursorSpriteJoints(HandleHorizontal, null, centered: false));
		dictionary.Add(CursorIcon.HandleVertical, new CursorSpriteJoints(HandleVertical, null, centered: false));
		dictionary.Add(CursorIcon.HandleTerrain, new CursorSpriteJoints(HandleTerrain, null, centered: false));
		dictionary.Add(CursorIcon.HandleDelete, new CursorSpriteJoints(HandleDelete, null, centered: false));
		dictionary.Add(CursorIcon.HandleHorizontal_White, new CursorSpriteJoints(HandleHorizontal_White, null, centered: false));
		dictionary.Add(CursorIcon.HandleVertical_White, new CursorSpriteJoints(HandleVertical_White, null, centered: false));
		dictionary.Add(CursorIcon.HandleTerrain_White, new CursorSpriteJoints(HandleTerrain_White, null, centered: false));
		dictionary.Add(CursorIcon.HandleDelete_White, new CursorSpriteJoints(HandleDelete_White, null, centered: false));
		dictionary.Add(CursorIcon.CursorProgress_1, new CursorSpriteJoints(CursorProgress_1, null, centered: false));
		dictionary.Add(CursorIcon.CursorProgress_2, new CursorSpriteJoints(CursorProgress_2, null, centered: false));
		dictionary.Add(CursorIcon.CursorProgress_3, new CursorSpriteJoints(CursorProgress_3, null, centered: false));
		dictionary.Add(CursorIcon.CursorProgress_4, new CursorSpriteJoints(CursorProgress_4, null, centered: false));
		dictionary.Add(CursorIcon.CursorProgress_5, new CursorSpriteJoints(CursorProgress_5, null, centered: false));
		dictionary.Add(CursorIcon.CursorProgress_6, new CursorSpriteJoints(CursorProgress_6, null, centered: false));
		dictionary.Add(CursorIcon.CursorProgress_7, new CursorSpriteJoints(CursorProgress_7, null, centered: false));
		dictionary.Add(CursorIcon.EditMode, new CursorSpriteJoints(EditMode, EditModeClick, centered: false));
		_allCursors = dictionary;
		_currentCursorTexture = _allCursors[CursorIcon.Idle].normal;
		RefreshCurseur(CursorIcon.Idle);
		instance = this;
	}

	private void Update()
	{
		if (Architecte.commence && _appFocus && enableLockCursorToScreen)
		{
			Clip(flag: true);
			CheckToNudgeScreen();
		}
		else
		{
			Clip(flag: false);
		}
		SetCurseur(currentCursor);
	}

	private void OnApplicationFocus(bool focus)
	{
		if (!focus)
		{
			Clip(flag: false);
		}
		_appFocus = focus;
	}

	private void Clip(bool flag)
	{
		if (flag)
		{
			RepositionCursorInScreen();
		}
	}

	private void CheckToNudgeScreen()
	{
		if (enableNudge)
		{
			Vector2 localMousePosition = ProMouse.Instance.GetLocalMousePosition();
			if (localMousePosition.x < (float)nudgeAreaSize)
			{
				CameraScript.CallMove(Vector3.left);
			}
			else if (localMousePosition.x > (float)(Screen.width - nudgeAreaSize))
			{
				CameraScript.CallMove(Vector3.right);
			}
			if (localMousePosition.y < (float)nudgeAreaSize)
			{
				CameraScript.CallMove(Vector3.back);
			}
			else if (localMousePosition.y > (float)(Screen.height - nudgeAreaSize))
			{
				CameraScript.CallMove(Vector3.forward);
			}
		}
	}

	private void RepositionCursorInScreen()
	{
		Vector2 localMousePosition = ProMouse.Instance.GetLocalMousePosition();
		int xPos = (int)localMousePosition.x;
		int yPos = (int)localMousePosition.y;
		bool flag = false;
		if (localMousePosition.x < 0f)
		{
			xPos = 0;
			flag = true;
		}
		else if (localMousePosition.x > (float)Screen.width)
		{
			xPos = Screen.width;
			flag = true;
		}
		if (localMousePosition.y < 0f)
		{
			yPos = 0;
			flag = true;
		}
		else if (localMousePosition.y > (float)Screen.height)
		{
			yPos = Screen.height;
			flag = true;
		}
		if (flag)
		{
			ProMouse.Instance.SetCursorPosition(xPos, yPos);
		}
	}

	private void OnDestroy()
	{
		Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
	}

	public static void SetCurseur(CursorIcon cursor)
	{
		Texture2D goodTexture = GetGoodTexture(cursor);
		if (_currentCursorTexture != goodTexture)
		{
			_currentCursorTexture = goodTexture;
			currentCursor = cursor;
			instance.RefreshCurseur(cursor);
		}
	}

	private void RefreshCurseur(CursorIcon icon)
	{
		if (curseurHardware)
		{
			Vector2 hotspot = IconPreferedPosition(icon);
			Cursor.SetCursor(_currentCursorTexture, hotspot, CursorMode.Auto);
		}
	}

	private Vector2 IconPreferedPosition(CursorIcon icon)
	{
		if (_allCursors[icon].Centered)
		{
			Texture2D goodTexture = GetGoodTexture(icon);
			if (goodTexture != null)
			{
				return new Vector2((float)goodTexture.height / 2f, (float)goodTexture.width / 2f);
			}
		}
		return new Vector2(1f, 1f);
	}

	public static bool IsInGameWindow()
	{
		if (new Rect(0f, 0f, Screen.width, Screen.height).Contains(PickingUtility.Player.controllers.Mouse.screenPosition))
		{
			return true;
		}
		return false;
	}

	private static Texture2D GetGoodTexture(CursorIcon mode)
	{
		if (IsInGameWindow())
		{
			Player player = PickingUtility.Player;
			if ((player.GetButton("cam_action1") && player.GetButtonTimePressed("cam_action1") < 0.1f) || (player.GetButton("cam_action2") && player.GetButtonTimePressed("cam_action2") < 0.1f) || (player.GetButton("cam_mouse_rotate") && player.GetButtonTimePressed("cam_mouse_rotate") < 0.1f))
			{
				return _allCursors[mode].white;
			}
			return _allCursors[mode].normal;
		}
		return null;
	}
}
