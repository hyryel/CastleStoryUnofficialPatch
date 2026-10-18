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
