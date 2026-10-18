using Brix.Utils.Closures;
using UnityEngine;

namespace Motus.Behavior;

public struct ConditionAppenderBuilder<TBuilder> : IBuilderBase where TBuilder : IConditionBuilder, IBuilderBase
{
	private readonly TBuilder _builder;

	public StaticNode ToNode => _builder.ToNode;

	public ConditionAppenderBuilder(TBuilder builder)
	{
		this = default(ConditionAppenderBuilder<TBuilder>);
		_builder = builder;
	}

	public ConditionAppenderBuilder(ConditionAppenderBuilder<TBuilder> builder)
	{
		this = default(ConditionAppenderBuilder<TBuilder>);
		_builder = builder._builder;
	}

	public AndConditionAppender<TBuilder> And()
	{
		return new AndConditionAppender<TBuilder>(_builder);
	}

	public OrConditionAppender<TBuilder> Or()
	{
		return new OrConditionAppender<TBuilder>(_builder);
	}

	public XorConditionAppender<TBuilder> Xor()
	{
		return new XorConditionAppender<TBuilder>(_builder);
	}

	public TBuilder Also()
	{
		return _builder;
	}

	public void SetFinalize(IClosure<IAction> a)
	{
		_builder.SetFinalize(a);
	}

	public void Label(string label, Object context)
	{
		_builder.Label(label, context);
	}

	public static implicit operator Node(ConditionAppenderBuilder<TBuilder> builder)
	{
		return builder.ToNode;
	}

	public static implicit operator StaticNode(ConditionAppenderBuilder<TBuilder> builder)
	{
		return builder.ToNode;
	}
}
