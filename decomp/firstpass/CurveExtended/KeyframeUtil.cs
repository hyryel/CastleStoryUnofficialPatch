using System;
using System.Reflection;
using Brix.Utils.Logging;
using UnityEngine;

namespace CurveExtended;

public class KeyframeUtil
{
	private static BrixLogger logger = new BrixLogger(typeof(KeyframeUtil).FullName);

	public static Keyframe GetNew(float time, float value, TangentMode leftAndRight)
	{
		return GetNew(time, value, leftAndRight, leftAndRight);
	}

	public static Keyframe GetNew(float time, float value, TangentMode left, TangentMode right)
	{
		object obj = new Keyframe(time, value);
		SetKeyBroken(obj, broken: true);
		SetKeyTangentMode(obj, 0, left);
		SetKeyTangentMode(obj, 1, right);
		Keyframe result = (Keyframe)obj;
		if (left == TangentMode.Stepped)
		{
			result.inTangent = float.PositiveInfinity;
		}
		if (right == TangentMode.Stepped)
		{
			result.outTangent = float.PositiveInfinity;
		}
		return result;
	}

	public static void SetKeyTangentMode(object keyframe, int leftRight, TangentMode mode)
	{
		Type typeFromHandle = typeof(Keyframe);
		FieldInfo field = typeFromHandle.GetField("m_TangentMode", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		int num = (int)field.GetValue(keyframe);
		if (leftRight == 0)
		{
			num &= -7;
			num |= (int)mode << 1;
		}
		else
		{
			num &= -25;
			num |= (int)mode << 3;
		}
		field.SetValue(keyframe, num);
		if (GetKeyTangentMode(num, leftRight) != mode)
		{
			logger.Info("bug");
		}
	}

	public static TangentMode GetKeyTangentMode(int tangentMode, int leftRight)
	{
		if (leftRight == 0)
		{
			return (TangentMode)((tangentMode & 6) >> 1);
		}
		return (TangentMode)((tangentMode & 0x18) >> 3);
	}

	public static TangentMode GetKeyTangentMode(Keyframe keyframe, int leftRight)
	{
		Type typeFromHandle = typeof(Keyframe);
		FieldInfo field = typeFromHandle.GetField("m_TangentMode", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		int num = (int)field.GetValue(keyframe);
		if (leftRight == 0)
		{
			return (TangentMode)((num & 6) >> 1);
		}
		return (TangentMode)((num & 0x18) >> 3);
	}

	public static void SetKeyBroken(object keyframe, bool broken)
	{
		Type typeFromHandle = typeof(Keyframe);
		FieldInfo field = typeFromHandle.GetField("m_TangentMode", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		int num = (int)field.GetValue(keyframe);
		num = ((!broken) ? (num & -2) : (num | 1));
		field.SetValue(keyframe, num);
	}
}
