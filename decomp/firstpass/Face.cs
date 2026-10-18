using System.Collections.Generic;
using Brix.Game.Components;
using Brix.Lua;
using MoonSharp.Interpreter;
using UnityEngine;

[ExecuteInEditMode]
public class Face : UpdatableGameComponent<Face>
{
	public enum Expression
	{
		Blank,
		Happy,
		Sad,
		Pleased,
		Angry,
		Confused,
		Tired,
		Irritated,
		WTF,
		Incredulous,
		Confident,
		Fierce,
		Pouty,
		Rage
	}

	private struct ExpressionMapping
	{
		public float Eyes;

		public float Mouth;

		public bool OverrideBlink;
	}

	private Dictionary<Expression, ExpressionMapping> Expressions;

	public float Blink;

	public float BlinkInterval = 8f;

	public float BlinkIntervalVariance = 1f;

	public float BlinkDuration = 0.2f;

	public float DoubleBlinkProbability = 0.5f;

	public float DoubleBlinkInterval = 0.2f;

	private float TimeUntilNextBlink;

	private float CurrentBlinkTime;

	private bool NextBlinkIsDouble;

	private int eyesTileProperty;

	private int mouthTileProperty;

	private int blinkProperty;

	[LuaDrivable("expressionNames")]
	private string[] expressionNames;

	private bool Blinking;

	public Expression CurrentExpression;

	private Expression PreviousExpression;

	public GameObject Skin;

	private Material Material;

	protected override void Start()
	{
		base.Start();
		eyesTileProperty = Shader.PropertyToID("_EyesTile");
		mouthTileProperty = Shader.PropertyToID("_MouthTile");
		blinkProperty = Shader.PropertyToID("_Blink");
		BuildExpressionTable();
		PreviousExpression = CurrentExpression;
		ResetTimeUntilNextBlink();
		SetExpression(CurrentExpression);
	}

	protected override void Awake()
	{
		base.Awake();
		CurrentExpression = (Expression)Random.Range(0, 14);
	}

	public void ResetTimeUntilNextBlink()
	{
		TimeUntilNextBlink = BlinkInterval + Random.Range(0f - BlinkIntervalVariance, BlinkIntervalVariance);
		NextBlinkIsDouble = Random.value < DoubleBlinkProbability;
	}

	protected override void BrixFixedUpdate()
	{
		if (Skin == null)
		{
			return;
		}
		if (Expressions == null)
		{
			BuildExpressionTable();
		}
		if (!Blinking)
		{
			TimeUntilNextBlink -= Time.fixedDeltaTime;
			if (TimeUntilNextBlink <= 0f)
			{
				Material.SetFloat(blinkProperty, 1f);
				Blinking = true;
				CurrentBlinkTime = 0f;
			}
		}
		if (!Blinking)
		{
			return;
		}
		CurrentBlinkTime += Time.fixedDeltaTime;
		if (CurrentBlinkTime >= BlinkDuration)
		{
			Material.SetFloat(blinkProperty, 0f);
			Blinking = false;
			if (NextBlinkIsDouble)
			{
				TimeUntilNextBlink = DoubleBlinkInterval;
				NextBlinkIsDouble = false;
			}
			else
			{
				ResetTimeUntilNextBlink();
			}
		}
	}

	[ContextMenu("BuildExpressionTable")]
	public void BuildExpressionTable()
	{
		if (!(Skin == null))
		{
			Material = Skin.GetComponent<Renderer>().material;
			Expressions = new Dictionary<Expression, ExpressionMapping>();
			Expressions[Expression.Blank] = new ExpressionMapping
			{
				Eyes = 0f,
				Mouth = 0f
			};
			Expressions[Expression.Happy] = new ExpressionMapping
			{
				Eyes = 0f,
				Mouth = 0.175f
			};
			Expressions[Expression.Sad] = new ExpressionMapping
			{
				Eyes = 0.05f,
				Mouth = 0.05f
			};
			Expressions[Expression.Pleased] = new ExpressionMapping
			{
				Eyes = 0.05f,
				Mouth = 0.2f
			};
			Expressions[Expression.Angry] = new ExpressionMapping
			{
				Eyes = 0.15f,
				Mouth = 0.03f
			};
			Expressions[Expression.Confused] = new ExpressionMapping
			{
				Eyes = 0.05f,
				Mouth = 0.08f
			};
			Expressions[Expression.Tired] = new ExpressionMapping
			{
				Eyes = 0.1f,
				Mouth = 0.03f
			};
			Expressions[Expression.Irritated] = new ExpressionMapping
			{
				Eyes = 0.13f,
				Mouth = 0.05f
			};
			Expressions[Expression.WTF] = new ExpressionMapping
			{
				Eyes = 0.33f,
				Mouth = 0f
			};
			Expressions[Expression.Incredulous] = new ExpressionMapping
			{
				Eyes = 0.03f,
				Mouth = 0.03f
			};
			Expressions[Expression.Confident] = new ExpressionMapping
			{
				Eyes = 0.1f,
				Mouth = 0.18f
			};
			Expressions[Expression.Fierce] = new ExpressionMapping
			{
				Eyes = 0.33f,
				Mouth = 0.3f
			};
			Expressions[Expression.Pouty] = new ExpressionMapping
			{
				Eyes = 0.2f,
				Mouth = 0.08f
			};
			Expressions[Expression.Rage] = new ExpressionMapping
			{
				Eyes = 0.15f,
				Mouth = 0.3f
			};
		}
	}

