using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Brix.UI.Builder.Helpers;

public class DropdownHelper : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler, IEventSystemHandler
{
	public List<Button> Buttons;

	public List<Dropdown> Dropdowns;

	public void OnPointerClick(PointerEventData data)
	{
		foreach (Button button in Buttons)
		{
			if (!(button == null))
			{
				button.OnPointerClick(data);
			}
		}
		foreach (Dropdown dropdown in Dropdowns)
		{
			if (!(dropdown == null))
			{
				dropdown.OnPointerClick(data);
			}
		}
	}

	public void OnPointerDown(PointerEventData data)
	{
		foreach (Button button in Buttons)
		{
			if (!(button == null))
			{
				button.OnPointerDown(data);
			}
		}
		foreach (Dropdown dropdown in Dropdowns)
		{
			if (!(dropdown == null))
			{
				dropdown.OnPointerDown(data);
			}
		}
	}

	public void OnPointerUp(PointerEventData data)
	{
		foreach (Button button in Buttons)
		{
			if (!(button == null))
			{
				button.OnPointerUp(data);
			}
		}
		foreach (Dropdown dropdown in Dropdowns)
		{
			if (!(dropdown == null))
			{
				dropdown.OnPointerUp(data);
			}
		}
	}

	public void OnPointerEnter(PointerEventData data)
	{
		foreach (Button button in Buttons)
		{
			if (!(button == null))
			{
				button.OnPointerEnter(data);
			}
		}
		foreach (Dropdown dropdown in Dropdowns)
		{
			if (!(dropdown == null))
			{
				dropdown.OnPointerEnter(data);
			}
		}
	}

	public void OnPointerExit(PointerEventData data)
	{
		foreach (Button button in Buttons)
		{
			if (!(button == null))
			{
				button.OnPointerExit(data);
			}
		}
		foreach (Dropdown dropdown in Dropdowns)
		{
			if (!(dropdown == null))
			{
				dropdown.OnPointerExit(data);
			}
		}
	}
}
