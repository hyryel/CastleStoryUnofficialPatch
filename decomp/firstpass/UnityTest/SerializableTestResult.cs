using System;

namespace UnityTest;

[Serializable]
internal class SerializableTestResult : ITestResult
{
	public TestResultState resultState;

	public string message;

	public bool executed;

	public string name;

	public string fullName;

	public string id;

	public bool isSuccess;

	public double duration;

	public string stackTrace;

	public bool isIgnored;

	public TestResultState ResultState => resultState;

	public string Message => message;

	public string Logs => null;

	public bool Executed => executed;

	public string Name => name;

	public string FullName => fullName;

	public string Id => id;

	public bool IsSuccess => isSuccess;

	public double Duration => duration;

	public string StackTrace => stackTrace;

	public bool IsIgnored => isIgnored;
}
