using System;
using UnityEngine;

namespace UnityTest;

[Serializable]
public class TestResult : ITestResult, IComparable<TestResult>
{
	public enum ResultType
	{
		Success,
		Failed,
		Timeout,
		NotRun,
		FailedException,
		Ignored
	}

	private readonly GameObject m_Go;

	private string m_Name;

	public ResultType resultType = ResultType.NotRun;

	public double duration;

	public string messages;

	public string stacktrace;

	public string id;

	public bool dynamicTest;

	public TestComponent TestComponent;

	public GameObject GameObject => m_Go;

	public TestResultState ResultState => resultType switch
	{
		ResultType.Success => TestResultState.Success, 
		ResultType.Failed => TestResultState.Failure, 
		ResultType.FailedException => TestResultState.Error, 
		ResultType.Ignored => TestResultState.Ignored, 
		ResultType.NotRun => TestResultState.Skipped, 
		ResultType.Timeout => TestResultState.Cancelled, 
		_ => throw new Exception(), 
	};

	public string Message => messages;

	public string Logs => null;

	public bool Executed => resultType != ResultType.NotRun;

	public string Name
	{
		get
		{
			if (m_Go != null)
			{
				m_Name = m_Go.name;
			}
			return m_Name;
		}
	}

	public string Id => id;

	public bool IsSuccess => resultType == ResultType.Success;

	public bool IsTimeout => resultType == ResultType.Timeout;

	public double Duration => duration;

	public string StackTrace => stacktrace;

	public string FullName
	{
		get
		{
			string text = Name;
			if (m_Go != null)
			{
				Transform parent = m_Go.transform.parent;
				while (parent != null)
				{
					text = parent.name + "." + text;
					parent = parent.transform.parent;
				}
			}
			return text;
		}
	}

	public bool IsIgnored => resultType == ResultType.Ignored;

	public bool IsFailure => resultType == ResultType.Failed || resultType == ResultType.FailedException || resultType == ResultType.Timeout;

	public TestResult(TestComponent testComponent)
	{
		TestComponent = testComponent;
		m_Go = testComponent.gameObject;
		id = testComponent.gameObject.GetInstanceID().ToString();
		dynamicTest = testComponent.dynamic;
		if (m_Go != null)
		{
			m_Name = m_Go.name;
		}
		if (dynamicTest)
		{
			id = testComponent.dynamicTypeName;
		}
	}

	public void Update(TestResult oldResult)
	{
		resultType = oldResult.resultType;
		duration = oldResult.duration;
		messages = oldResult.messages;
		stacktrace = oldResult.stacktrace;
	}

	public void Reset()
	{
		resultType = ResultType.NotRun;
		duration = 0.0;
		messages = string.Empty;
		stacktrace = string.Empty;
	}

	public override int GetHashCode()
	{
		return id.GetHashCode();
	}

	public int CompareTo(TestResult other)
	{
		int num = Name.CompareTo(other.Name);
		if (num == 0)
		{
			num = m_Go.GetInstanceID().CompareTo(other.m_Go.GetInstanceID());
		}
		return num;
	}

	public override bool Equals(object obj)
	{
		if (obj is TestResult)
		{
			return GetHashCode() == obj.GetHashCode();
		}
		return base.Equals(obj);
	}
}
