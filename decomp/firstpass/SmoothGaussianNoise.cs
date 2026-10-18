using System;
using UnityEngine;

public class SmoothGaussianNoise : MonoBehaviour
{
	public delegate float ParametrizedFunc(float x);

	public float NoiseF = 0.2f;

	[SerializeField]
	private bool debug;

	[SerializeField]
	private float delayAverage;

	[SerializeField]
	private float delayStdDeviation;

	[SerializeField]
	private float intensityAverage;

	[SerializeField]
	private float intensityStdDeviation;

	private Vector2 current = Vector2.zero;

	private Vector2 next = Vector2.zero;

	public ParametrizedFunc smoothingFunc;

	private System.Random random = new System.Random();

	private void Start()
	{
		OnPointReached();
	}

	private void Mark(Vector2 pos, Color color, float size)
	{
		GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		gameObject.transform.position = pos;
		gameObject.transform.localScale = Vector3.one * size;
		gameObject.GetComponent<Renderer>().material.color = color;
	}

	private void Update()
	{
		NoiseF = smoothingFunc(Time.time);
		if (debug)
		{
			Mark(new Vector3(Time.time, NoiseF), Color.white, 0.1f);
		}
		if (Time.fixedTime >= next.x)
		{
			OnPointReached();
		}
	}

	public float GenerateIntensity()
	{
		return NextGaussianDouble(random) * intensityStdDeviation + intensityAverage;
	}

	public float GenerateDelay()
	{
		return NextGaussianDouble(random) * delayStdDeviation + delayAverage;
	}

	public void OnPointReached()
	{
		if (next == Vector2.zero)
		{
			current.x = Time.time;
			next = GenerateNextPoint();
		}
		current = next;
		next = GenerateNextPoint();
		smoothingFunc = LogisticBetweenPoints(current, next);
		if (debug)
		{
			Mark(next, Color.red, 0.3f);
		}
	}

	public Vector2 GenerateNextPoint()
	{
		float num;
		do
		{
			num = GenerateDelay();
		}
		while (num < 0f);
		float num2;
		do
		{
			num2 = GenerateIntensity();
		}
		while (num2 < 0f);
		return new Vector2(current.x + num, num2);
	}

	public ParametrizedFunc LogisticBetweenPoints(Vector2 p1, Vector2 p2)
	{
		Vector2 midPoint = (p1 + p2) / 2f;
		Vector2 diff = p2 - p1;
		return (float x) => LogisticFunction(x, 8f / diff.x, diff.y, midPoint.x) + p1.y;
	}

	public float LogisticFunction(float x, float stepness, float YRange, float xMidPoint)
	{
		return (float)((double)YRange / (1.0 + Math.Exp((0f - stepness) * (x - xMidPoint))));
	}

	public float NextGaussianDouble(System.Random r)
	{
		double num;
		double num3;
		do
		{
			num = 2.0 * r.NextDouble() - 1.0;
			double num2 = 2.0 * r.NextDouble() - 1.0;
			num3 = num * num + num2 * num2;
		}
		while (num3 >= 1.0);
		double num4 = Math.Sqrt(-2.0 * Math.Log(num3) / num3);
		return (float)(num * num4);
	}
}
