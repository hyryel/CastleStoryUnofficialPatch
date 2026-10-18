using UnityEngine;
using UnityEngine.UI;

namespace Brix.UI;

public class BrixImage : Image
{
	private static bool _materialOptimization = true;

	public bool MaterialOptimizationOverride;

	public bool IsMaterialOptimizationOverride;

	private bool _materialUpdated;

	private static bool _layoutOptimization = true;

	private static bool _rebuildMaterialOnEnable = true;

	private static bool _rebuildLayoutOnEnable;

	private static bool _rebuildMaterialOnDisable;

	private static bool _rebuildLayoutOnDisable;

	private bool MaterialOptimmization => !_materialOptimization && IsMaterialOptimizationOverride && !MaterialOptimizationOverride;

	[ContextMenu("Material Optimization: On")]
	public void MaterialOptimization_On()
	{
		_materialOptimization = true;
	}

	[ContextMenu("Material Optimization: Off")]
	public void MaterialOptimization_Off()
	{
		_materialOptimization = false;
	}

	public void MarkMaterialForRebuild()
	{
		if (!MaterialOptimmization)
		{
			_materialUpdated = false;
			base.SetMaterialDirty();
		}
	}

	protected override void UpdateMaterial()
	{
		if (MaterialOptimmization)
		{
			if (_materialUpdated)
			{
				return;
			}
			_materialUpdated = true;
		}
		base.UpdateMaterial();
	}

	[ContextMenu("Layout Optimization: On")]
	public void LayoutOptimization_On()
	{
		_layoutOptimization = true;
	}

	[ContextMenu("Layout Optimization: Off")]
	public void LayoutOptimization_Off()
	{
		_layoutOptimization = false;
	}

	public void MarkLayoutForRebuild()
	{
		if (_layoutOptimization)
		{
			base.SetLayoutDirty();
		}
	}

	public override void SetLayoutDirty()
	{
		if (!_layoutOptimization)
		{
			base.SetLayoutDirty();
		}
	}

	[ContextMenu("Material Rebuild OnEnable: ON")]
	public void MaterialRebuildOnEnable_On()
	{
		_rebuildMaterialOnEnable = true;
	}

	[ContextMenu("Material Rebuild OnEnable: OFF")]
	public void MaterialRebuildOnEnable_Off()
	{
		_rebuildMaterialOnEnable = false;
	}

	[ContextMenu("Layout Rebuild OnEnable: ON")]
	public void LayoutRebuildOnEnable_On()
	{
		_rebuildLayoutOnEnable = true;
	}

	[ContextMenu("Layout Rebuild OnEnable: OFF")]
	public void LayoutRebuildOnEnable_Off()
	{
		_rebuildLayoutOnEnable = false;
	}

	protected override void OnEnable()
	{
		if (_rebuildMaterialOnEnable)
		{
			MarkMaterialForRebuild();
		}
		if (_rebuildLayoutOnEnable)
		{
			MarkLayoutForRebuild();
		}
		base.OnEnable();
	}

	[ContextMenu("Material Rebuild OnDisable: ON")]
	public void MaterialRebuildOnDisable_On()
	{
		_rebuildMaterialOnDisable = true;
	}

	[ContextMenu("Material Rebuild OnDisable: OFF")]
	public void MaterialRebuildOnDisable_Off()
	{
		_rebuildMaterialOnDisable = false;
	}

	[ContextMenu("Layout Rebuild OnDisable: ON")]
	public void LayoutRebuildOnDisable_On()
	{
		_rebuildLayoutOnDisable = true;
	}

	[ContextMenu("Layout Rebuild OnDisable: OFF")]
	public void LayoutRebuildOnDisable_Off()
	{
		_rebuildLayoutOnDisable = false;
	}

	protected override void OnDisable()
	{
		if (_rebuildMaterialOnDisable)
		{
			MarkMaterialForRebuild();
		}
		if (_rebuildLayoutOnDisable)
		{
			MarkLayoutForRebuild();
		}
		base.OnDisable();
	}
}
