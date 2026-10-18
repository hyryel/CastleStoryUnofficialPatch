using Brix.Engine;
using Brix.External.Signals;
using Brix.Utils.Signals;
using UnityEngine;

public class CapturableHighlightDriver : MonoBehaviour
{
	private int captureProgress_id;

	private int captureRange_selected_id;

	private int captureRange_hover_id;

	private int captureRange_CaptureInProgress_id;

	[SerializeField]
	private Material CapturableProgress;

	[SerializeField]
	private Material CapturableRange;

	[SerializeField]
	private int CrystalProgressBarRadius;

	private SignalConnection ProgressSignal;

	private Signal Destroyed = new Signal();

	private void Start()
	{
		captureProgress_id = CircleHighlighter.GetId();
		captureRange_hover_id = CircleHighlighter.GetId();
		captureRange_selected_id = CircleHighlighter.GetId();
		captureRange_CaptureInProgress_id = CircleHighlighter.GetId();
		GameSignals.LateConnect(UIGameObserver.IsReady, delegate
		{
			ConnectToSignals();
		});
	}

	private void ConnectToSignals()
	{
		UIGameObserver.capturables.Hovered.Connect(OnCapturableHovered).DisconnectWhen(Destroyed);
		UIGameObserver.capturables.Unhovered.Connect(OnCapturableUnHovered).DisconnectWhen(Destroyed);
		UIGameObserver.capturables.Selected.Connect(OnCapturableSelected).DisconnectWhen(Destroyed);
		UIGameObserver.capturables.Unselected.Connect(OnCapturableUnSelected).DisconnectWhen(Destroyed);
		UIGameObserver.capturables.CaptureStarted.Connect(OnCapturableCaptureStarted).DisconnectWhen(Destroyed);
		UIGameObserver.capturables.CaptureEnded.Connect(OnCapturableCaptureEnded).DisconnectWhen(Destroyed);
		UIGameObserver.capturables.Added.Connect(OnCapturableAdded).DisconnectWhen(Destroyed);
		UIGameObserver.capturables.ExitedScreen.Connect(OnCapturableExitedScreen).DisconnectWhen(Destroyed);
		UIGameObserver.capturables.EnteredScreen.Connect(OnCapturableEnteredScreen).DisconnectWhen(Destroyed);
	}

	private void ConnectToCapturableSignals(IsCapturable capturable)
	{
	}

	private void OnCapturableExitedScreen(IsCapturable capturable)
	{
		if (UIGameObserver.capturables.IsBeingCaptured(capturable))
		{
			capturable.RatioChanged.Disconnect(ProgressSignal);
			CircleHighlighter.Remove(captureRange_CaptureInProgress_id, capturable.gameObject);
			CircleHighlighter.Remove(captureProgress_id, capturable.gameObject);
		}
	}

	private void OnCapturableEnteredScreen(IsCapturable capturable)
	{
		if (UIGameObserver.capturables.IsBeingCaptured(capturable))
		{
			OnCapturableCaptureStarted(capturable);
		}
	}

	private void OnCapturableAdded(IsCapturable capturable)
	{
	}

	private void OnCapturableCaptureStarted(IsCapturable capturable)
	{
		if (!UIGameObserver.capturables.IsInsideScreen(capturable))
		{
			return;
		}
		CircleHighlight progressCircle = CircleHighlighter.HighlightGameObject(captureProgress_id, capturable.gameObject, CapturableProgress, CrystalProgressBarRadius);
		progressCircle.Color = capturable.faction.FactionColor;
		progressCircle.Color2 = ((!(capturable.CurrentCapturers != null)) ? progressCircle.Color2 : capturable.CurrentCapturers.FactionColor);
		progressCircle.Progress = 1f - capturable.CaptureRatio;
		progressCircle.Width -= 0.6f;
		ProgressSignal = capturable.RatioChanged.Connect(delegate(float newRatio)
		{
			if ((bool)progressCircle)
			{
				progressCircle.Progress = 1f - newRatio;
			}
		});
		ProgressSignal.DisconnectWhen(progressCircle.OnReleaseOrDestroySignal);
		ClearRangeHighlight(capturable);
		CircleHighlighter.HighlightGameObject(captureRange_CaptureInProgress_id, capturable.gameObject, CapturableRange, capturable.CaptureRange);
	}

	private void OnCapturableCaptureEnded(IsCapturable capturable)
	{
		if (UIGameObserver.capturables.IsInsideScreen(capturable))
		{
			ProgressSignal.Disconnect();
			CircleHighlighter.Remove(captureRange_CaptureInProgress_id, capturable.gameObject);
			CircleHighlighter.Remove(captureProgress_id, capturable.gameObject);
		}
	}

	private void OnCapturableHovered(IsCapturable capturable)
	{
		if (capturable.CanBeCaptured())
		{
			CircleHighlighter.HighlightGameObject(captureRange_hover_id, capturable.gameObject, CapturableRange, capturable.CaptureRange);
		}
	}

	private void OnCapturableUnHovered(IsCapturable capturable)
	{
		CircleHighlighter.Remove(captureRange_hover_id, capturable.gameObject);
	}

	private void OnCapturableSelected(IsCapturable capturable)
	{
		CircleHighlighter.Remove(captureRange_hover_id, capturable.gameObject);
		if (capturable.CanBeCaptured())
		{
			CircleHighlighter.HighlightGameObject(captureRange_selected_id, capturable.gameObject, CapturableRange, capturable.CaptureRange);
		}
	}

	private void OnCapturableUnSelected(IsCapturable capturable)
	{
		CircleHighlighter.Remove(captureRange_selected_id, capturable.gameObject);
	}

	private void ClearRangeHighlight(IsCapturable capturable)
	{
		CircleHighlight highlight = CircleHighlighter.GetHighlight(captureRange_hover_id, capturable.gameObject);
		CircleHighlight highlight2 = CircleHighlighter.GetHighlight(captureRange_selected_id, capturable.gameObject);
		if (highlight != null)
		{
			CircleHighlighter.Remove(captureRange_hover_id, capturable.gameObject);
		}
		if (highlight2 != null)
		{
			CircleHighlighter.Remove(captureRange_selected_id, capturable.gameObject);
		}
	}

	private void OnDestroy()
	{
		Destroyed.Invoke();
	}
}
