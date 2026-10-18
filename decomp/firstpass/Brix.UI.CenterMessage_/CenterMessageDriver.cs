using Brix.Engine;
using Brix.External.Signals;
using Brix.Utils;
using UnityEngine;

namespace Brix.UI.CenterMessage_;

public class CenterMessageDriver : MonoBehaviour
{
	[SerializeField]
	private CenterMessage message;

	[SerializeField]
	private CenterMessage editorMode;

	private SignalConnection _autoFadeDialogConnection = SignalConnection.Empty;

	private SignalConnection _editorModeChangedConnection = SignalConnection.Empty;

	private void Start()
	{
		ConnectToSignals();
	}

	private void ConnectToSignals()
	{
		if (message != null)
		{
			_autoFadeDialogConnection = GameSignals.Connect(GameSignals.OnAutoFadeDialog, OnAutoFadeDialog);
		}
		if (editorMode != null)
		{
			_editorModeChangedConnection = GameSignals.Connect(GameSignals.OnEditorModeChanged, OnEditorModeChanged);
		}
	}

	private void OnDestroy()
	{
		DisconnectFromSignals();
	}

	private void DisconnectFromSignals()
	{
		_autoFadeDialogConnection.Disconnect();
		_editorModeChangedConnection.Disconnect();
	}

	private void OnAutoFadeDialog(string text, float span)
	{
		message.Show(text, span);
	}

	private void OnEditorModeChanged(bool value)
	{
		if (value)
		{
			editorMode.Show(I2Helper.TryGet("##gamemenu_debug_modedisplay"));
		}
		else
		{
			editorMode.Hide();
		}
	}
}
