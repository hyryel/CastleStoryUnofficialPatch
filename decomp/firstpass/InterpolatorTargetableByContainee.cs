using UnityEngine;

[RequireComponent(typeof(InterpolatorTargetable))]
[ExecuteInEditMode]
public class InterpolatorTargetableByContainee : MonoBehaviour
{
	[SerializeField]
	private InterpolatorTargetable m_InterpolatorTargetable;

	[SerializeField]
	private float m_ClosedTargetTraverse;

	[SerializeField]
	private float m_OpenedTargetTraverse = 1f;

	public InterpolatorTargetable interpolatorTargetable
	{
		get
		{
			return m_InterpolatorTargetable;
		}
		set
		{
			m_InterpolatorTargetable = value;
		}
	}

	public float closedTargetTraverse
	{
		get
		{
			return m_ClosedTargetTraverse;
		}
		set
		{
			m_ClosedTargetTraverse = value;
		}
	}

	public float openedTargetTraverse
	{
		get
		{
			return m_OpenedTargetTraverse;
		}
		set
		{
			m_OpenedTargetTraverse = value;
		}
	}

	private void OnEnable()
	{
		interpolatorTargetable = ((!(interpolatorTargetable != null)) ? base.gameObject.GetComponent<InterpolatorTargetable>() : interpolatorTargetable);
		Register();
	}

	private void OnDisable()
	{
		Unregister();
	}

	public void TryRegister()
	{
	}

	public void Register()
	{
		Unregister();
	}

	public void Unregister()
	{
	}

	private void OnTraverseValueChange()
	{
	}

	public void SetTraverse()
	{
	}

	public void SetTargetTraverse()
	{
	}
}
