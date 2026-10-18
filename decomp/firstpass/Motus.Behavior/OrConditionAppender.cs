using Brix.Utils.Closures;
using UnityEngine;

namespace Motus.Behavior;

public struct OrConditionAppender<TBuilder> : IConditionBuilder, IBuilderBase where TBuilder : IConditionBuilder, IBuilderBase
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
			builder.Condition = OrMultiCondition.Request(_builder.Condition, value);
		}
	}

	public StaticNode ToNode => _builder.ToNode;

	public OrConditionAppender(TBuilder builder)
	{
		this = default(OrConditionAppender<TBuilder>);
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

	public static implicit operator Node(OrConditionAppender<TBuilder> builder)
	{
		return builder.ToNode;
	}

	public static implicit operator StaticNode(OrConditionAppender<TBuilder> builder)
	{
		return builder.ToNode;
	}
}
