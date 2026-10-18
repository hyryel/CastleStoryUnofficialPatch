using UnityEngine;

namespace Brix.Core;

public interface IGeometrie
{
	Vector3 GetSommet(int i);

	Vector3 GetNormale(int i);

	Vector3 GetTangente(int i);
}
