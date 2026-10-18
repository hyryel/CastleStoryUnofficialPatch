using Brix.Common;

namespace Motus.Behavior;

public static class ConditionNodeBuilderExtender
{
	public static ConditionNode.ConditionalNodeBuilder FailWhen<TBuilder, TException>(this TBuilder b, TException e) where TBuilder : IBuilderBase where TException : FailException
	{
		return Node.ConditionNode(b.ToNode, isStopCondition: true, e);
	}

	public static ConditionNode.ConditionalNodeBuilder FailUnless<TBuilder, TException>(this TBuilder b, TException e) where TBuilder : IBuilderBase where TException : FailException
	{
		return Node.ConditionNode(b.ToNode, isStopCondition: false, e);
	}

	public static ConditionNode.ConditionalNodeBuilder TerminateWhen<TBuilder>(this TBuilder b, ICondition c) where TBuilder : IBuilderBase
	{
		return Node.ConditionNode(b.ToNode, isStopCondition: true);
	}

	public static ConditionNode.ConditionalNodeBuilder TerminateUnless<TBuilder>(this TBuilder b, ICondition c) where TBuilder : IBuilderBase
	{
		return Node.ConditionNode(b.ToNode, isStopCondition: false);
	}

	public static ConditionNode.ConditionalNodeBuilder FailWhen<TException>(this Node n) where TException : FailException
	{
		return Node.ConditionNode(n, isStopCondition: true, typeof(TException).MakeInstance() as TException);
	}

	public static ConditionNode.ConditionalNodeBuilder FailWhen<TException>(this Node n, TException e) where TException : FailException
	{
		return Node.ConditionNode(n, isStopCondition: true, e);
	}

	public static ConditionNode.ConditionalNodeBuilder FailUnless<TException>(this Node n) where TException : FailException
	{
		return Node.ConditionNode(n, isStopCondition: false, typeof(TException).MakeInstance() as TException);
	}

	public static ConditionNode.ConditionalNodeBuilder FailUnless<TException>(this Node n, TException e) where TException : FailException
	{
		return Node.ConditionNode(n, isStopCondition: false, e);
	}

	public static ConditionNode.ConditionalNodeBuilder TerminateWhen(this Node n, ICondition c)
	{
		return Node.ConditionNode(n, isStopCondition: true);
	}

	public static ConditionNode.ConditionalNodeBuilder TerminateUnless(this Node n, ICondition c)
	{
		return Node.ConditionNode(n, isStopCondition: false);
	}

	public static ConditionNode.ConditionalNodeBuilder FailWhen<TException>(this StaticNode n) where TException : FailException
	{
		return Node.ConditionNode(n, isStopCondition: true, typeof(TException).MakeInstance() as TException);
	}

	public static ConditionNode.ConditionalNodeBuilder FailWhen<TException>(this StaticNode n, TException e) where TException : FailException
	{
		return Node.ConditionNode(n, isStopCondition: true, e);
	}

	public static ConditionNode.ConditionalNodeBuilder FailUnless<TException>(this StaticNode n) where TException : FailException
	{
		return Node.ConditionNode(n, isStopCondition: false, typeof(TException).MakeInstance() as TException);
	}

	public static ConditionNode.ConditionalNodeBuilder FailUnless<TException>(this StaticNode n, TException e) where TException : FailException
	{
		return Node.ConditionNode(n, isStopCondition: false, e);
	}

	public static ConditionNode.ConditionalNodeBuilder TerminateWhen(this StaticNode n, ICondition c)
	{
		return Node.ConditionNode(n, isStopCondition: true);
	}

	public static ConditionNode.ConditionalNodeBuilder TerminateUnless(this StaticNode n, ICondition c)
	{
		return Node.ConditionNode(n, isStopCondition: false);
	}
}
