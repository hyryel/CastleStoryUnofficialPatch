using Brix.External.Signals;
using UnityEngine;

public class LoadingScreenSlide : MonoBehaviour
{
	public enum State
	{
		closing,
		opening,
		idle
	}

	public Signal AnimStart = new Signal();

	public Signal AnimEnd = new Signal();

	[SerializeField]
	public RectTransform Top;

	[SerializeField]
	public RectTransform Bot;

	public float LagRatio;

	public float FinishAnimThreshold;

	private State state;

	private void Start()
	{
	}

	public void InstantClose()
	{
		Bot.anchorMax = new Vector2(Bot.anchorMax.x, 0.5f);
		Top.anchorMin = new Vector2(Top.anchorMin.x, 0.5f);
		state = State.idle;
		AnimEnd.Invoke();
	}

	public void InstantOpen()
	{
		Bot.anchorMax = new Vector2(Bot.anchorMax.x, 0f);
		Top.anchorMin = new Vector2(Top.anchorMin.x, 1f);
		state = State.idle;
		AnimEnd.Invoke();
	}

	public void Open()
	{
		state = State.opening;
		AnimStart.Invoke();
	}

	public void Close()
	{
		state = State.closing;
		AnimStart.Invoke();
	}

	public void Idle()
	{
		state = State.idle;
		AnimEnd.Invoke();
	}

	public float ComputeIncrement()
	{
		if (LagRatio == 0f)
		{
			return 0f;
		}
		float num = 0f;
		if (state == State.closing)
		{
			num = 0.5f - Bot.anchorMax.y;
		}
		if (state == State.opening)
		{
			num = 0f - Bot.anchorMax.y;
		}
		if (state == State.idle)
		{
			num = 0f;
		}
		return num / LagRatio * Time.deltaTime;
	}

	private void Update()
	{
		float num = ComputeIncrement();
		if (Mathf.Abs(num) < FinishAnimThreshold)
		{
			if (state == State.closing)
			{
				InstantClose();
			}
			if (state == State.opening)
			{
				InstantOpen();
			}
		}
		else
		{
			Bot.anchorMax = new Vector2(Bot.anchorMax.x, Bot.anchorMax.y + num);
			Top.anchorMin = new Vector2(Top.anchorMin.x, 1f - (Bot.anchorMax.y + num));
		}
	}
}
