using System;

namespace UnityTest;

public class AssertionException : Exception
{
	private readonly AssertionComponent m_Assertion;

	public override string StackTrace => "Created in " + m_Assertion.GetCreationLocation();

	public AssertionException(AssertionComponent assertion)
		: base(assertion.Action.GetFailureMessage())
	{
		m_Assertion = assertion;
	}
}
