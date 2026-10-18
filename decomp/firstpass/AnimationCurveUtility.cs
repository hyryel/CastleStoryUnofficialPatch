using System;
using System.Reflection;
using UnityEngine;

public static class AnimationCurveUtility
{
	public enum TangentMode
	{
		Editable,
		Smooth,
		Linear,
		Stepped
	}

	public enum TangentDirection
	{
		Left,
		Right
	}

	public static void SetLinear(ref AnimationCurve curve)
	{
		Type typeFromHandle = typeof(Keyframe);
		FieldInfo field = typeFromHandle.GetField("m_TangentMode", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		for (int i = 0; i < curve.length; i++)
		{
			object obj = curve.keys[i];
			field.SetValue(obj, GetNewTangentKeyMode((int)field.GetValue(obj), TangentDirection.Left, TangentMode.Linear));
			field.SetValue(obj, GetNewTangentKeyMode((int)field.GetValue(obj), TangentDirection.Right, TangentMode.Linear));
			curve.MoveKey(i, (Keyframe)obj);
			curve.SmoothTangents(i, 0f);
		}
	}

	public static void SetStepped(ref AnimationCurve curve)
	{
		Type typeFromHandle = typeof(Keyframe);
		FieldInfo field = typeFromHandle.GetField("m_TangentMode", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		for (int i = 0; i < curve.length; i++)
		{
			object obj = curve.keys[i];
			field.SetValue(obj, GetNewTangentKeyMode((int)field.GetValue(obj), TangentDirection.Left, TangentMode.Stepped));
			field.SetValue(obj, GetNewTangentKeyMode((int)field.GetValue(obj), TangentDirection.Right, TangentMode.Stepped));
			curve.MoveKey(i, (Keyframe)obj);
			curve.SmoothTangents(i, 0f);
		}
	}

	public static int GetNewTangentKeyMode(int currentTangentMode, TangentDirection leftRight, TangentMode mode)
	{
		int num = currentTangentMode;
		if (leftRight == TangentDirection.Left)
		{
			num &= -7;
			return num | ((int)mode << 1);
		}
		num &= -25;
		return num | ((int)mode << 3);
	}
}
