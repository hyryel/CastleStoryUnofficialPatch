using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityTest.IntegrationTestRunner;

namespace UnityTest;

[Serializable]
public class TestRunner : MonoBehaviour
{
	private enum TestState
	{
		Running,
		Success,
		Failure,
		Exception,
		Timeout,
		Ignored
	}

	private static int TestSceneNumber = 0;

	private static readonly TestResultRenderer k_ResultRenderer = new TestResultRenderer();

	public TestComponent currentTest;

	private List<TestResult> m_ResultList = new List<TestResult>();

	private List<TestComponent> m_TestComponents;

	private double m_StartTime;

	private bool m_ReadyToRun;

	private string m_TestMessages;

	private string m_Stacktrace;

	private TestState m_TestState;

	private TestRunnerConfigurator m_Configurator;

	public TestRunnerCallbackList TestRunnerCallback = new TestRunnerCallbackList();

	private IntegrationTestsProvider m_TestsProvider;

	private const string k_Prefix = "IntegrationTest";

	private const string k_StartedMessage = "IntegrationTest Started";

	private const string k_FinishedMessage = "IntegrationTest Finished";

	private const string k_TimeoutMessage = "IntegrationTest Timeout";

	private const string k_FailedMessage = "IntegrationTest Failed";

	private const string k_FailedExceptionMessage = "IntegrationTest Failed with exception";

	private const string k_IgnoredMessage = "IntegrationTest Ignored";

	private const string k_InterruptedMessage = "IntegrationTest Run interrupted";

	public bool isInitializedByRunner
	{
		get
		{
			if (Application.isEditor && !IsBatchMode())
			{
				return true;
			}
			return false;
		}
	}

	public void Awake()
	{
		m_Configurator = new TestRunnerConfigurator();
		if (!isInitializedByRunner)
		{
			TestComponent.DisableAllTests();
		}
	}

	public void Start()
	{
		if (isInitializedByRunner)
		{
			return;
		}
		if (m_Configurator.sendResultsOverNetwork)
		{
			ITestRunnerCallback testRunnerCallback = m_Configurator.ResolveNetworkConnection();
			if (testRunnerCallback != null)
			{
				TestRunnerCallback.Add(testRunnerCallback);
			}
		}
		TestComponent.DestroyAllDynamicTests();
		IEnumerable<Type> typesWithHelpAttribute = TestComponent.GetTypesWithHelpAttribute(SceneManager.GetActiveScene().name);
		foreach (Type item in typesWithHelpAttribute)
		{
			TestComponent.CreateDynamicTest(item);
		}
		List<TestComponent> tests = TestComponent.FindAllTestsOnScene();
		InitRunner(tests, typesWithHelpAttribute.Select((Type type) => type.AssemblyQualifiedName).ToList());
	}

	public void InitRunner(List<TestComponent> tests, List<string> dynamicTestsToRun)
	{
		Application.logMessageReceived += LogHandler;
		foreach (string item in dynamicTestsToRun)
		{
			Type type = Type.GetType(item);
			if (type == null)
			{
				continue;
			}
			MonoBehaviour[] array = Resources.FindObjectsOfTypeAll(type) as MonoBehaviour[];
			if (array.Length == 0)
			{
				Debug.LogWarning(string.Concat(type, " not found. Skipping."));
				continue;
			}
			if (array.Length > 1)
			{
				Debug.LogWarning("Multiple GameObjects refer to " + item);
			}
			tests.Add(array.First().GetComponent<TestComponent>());
		}
		m_TestComponents = ParseListForGroups(tests).ToList();
		m_ResultList = m_TestComponents.Select((TestComponent component) => new TestResult(component)).ToList();
		m_TestsProvider = new IntegrationTestsProvider(((IEnumerable<TestResult>)m_ResultList).Select((Func<TestResult, ITestComponent>)((TestResult result) => result.TestComponent)));
		m_ReadyToRun = true;
	}

	private static IEnumerable<TestComponent> ParseListForGroups(IEnumerable<TestComponent> tests)
	{
		HashSet<TestComponent> hashSet = new HashSet<TestComponent>();
		foreach (TestComponent testResult in tests)
		{
			if (testResult.IsTestGroup())
			{
				TestComponent[] array = (from t in testResult.gameObject.GetComponentsInChildren(typeof(TestComponent), includeInactive: true)
					where t != testResult
					select t).Cast<TestComponent>().ToArray();
				TestComponent[] array2 = array;
				foreach (TestComponent testComponent in array2)
				{
					if (!testComponent.IsTestGroup())
					{
						hashSet.Add(testComponent);
					}
				}
			}
			else
			{
				hashSet.Add(testResult);
			}
		}
		return hashSet;
	}