	public void SetExpression(Expression expression)
	{
		if (!(Skin == null))
		{
			Material.SetFloat(eyesTileProperty, Expressions[expression].Eyes);
			Material.SetFloat(mouthTileProperty, Expressions[expression].Mouth);
		}
	}

	protected override void BrixUpdate()
	{
		if (Expressions == null)
		{
			BuildExpressionTable();
		}
		if (PreviousExpression != CurrentExpression)
		{
			SetExpression(CurrentExpression);
			PreviousExpression = CurrentExpression;
		}
	}

	public void FillExpressionList()
	{
		int num = 0;
		foreach (KeyValuePair<Expression, ExpressionMapping> expression in Expressions)
		{
			expressionNames[num] = expression.Key.ToString();
			num++;
		}
	}

	[LuaDrivable("ChangeBricktronFace")]
	public static DynValue lua_ChangeBricktronFace(ScriptExecutionContext executionContext, CallbackArguments args)
	{
		Face face = args.AsComponent<Face>(0, "ChangeBricktronFace");
		switch (args.AsString(1, "ChangeBricktronFace"))
		{
		case "Blank":
			face.CurrentExpression = Expression.Blank;
			face.SetExpression(face.CurrentExpression);
			face.PreviousExpression = face.CurrentExpression;
			break;
		case "Happy":
			face.CurrentExpression = Expression.Happy;
			face.SetExpression(face.CurrentExpression);
			face.PreviousExpression = face.CurrentExpression;
			break;
		case "Sad":
			face.CurrentExpression = Expression.Sad;
			face.SetExpression(face.CurrentExpression);
			face.PreviousExpression = face.CurrentExpression;
			break;
		case "Pleased":
			face.CurrentExpression = Expression.Pleased;
			face.SetExpression(face.CurrentExpression);
			face.PreviousExpression = face.CurrentExpression;
			break;
		case "Angry":
			face.CurrentExpression = Expression.Angry;
			face.SetExpression(face.CurrentExpression);
			face.PreviousExpression = face.CurrentExpression;
			break;
		case "Confused":
			face.CurrentExpression = Expression.Confused;
			face.SetExpression(face.CurrentExpression);
			face.PreviousExpression = face.CurrentExpression;
			break;
		case "Tired":
			face.CurrentExpression = Expression.Tired;
			face.SetExpression(face.CurrentExpression);
			face.PreviousExpression = face.CurrentExpression;
			break;
		case "Irritated":
			face.CurrentExpression = Expression.Irritated;
			face.SetExpression(face.CurrentExpression);
			face.PreviousExpression = face.CurrentExpression;
			break;
		case "WTF":
			face.CurrentExpression = Expression.WTF;
			face.SetExpression(face.CurrentExpression);
			face.PreviousExpression = face.CurrentExpression;
			break;
		case "Incredulous":
			face.CurrentExpression = Expression.Incredulous;
			face.SetExpression(face.CurrentExpression);
			face.PreviousExpression = face.CurrentExpression;
			break;
		case "Confident":
			face.CurrentExpression = Expression.Confident;
			face.SetExpression(face.CurrentExpression);
			face.PreviousExpression = face.CurrentExpression;
			break;
		case "Fierce":
			face.CurrentExpression = Expression.Fierce;
			face.SetExpression(face.CurrentExpression);
			face.PreviousExpression = face.CurrentExpression;
			break;
		case "Pouty":
			face.CurrentExpression = Expression.Pouty;
			face.SetExpression(face.CurrentExpression);
			face.PreviousExpression = face.CurrentExpression;
			break;
		case "Rage":
			face.CurrentExpression = Expression.Rage;
			face.SetExpression(face.CurrentExpression);
			face.PreviousExpression = face.CurrentExpression;
			break;
		}
		return DynValue.Void;
	}
}
