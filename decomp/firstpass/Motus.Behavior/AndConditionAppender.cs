using Brix.Utils.Closures;
using UnityEngine;

namespace Motus.Behavior;

public struct AndConditionAppender<TBuilder> : IConditionBuilder, IBuilderBase where TBuilder : IConditionBuilder, IBuilderBase
{
	private readonly TBuilder _builder;

	ICondition IConditionBuilder.Condition
	{
		get
		{
			return _builder.Condition;
		}
		set
		{
			TBuilder builder = _builder;
			builder.Condition = AndMultiCondition.Request(_builder.Condition, value);
		}
	}

	public StaticNode ToNode => _builder.ToNode;

	public AndConditionAppender(TBuilder builder)
	{
		this = default(AndConditionAppender<TBuilder>);
		_builder = builder;
	}

	public void SetFinalize(IClosure<IAction> a)
	{
		_builder.SetFinalize(a);
	}

	public void Label(string label, Object context)
	{
		_builder.Label(label, context);
	}

	public static implicit operator Node(AndConditionAppender<TBuilder> builder)
	{
		return builder.ToNode;
	}

	public static implicit operator StaticNode(AndConditionAppender<TBuilder> builder)
	{
		return builder.ToNode;
	}
}
