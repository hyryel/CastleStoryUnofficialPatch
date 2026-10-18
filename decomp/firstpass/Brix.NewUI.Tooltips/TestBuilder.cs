using UnityEngine;

namespace Brix.NewUI.Tooltips;

public class TestBuilder : MonoBehaviour
{
	public TooltipBuilder.TooltipTag tooltipTag;

	public TooltipBuilder builder;

	public RectTransform target2D;

	public GameObject target3D;

	public Vector3 offset;

	public Sprite img;

	public TooltipPositioner.Direction direction;

	public bool testTitleDynamic;

	public bool testTitleNotDynamic;

	public bool test3DFollow;

	private void Start()
	{
	}

	private void TestTitleDynamic()
	{
		builder.NewTooltip(tooltipTag).WithTitle("Allo I will follow").WithMainImage(img)
			.Pointing2D(target2D)
			.From(direction)
			.Dynamically()
			.Make();
	}

	private void TestTitleNotDynamic()
	{
		builder.NewTooltip(tooltipTag).WithTitle("Allo I dont follow").WithMainImage(img)
			.Pointing2D(target2D)
			.From(direction)
			.Make();
	}

	private void TestDynamic3D()
	{
		builder.NewTooltip(tooltipTag).OfSize(Tooltip.Width.narrow).WithTitle("I follow!")
			.Pointing3D(target3D)
			.From(direction)
			.WithOffset2D(offset)
			.Make();
	}

	private void Update()
	{
		if (testTitleDynamic)
		{
			TestTitleDynamic();
			testTitleDynamic = false;
		}
		if (testTitleNotDynamic)
		{
			TestTitleNotDynamic();
			testTitleNotDynamic = false;
		}
		if (test3DFollow)
		{
			TestDynamic3D();
			test3DFollow = false;
		}
	}
}
