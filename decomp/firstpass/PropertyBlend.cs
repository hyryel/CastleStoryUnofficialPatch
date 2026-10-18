using UnityEngine;

public abstract class PropertyBlend : ScriptableObject
{
	public static string outputTypeDesc = string.Empty;

	public static string interpDesc = string.Empty;

	public bool show;

	[SerializeField]
	private float[] m_Ranges;

	public int outIdx;

	public float outPartialTraverse;

	public float[] ranges
	{
		get
		{
			return m_Ranges;
		}
		set
		{
			m_Ranges = value;
		}
	}

	public virtual string GetOutputTypeDesc()
	{
		return outputTypeDesc;
	}

	public virtual string GetInterpDesc()
	{
		return interpDesc;
	}

	public virtual PropertyBlend Init()
	{
		return this;
	}

	protected static int GetIdx(int size, float t, float[] ts)
	{
		int result = -1;
		for (int i = 0; i < size && i < ts.Length && i + 1 < ts.Length; i++)
		{
			if (ts[i] <= t && ts[i + 1] >= t)
			{
				result = i;
				break;
			}
		}
		return result;
	}
}
