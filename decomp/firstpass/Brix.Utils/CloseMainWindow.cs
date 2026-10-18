using System.Collections;
using System.Diagnostics;
using UnityEngine;

namespace Brix.Utils;

public class CloseMainWindow : MonoBehaviour
{
	public IEnumerator Start()
	{
		yield return null;
		yield return null;
		Process.GetCurrentProcess().CloseMainWindow();
	}
}
