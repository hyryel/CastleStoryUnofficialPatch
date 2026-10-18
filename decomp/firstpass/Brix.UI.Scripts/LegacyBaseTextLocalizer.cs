using Brix.Engine;
using Brix.External.Signals;
using UnityEngine;
using UnityEngine.UI;

namespace Brix.UI.Scripts;

public abstract class LegacyBaseTextLocalizer : MonoBehaviour
{
	public Text Text;

	public bool ToUpper = true;

	private SignalConnection _sgLocalize;

	public static GameSignals.GameSignal LocalizeSignal = new GameSignals.GameSignal();

	protected abstract string Value { get; }

	private void Refresh()
	{
		if (!(Text == null))
		{
			Text.text = ((!ToUpper) ? Value : Value.ToUpper());
		}
	}

	public void OnEnable()
	{
		_sgLocalize = GameSignals.Connect(LocalizeSignal, Refresh);
		Refresh();
	}

	public void OnDisable()
	{
		_sgLocalize.Disconnect();
	}
}
