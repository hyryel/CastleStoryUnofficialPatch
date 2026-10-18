using System;
using System.Collections;
using System.Collections.Generic;
using Brix.Components;
using Brix.External.Factories;
using Brix.External.Factories.Templates;
using Brix.External.Signals;
using Brix.Game;
using Brix.Utils.Logging;
using Smooth.Slinq;
using Smooth.Slinq.Context;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Brix.Utils.Factories;

public class FactoryLoader : BrixSingleton<FactoryLoader>
{
	private static BrixLogger logger = new BrixLogger(typeof(FactoryLoader).FullName);

	public UnityEvent FactoriesLoadedEvent;

	public UnityEvent FactoriesReadyEvent;

	private SignalConnection FactoryCreatedConnection;

	public Signal FactoriesReadySignal = new Signal();

	[SerializeField]
	[HideInInspector]
	public List<string> _scenes;

	public IEnumerable<string> FactoryScenesNames
	{
		get
		{
			foreach (string s in _scenes)
			{
				int firstIndexOfSceneName = s.LastIndexOf('/') + 1;
				int lastIndexOfSceneName = s.LastIndexOf('.');
				int sceneNameLength = lastIndexOfSceneName - firstIndexOfSceneName;
				yield return s.Substring(firstIndexOfSceneName, sceneNameLength);
			}
		}
	}

	public IEnumerable<Scene> FactoryScenesLoaded
	{
		get
		{
			Slinq<Scene, PredicateContext<Scene, SelectContext<Scene, string, IEnumerableContext<string>>>> slinq = from s in FactoryScenesNames.Slinq().Select(SceneManager.GetSceneByName)
				where s.IsValid() && !IsThereALivingFactoryLeftInScene(s)
				select s;
			while (slinq.current.isSome)
			{
				yield return slinq.current.value;
				slinq.Skip();
			}
		}
	}

	public void EditorForcedLoad()
	{
	}

	protected override void Awake()
	{
		base.Awake();
		Factory.FactoryCreatedSignal.ConnectOnce(OnFirstFactoryCreated);
		FactoryCreatedConnection = Factory.FactoryCreatedSignal.Connect(delegate(Factory factory)
		{
			OnFactoryCreated(factory);
		});
		FactoriesReadyEvent.AddListener(delegate
		{
			FactoriesReadySignal.Invoke();
		});
	}

	protected override void Start()
	{
		base.Start();
		foreach (string factoryScenesName in FactoryScenesNames)
		{
			SceneManager.LoadScene(factoryScenesName, LoadSceneMode.Additive);
		}
		logger.Info("FactoryLoader requests done.");
	}

	private IEnumerator WaitForFactoriesLoaded()
	{
		yield return GameClock.WaitForEndOfFrame;
		FactoryScenesLoaded.Slinq().ForEach(delegate(Scene scene)
		{
			UnloadScene(scene.name);
		});
		GetComponents<IFactoryPostProcessor>().Slinq().ForEach(PostProcessFactories);
		yield return GameClock.WaitForEndOfFrame;
		FactoriesLoadedEvent.Invoke();
		yield return GameClock.WaitForEndOfFrame;
		foreach (Factory factory in Factory.GetFactories())
		{
			foreach (FactoryTemplate template in factory.Templates)
			{
				IBrixComponent[] components = template.GetComponents<IBrixComponent>();
				foreach (IBrixComponent brixComponent in components)
				{
					try
					{
						brixComponent.Initialize();
					}
					catch (Exception e)
					{
						logger.ErrorFormat(e, "Could not initialize IBrixComponent {0} for {1} in factory {2}", (brixComponent == null) ? "null" : brixComponent.GetType().Name, template, factory);
					}
				}
			}
		}
		FactoriesReadyEvent.Invoke();
		yield return GameClock.WaitForEndOfFrame;
	}

	private static void UnloadScene(string sceneName)
	{
		logger.DebugFormat("Unloading FactoryScene '{0}'", sceneName);
		SceneManager.UnloadSceneAsync(sceneName);
	}

	private static void PostProcessFactories(IFactoryPostProcessor fpp)
	{
		Factory.GetFactories().Slinq().ForEach(delegate(Factory f)
		{
			fpp.PostProcess(f);
		});
	}

	private void OnFirstFactoryCreated(Factory newFactory)
	{
		StartCoroutine(WaitForFactoriesLoaded());
	}

	private void OnFactoryCreated(Factory newFactory)
	{
		newFactory.transform.SetParent(base.transform);
	}

	private bool IsThereALivingFactoryLeftInScene(Scene scene)
	{
		return scene.GetRootGameObjects().Slinq().Any((GameObject go) => go.GetComponent<Factory>() != null);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		FactoryCreatedConnection.Disconnect();
	}
}
You are not using the latest version of the tool, please update.
Latest version is '11.1.0.9782' (yours is '9.1.0.7988')
