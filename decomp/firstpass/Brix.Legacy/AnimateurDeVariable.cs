using System;
using System.Reflection;
using UnityEngine;

namespace Brix.Legacy;

[ExecuteInEditMode]
public class AnimateurDeVariable : MonoBehaviour
{
	public MonoBehaviour target;

	public string variable;

	public float animDuree;

	public string animCourbe;

	public AnimationCurve animCurve;

	public bool anim;

	public bool ready;

	public bool editorTime;

	private float animDebut;

	private float animFin;

	private object value;

	private object valueDebut;

	private object valueFin;

	private Func<object, object, float, object> objectValueLerp;

	private Func<float, float, float, float> floatValueLerp;

	private Func<Vector3, Vector3, float, Vector3> vector3ValueLerp;

	private Func<Color, Color, float, Color> colorValueLerp;

	private void Start()
	{
		value = null;
		ready = false;
	}

	private void LateUpdate()
	{
		float time = Time.time;
		if (!ready || value == null)
		{
			value = GetValue();
			anim = false;
			ready = true;
		}
		object obj = GetValue();
		if (obj != value)
		{
			anim = true;
			animCurve = Courbes.Get(animCourbe);
			animDebut = time;
			animFin = animDebut + animDuree;
			valueDebut = value;
			valueFin = obj;
			if (value.GetType() == typeof(float))
			{
				objectValueLerp = null;
				floatValueLerp = Mathf.Lerp;
				vector3ValueLerp = null;
				colorValueLerp = null;
			}
			else if (value.GetType() == typeof(Vector3))
			{
				objectValueLerp = null;
				floatValueLerp = null;
				vector3ValueLerp = Vector3.Lerp;
				colorValueLerp = null;
			}
			else if (value.GetType() == typeof(Color))
			{
				objectValueLerp = null;
				floatValueLerp = null;
				vector3ValueLerp = null;
				colorValueLerp = Color.Lerp;
			}
			else
			{
				objectValueLerp = (object a, object b, float t) => (!(t < 1f)) ? b : a;
				floatValueLerp = null;
				vector3ValueLerp = null;
				colorValueLerp = null;
			}
		}
		if (!anim)
		{
			return;
		}
		if (time >= animFin)
		{
			value = valueFin;
			SetValue(value);
			anim = false;
			return;
		}
		float arg = animCurve.Evaluate((time - animDebut) / (animFin - animDebut));
		if (objectValueLerp != null)
		{
			value = objectValueLerp(valueDebut, valueFin, arg);
		}
		else if (floatValueLerp != null)
		{
			value = floatValueLerp((float)valueDebut, (float)valueFin, arg);
		}
		else if (vector3ValueLerp != null)
		{
			value = vector3ValueLerp((Vector3)valueDebut, (Vector3)valueFin, arg);
		}
		else if (colorValueLerp != null)
		{
			value = colorValueLerp((Color)valueDebut, (Color)valueFin, arg);
		}
		SetValue(value);
	}

	public object GetValue()
	{
		if (target != null)
		{
			FieldInfo field = target.GetType().GetField(variable);
			if (field != null)
			{
				return field.GetValue(target);
			}
		}
		return value;
	}

	public void SetValue(object value)
	{
		if (target != null)
		{
			FieldInfo field = target.GetType().GetField(variable);
			if (field != null)
			{
				field.SetValue(target, value);
				return;
			}
		}
		this.value = value;
	}
}
