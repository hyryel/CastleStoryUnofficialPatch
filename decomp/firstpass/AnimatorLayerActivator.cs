using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimatorLayerActivator : MonoBehaviour
{
	public enum CastleStoryLayers
	{
		locomotion,
		hurt,
		shield
	}

	private class LayerActivation
	{
		public bool Activated;

		public float Speed;

		public float Limit;
	}

	private Animator Animator;

	private List<LayerActivation> LayerActivations;

	public void Awake()
	{
		Animator = GetComponent<Animator>();
		LayerActivations = new List<LayerActivation>();
		for (int i = 1; i < Animator.layerCount; i++)
		{
			LayerActivations.Add(new LayerActivation
			{
				Activated = (Animator.GetLayerWeight(i) > 0.999f),
				Speed = 2f,
				Limit = 1f
			});
		}
	}

	public void Activate(int layer)
	{
		LayerActivations[layer - 1].Activated = true;
	}

	public void Deactivate(int layer)
	{
		LayerActivations[layer - 1].Activated = false;
	}

	public void SetSpeed(int layer, float speed)
	{
		LayerActivations[layer - 1].Speed = speed;
	}

	public void SetLimit(int layer, float limit)
	{
		LayerActivations[layer - 1].Limit = limit;
	}

	public void ResetSpeed(int layer)
	{
		LayerActivations[layer - 1].Speed = 2f;
	}

	public void ResetLimit(int layer)
	{
		LayerActivations[layer - 1].Limit = 1f;
	}

	public void AnimatorUpdate(float deltaTime)
	{
		for (int i = 0; i < LayerActivations.Count; i++)
		{
			LayerActivation layerActivation = LayerActivations[i];
			int layerIndex = i + 1;
			float layerWeight = Animator.GetLayerWeight(layerIndex);
			if (layerActivation.Activated && layerWeight < layerActivation.Limit)
			{
				Animator.SetLayerWeight(layerIndex, Mathf.Min(layerActivation.Limit, layerWeight + layerActivation.Speed * deltaTime));
			}
			else if (!layerActivation.Activated && layerWeight > 0f)
			{
				Animator.SetLayerWeight(layerIndex, Mathf.Max(0f, layerWeight - layerActivation.Speed * deltaTime));
			}
		}
	}
}