	public void Update()
	{
		if (m_ReadyToRun && Time.frameCount > 1)
		{
			m_ReadyToRun = false;
			StartCoroutine("StateMachine");
		}
	}

	public void OnDestroy()
	{
		if (currentTest != null)
		{
			TestResult testResult = m_ResultList.Single((TestResult result) => result.TestComponent == currentTest);
			testResult.messages += "Test run interrupted (crash?)";
			LogMessage("IntegrationTest Run interrupted");
			FinishTest(TestResult.ResultType.Failed);
		}
		if (currentTest != null || (m_TestsProvider != null && m_TestsProvider.AnyTestsLeft()))
		{
			List<ITestComponent> remainingTests = m_TestsProvider.GetRemainingTests();
			TestRunnerCallback.TestRunInterrupted(remainingTests.ToList());
		}
		Application.logMessageReceived -= LogHandler;
	}

	private void LogHandler(string condition, string stacktrace, LogType type)
	{
		if (!condition.StartsWith("IntegrationTest Started") && !condition.StartsWith("IntegrationTest Finished"))
		{
			string text = condition;
			if (text.StartsWith("IntegrationTest"))
			{
				text = text.Substring("IntegrationTest".Length + 1);
			}
			if (currentTest != null && text.EndsWith("(" + currentTest.name + ')'))
			{
				text = text.Substring(0, text.LastIndexOf('('));
			}
			m_TestMessages = m_TestMessages + text + "\n";
		}
		switch (type)
		{
		case LogType.Exception:
		{
			string text2 = condition.Substring(0, condition.IndexOf(':'));
			if (currentTest != null && currentTest.IsExceptionExpected(text2))
			{
				m_TestMessages = m_TestMessages + text2 + " was expected\n";
				if (currentTest.ShouldSucceedOnException())
				{
					m_TestState = TestState.Success;
				}
			}
			else
			{
				m_TestState = TestState.Exception;
				m_Stacktrace = stacktrace;
			}
			break;
		}
		case LogType.Error:
		case LogType.Assert:
			m_TestState = TestState.Failure;
			m_Stacktrace = stacktrace;
			break;
		case LogType.Log:
			if (m_TestState == TestState.Running && condition.StartsWith("IntegrationTest Pass"))
			{
				m_TestState = TestState.Success;
			}
			if (condition.StartsWith("IntegrationTest Fail"))
			{
				m_TestState = TestState.Failure;
			}
			break;
		case LogType.Warning:
			break;
		}
	}

	public IEnumerator StateMachine()
	{
		TestRunnerCallback.RunStarted(Application.platform.ToString(), m_TestComponents);
		while (m_TestsProvider.AnyTestsLeft() || !(currentTest == null))
		{
			if (currentTest == null)
			{
				StartNewTest();
			}
			if (currentTest != null)
			{
				if (m_TestState == TestState.Running)
				{
					if (currentTest.ShouldSucceedOnAssertions())
					{
						AssertionComponent[] source = (from a in currentTest.gameObject.GetComponentsInChildren<AssertionComponent>()
							where a.enabled
							select a).ToArray();
						if (source.Any() && source.All((AssertionComponent a) => a.checksPerformed > 0))
						{
							IntegrationTest.Pass(currentTest.gameObject);
							m_TestState = TestState.Success;
						}
					}
					if (currentTest != null && (double)Time.time > m_StartTime + currentTest.GetTimeout())
					{
						m_TestState = TestState.Timeout;
					}
				}
				switch (m_TestState)
				{
				case TestState.Success:
					LogMessage("IntegrationTest Finished");
					FinishTest(TestResult.ResultType.Success);
					break;
				case TestState.Failure:
					LogMessage("IntegrationTest Failed");
					FinishTest(TestResult.ResultType.Failed);
					break;
				case TestState.Exception:
					LogMessage("IntegrationTest Failed with exception");
					FinishTest(TestResult.ResultType.FailedException);
					break;
				case TestState.Timeout:
					LogMessage("IntegrationTest Timeout");
					FinishTest(TestResult.ResultType.Timeout);
					break;
				case TestState.Ignored:
					LogMessage("IntegrationTest Ignored");
					FinishTest(TestResult.ResultType.Ignored);
					break;
				}
			}
			yield return null;
		}
		FinishTestRun();
	}

	private void LogMessage(string message)
	{
		if (currentTest != null)
		{
			Debug.Log(message + " (" + currentTest.Name + ")", currentTest.gameObject);
		}
		else
		{
			Debug.Log(message);
		}
	}

