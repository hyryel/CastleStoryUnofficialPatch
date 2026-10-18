using Brix.Engine;
using Brix.External.Factories;
using Brix.Lifecycle.Pooling;
using UnityEngine;
using UnityEngine.Events;

public class FactoryTemplateReference : MonoBehaviour
{
	[HideInInspector]
	public UnityEvent _onAfterEnable;

	[HideInInspector]
	public UnityEvent _onBeforeDisable;

	public Factory.AssetKey _source;

	private GameObject _destination;

	private void OnAfterEnable()
	{
		_onAfterEnable.Invoke();
	}

	private void OnBeforeDisable()
	{
		_onBeforeDisable.Invoke();
	}

	[ContextMenu("Enable")]
	public void Enable()
	{
		ObjectPoolSingleton.Request(out _destination, _source);
		_destination.transform.SetParent(base.transform.parent, worldPositionStays: false);
		_destination.transform.SetSiblingIndex(base.transform.GetSiblingIndex());
		base.gameObject.SetActive(value: false);
		OnAfterEnable();
	}

	[ContextMenu("Disable")]
	public void Disable()
	{
		OnBeforeDisable();
		ObjectPoolSingleton.Release(_destination);
		base.gameObject.SetActive(value: true);
	}

	private void AutoAdjustSizes()
	{
		if (_destination == null)
		{
			return;
		}
		foreach (ContentSizeFitterPlus item in _destination.GetComponentsInChildrenBreadthFirst<ContentSizeFitterPlus>())
		{
			item.Constrain();
		}
	}

	[ContextMenu("OkDialog:Open")]
	public void OkDialogOpen()
	{
		GameSignals.Invoke(GameSignals.OnDisplayOkDialog, "That Prince", "The prince then inquired if she knew what had become of the enchanted horse since the Indian's death, but the princess could only reply that she had heard nothing about it. Still she did not suppose that the horse could have been forgotten by the Sultan, after all she had told him of its value.", "OK", delegate
		{
			GameSignals.Invoke(GameSignals.OnCloseDialog);
		});
	}

	[ContextMenu("OkDialog:Close")]
	public void OkDialogClose()
	{
		GameSignals.Invoke(GameSignals.OnCloseDialog);
	}
}
