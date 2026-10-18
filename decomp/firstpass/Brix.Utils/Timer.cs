using UnityEngine;

namespace Brix.Utils;

public class Timer
{
	public int lastFrame;

	public float lastTime;

	public float smoothDelta;

	public float frameDiff;

	public float deltaTime()
	{
		float num = Time.realtimeSinceStartup - lastTime;
		frameDiff = Time.frameCount - lastFrame;
		lastFrame = Time.frameCount;
		lastTime = Time.realtimeSinceStartup;
		smoothDelta = (smoothDelta + Mathf.Clamp(num, smoothDelta - 0.001f, smoothDelta + 0.001f)) / 2f;
		return num;
	}

	public float smoothDeltaTime()
	{
		deltaTime();
		return smoothDelta;
	}
}
