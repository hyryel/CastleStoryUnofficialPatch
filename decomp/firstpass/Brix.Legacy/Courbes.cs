using UnityEngine;

namespace Brix.Legacy;

public class Courbes : MonoBehaviour
{
	public AnimationCurve Lerp;

	public AnimationCurve EaseIn;

	public AnimationCurve EaseInLong;

	public AnimationCurve EaseOut;

	public AnimationCurve EaseOutLong;

	public AnimationCurve BounceOut;

	public AnimationCurve BounceOutEaseIn;

	public AnimationCurve StepIn;

	public AnimationCurve StepOut;

	public AnimationCurve Stay;

	public static Courbes instance;

	public AnimationCurve GetCourbeParNom(string nom)
	{
		return nom switch
		{
			"Lerp" => Lerp, 
			"EaseIn" => EaseIn, 
			"EaseInLong" => EaseInLong, 
			"EaseOut" => EaseOut, 
			"EaseOutLong" => EaseOutLong, 
			"BounceOut" => BounceOut, 
			"BounceOutEaseIn" => BounceOutEaseIn, 
			"StepIn" => StepIn, 
			"StepOut" => StepOut, 
			"Stay" => Stay, 
			_ => AnimationCurve.Linear(0f, 0f, 1f, 1f), 
		};
	}

	public static Courbes Instantiate()
	{
		if (instance != null)
		{
			return instance;
		}
		instance = (Object.Instantiate(Resources.Load("Prefabs/Courbes/Courbes"), Vector3.zero, Quaternion.identity) as GameObject).GetComponent<Courbes>();
		return instance;
	}

	private void Start()
	{
		if (instance == null)
		{
			instance = this;
		}
		base.transform.SetParent(GameObject.Find("/Systems").transform);
	}

	public static AnimationCurve Get(string s)
	{
		if (instance == null)
		{
			instance = Instantiate();
		}
		return instance.GetCourbeParNom(s);
	}
}