	private void FinishTestRun()
	{
		PrintResultToLog();
		TestRunnerCallback.RunFinished(m_ResultList);
		LoadNextLevelOrQuit();
	}

	private void PrintResultToLog()
	{
		string empty = string.Empty;
		empty = empty + "Passed: " + m_ResultList.Count((TestResult t) => t.IsSuccess);
		if (m_ResultList.Any((TestResult result) => result.IsFailure))
		{
			empty = empty + " Failed: " + m_ResultList.Count((TestResult t) => t.IsFailure);
			Debug.Log("Failed tests: " + string.Join(", ", (from result in m_ResultList
				where result.IsFailure
				select result.Name).ToArray()));
		}
		if (m_ResultList.Any((TestResult result) => result.IsIgnored))
		{
			empty = empty + " Ignored: " + m_ResultList.Count((TestResult t) => t.IsIgnored);
			Debug.Log("Ignored tests: " + string.Join(", ", (from result in m_ResultList
				where result.IsIgnored
				select result.Name).ToArray()));
		}
		Debug.Log(empty);
	}

	private void LoadNextLevelOrQuit()
	{
		if (isInitializedByRunner)
		{
			return;
		}
		TestSceneNumber++;
		string integrationTestScenes = m_Configurator.GetIntegrationTestScenes(TestSceneNumber);
		if (integrationTestScenes != null)
		{
			SceneManager.LoadScene(Path.GetFileNameWithoutExtension(integrationTestScenes));
			return;
		}
		TestRunnerCallback.AllScenesFinished();
		k_ResultRenderer.ShowResults();
		if (m_Configurator.isBatchRun && m_Configurator.sendResultsOverNetwork)
		{
			Application.Quit();
		}
	}

	public void OnGUI()
	{
		k_ResultRenderer.Draw();
	}

	private void StartNewTest()
	{
		m_TestMessages = string.Empty;
		m_Stacktrace = string.Empty;
		m_TestState = TestState.Running;
		m_StartTime = Time.time;
		currentTest = m_TestsProvider.GetNextTest() as TestComponent;
		TestResult test = m_ResultList.Single((TestResult result) => result.TestComponent == currentTest);
		if (currentTest != null && currentTest.IsExludedOnThisPlatform())
		{
			m_TestState = TestState.Ignored;
			Debug.Log(currentTest.gameObject.name + " is excluded on this platform");
		}
		if (currentTest != null && currentTest.IsIgnored() && (!isInitializedByRunner || m_ResultList.Count != 1))
		{
			m_TestState = TestState.Ignored;
		}
		LogMessage("IntegrationTest Started");
		TestRunnerCallback.TestStarted(test);
	}

	private void FinishTest(TestResult.ResultType result)
	{
		m_TestsProvider.FinishTest(currentTest);
		TestResult testResult = m_ResultList.Single((TestResult t) => t.GameObject == currentTest.gameObject);
		testResult.resultType = result;
		testResult.duration = (double)Time.time - m_StartTime;
		testResult.messages = m_TestMessages;
		testResult.stacktrace = m_Stacktrace;
		TestRunnerCallback.TestFinished(testResult);
		currentTest = null;
		if (!testResult.IsSuccess && testResult.Executed && !testResult.IsIgnored)
		{
			k_ResultRenderer.AddResults(SceneManager.GetActiveScene().name, testResult);
		}
	}

	public static TestRunner GetTestRunner()
	{
		TestRunner result = null;
		UnityEngine.Object[] array = Resources.FindObjectsOfTypeAll(typeof(TestRunner));
		if (array.Count() <= 1)
		{
			result = (array.Any() ? (array.Single() as TestRunner) : Create().GetComponent<TestRunner>());
		}
		else
		{
			UnityEngine.Object[] array2 = array;
			foreach (UnityEngine.Object obj in array2)
			{
				UnityEngine.Object.DestroyImmediate(((TestRunner)obj).gameObject);
			}
		}
		return result;
	}

	private static GameObject Create()
	{
		GameObject gameObject = new GameObject("TestRunner");
		gameObject.AddComponent<TestRunner>();
		Debug.Log("Created Test Runner");
		return gameObject;
	}

	private static bool IsBatchMode()
	{
		Type type = Type.GetType("UnityEditorInternal.InternalEditorUtility, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", throwOnError: false);
		if (type == null)
		{
			return false;
		}
		PropertyInfo property = type.GetProperty("inBatchMode");
		return (bool)property.GetValue(null, null);
	}
}
