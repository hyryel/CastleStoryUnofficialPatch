using Brix.External.Signals;
using UnityEngine;

namespace Brix.Utils.Factories;

public class FactorySignalInvoker : MonoBehaviour
{
	public static Signal OnFactoriesLoadedSig = new Signal();

	public static Signal OnFactoriesReadySig = new Signal();

	public void OnFactoriesLoaded()
	{
		OnFactoriesLoadedSig.Invoke();
	}

	public void OnFactoriesReady()
	{
		OnFactoriesReadySig.Invoke();
	}
}
You are not using the latest version of the tool, please update.
Latest version is '11.1.0.9782' (yours is '9.1.0.7988')
