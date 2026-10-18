using UnityEngine;

namespace Brix.Utils;

public class MeshSaver : MonoBehaviour
{
	public bool SaveMeMyLord;

	private void Update()
	{
		if (SaveMeMyLord)
		{
			SaveMeMyLord = false;
		}
	}
}
