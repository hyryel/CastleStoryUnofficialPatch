using Brix.Input;
using Brix.Util;
using UnityEngine;

namespace Brix.Game.Components;

internal class AlphabetFitter : AbstractFitter
{
	public override void FitAndOrient(VoxelRaycastHit voxelHit, out Vector3 absolutePosition)
	{
		VoxelTransform component = GetComponent<VoxelTransform>();
		Vector3 offsettedPosition = component.GetOffsettedPosition(voxelHit.WorldHitVector);
		base.gameObject.transform.rotation = _rotation;
		base.gameObject.transform.position = offsettedPosition;
		absolutePosition = offsettedPosition;
	}

	public override void ChangeFit()
	{
		float y = Camera.main.transform.rotation.eulerAngles.y;
		Vector3 zero = Vector3.zero;
		zero.y = y;
		_rotation = Quaternion.Euler(zero) * Quaternion.Euler(0f, 90f, 0f);
	}

	public override void CloneInternals(IFitter original)
	{
		AlphabetFitter alphabetFitter = original as AlphabetFitter;
		_lockRotation = alphabetFitter._lockRotation;
		_oneRotationPerAxis = alphabetFitter._oneRotationPerAxis;
		if (alphabetFitter != null && !_lockRotation)
		{
			if (_oneRotationPerAxis)
			{
				_rotation = Quaternion.LookRotation(Calcule.AbsVecteur(alphabetFitter._rotation * Vector3.forward), alphabetFitter._rotation * Vector3.up);
			}
			else
			{
				_rotation = alphabetFitter._rotation;
			}
		}
	}

	public override IFitter CopyTo(GameObject go)
	{
		Object.Destroy((MonoBehaviour)go.GetInterface<IFitter>());
		AlphabetFitter alphabetFitter = go.AddComponent<AlphabetFitter>();
		alphabetFitter.CloneInternals(this);
		return alphabetFitter;
	}
}
