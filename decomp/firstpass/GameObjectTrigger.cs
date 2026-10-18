using UnityEngine;
using UnityEngine.Events;

public class GameObjectTrigger : MonoBehaviour
{
	public UnityEvent m_OnEnable;

	public UnityEvent m_OnDisable;

	private void OnEnable()
	{
		m_OnEnable.Invoke();
	}

	private void OnDisable()
	{
		m_OnDisable.Invoke();
	}
}
