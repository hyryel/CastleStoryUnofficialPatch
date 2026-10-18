using UnityEngine;
using UnityEngine.Events;

public class KeyEventTrigger : MonoBehaviour
{
	public enum KeyEvent
	{
		None,
		OnKeyDown,
		OnKey,
		OnKeyUp
	}

	public KeyEvent m_KeyEvent;

	public KeyCode m_KeyCode = KeyCode.F10;

	public UnityEvent m_OnEvent;

	private void Update()
	{
		if (IsKeyEventValid())
		{
			m_OnEvent.Invoke();
		}
	}

	private bool IsKeyEventValid()
	{
		switch (m_KeyEvent)
		{
		case KeyEvent.OnKeyDown:
			if (Input.GetKeyDown(m_KeyCode))
			{
				return true;
			}
			break;
		case KeyEvent.OnKey:
			if (Input.GetKey(m_KeyCode))
			{
				return true;
			}
			break;
		case KeyEvent.OnKeyUp:
			if (Input.GetKeyUp(m_KeyCode))
			{
				return true;
			}
			break;
		}
		return false;
	}
}
