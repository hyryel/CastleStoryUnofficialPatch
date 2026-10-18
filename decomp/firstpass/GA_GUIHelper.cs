using UnityEngine;

public class GA_GUIHelper
{
	protected static bool clippingEnabled;

	protected static Rect clippingBounds;

	protected static Material lineMaterial;

	protected static bool clip_test(float p, float q, ref float u1, ref float u2)
	{
		bool result = true;
		if ((double)p < 0.0)
		{
			float num = q / p;
			if (num > u2)
			{
				result = false;
			}
			else if (num > u1)
			{
				u1 = num;
			}
		}
		else if ((double)p > 0.0)
		{
			float num = q / p;
			if (num < u1)
			{
				result = false;
			}
			else if (num < u2)
			{
				u2 = num;
			}
		}
		else if ((double)q < 0.0)
		{
			result = false;
		}
		return result;
	}

	protected static bool segment_rect_intersection(Rect bounds, ref Vector2 p1, ref Vector2 p2)
	{
		float u = 0f;
		float u2 = 1f;
		float num = p2.x - p1.x;
		if (clip_test(0f - num, p1.x - bounds.xMin, ref u, ref u2) && clip_test(num, bounds.xMax - p1.x, ref u, ref u2))
		{
			float num2 = p2.y - p1.y;
			if (clip_test(0f - num2, p1.y - bounds.yMin, ref u, ref u2) && clip_test(num2, bounds.yMax - p1.y, ref u, ref u2))
			{
				if ((double)u2 < 1.0)
				{
					p2.x = p1.x + u2 * num;
					p2.y = p1.y + u2 * num2;
				}
				if ((double)u > 0.0)
				{
					p1.x += u * num;
					p1.y += u * num2;
				}
				return true;
			}
		}
		return false;
	}

	public static void BeginGroup(Rect position)
	{
		clippingEnabled = true;
		clippingBounds = new Rect(0f, 0f, position.width, position.height);
		GUI.BeginGroup(position);
	}

	public static void EndGroup()
	{
		GUI.EndGroup();
		clippingBounds = new Rect(0f, 0f, Screen.width, Screen.height);
		clippingEnabled = false;
	}

	public static void DrawLine(Vector2 pointA, Vector2 pointB, Color color)
	{
		if (Event.current != null && Event.current.type == EventType.Repaint && (!clippingEnabled || segment_rect_intersection(clippingBounds, ref pointA, ref pointB)))
		{
			if (!lineMaterial)
			{
				lineMaterial = new Material("Shader \"Lines/Colored Blended\" {SubShader { Pass {   BindChannels { Bind \"Color\",color }   Blend SrcAlpha OneMinusSrcAlpha   ZWrite Off Cull Off Fog { Mode Off }} } }");
				lineMaterial.hideFlags = HideFlags.HideAndDontSave;
				lineMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
			}
			lineMaterial.SetPass(0);
			GL.Begin(1);
			GL.Color(color);
			GL.Vertex3(pointA.x, pointA.y, 0f);
			GL.Vertex3(pointB.x, pointB.y, 0f);
			GL.End();
		}
	}
}
