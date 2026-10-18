using System.Collections.Generic;
using Brix.Util;
using Brix.Utils;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Game.Components.Volumes;

public static class VPropertyExtender
{
	public static Dictionary<VProperty, Color> VPropertyColors = new Dictionary<VProperty, Color>(FastEnumIntEqualityComparer<VProperty>.Instance)
	{
		{
			VProperty.obstacle,
			new Color(0f, 1f, 0f)
		},
		{
			VProperty.access,
			Color.yellow
		},
		{
			VProperty.breakableAccess,
			new Color(0.9f, 0.83f, 0.014f, 1f)
		},
		{
			VProperty.occupy,
			new Color(1f, 0f, 0f)
		},
		{
			VProperty.walkable,
			new Color(1f, 0.5f, 0f)
		},
		{
			VProperty.operation,
			new Color(0.5f, 0.5f, 0.5f)
		},
		{
			VProperty.fantome,
			new Color(0f, 0.5f, 1f)
		},
		{
			VProperty.stoneSupport,
			new Color(0.1f, 0.9f, 0.1f)
		},
		{
			VProperty.logSupport,
			new Color(0.2f, 0.8f, 0.2f)
		},
		{
			VProperty.braceSupport,
			new Color(0.3f, 0.7f, 0.3f)
		},
		{
			VProperty.beamSupport,
			new Color(0.4f, 0.6f, 0.4f)
		},
		{
			VProperty.plankSupport,
			new Color(0.5f, 0.5f, 0.5f)
		},
		{
			VProperty.mainSupportReq,
			new Color(0.9f, 0.1f, 0.9f)
		},
		{
			VProperty.auxiliarySupportReq,
			new Color(0.8f, 0.2f, 0.8f)
		},
		{
			VProperty.debloc,
			new Color(1f, 0.4f, 0f)
		},
		{
			VProperty.path,
			new Color(0.5f, 0.5f, 0.5f)
		},
		{
			VProperty.fallback,
			new Color(0.2f, 0.8f, 0.2f)
		},
		{
			VProperty.meshSwapTrigger,
			new Color(0.7f, 0.9f, 0.7f)
		},
		{
			VProperty.preventMergeback,
			new Color(0.5f, 1f, 0.5f)
		},
		{
			VProperty.invincible,
			new Color(0.3f, 0.3f, 0.3f)
		},
		{
			VProperty.notWallSupport,
			new Color(0.1f, 0.2f, 0.8f)
		},
		{
			VProperty.natureBlocker,
			new Color(0f, 0f, 0.75f)
		},
		{
			VProperty._unused_25,
			new Color(0.75f, 0f, 0f)
		},
		{
			VProperty.character,
			new Color(0.75f, 0.75f, 0f)
		},
		{
			VProperty.deblocTerrain,
			new Color(0f, 0.4f, 0.4f)
		},
		{
			VProperty.canHookBeam,
			new Color(0.8f, 0.4f, 0.2f)
		},
		{
			VProperty.ropeSupport,
			new Color(0.2f, 0.4f, 0.1f)
		},
		{
			VProperty.stickySupport,
			new Color(0.2f, 0.1f, 0.1f)
		},
		{
			VProperty.characterAttackAccess,
			new Color(0.2f, 0.1f, 0.1f)
		}
	};

	public static LongBitsEnumerable Bits(this VPropertyMask mask)
	{
		return new LongBitsEnumerable((long)mask);
	}

	public static VPropertyMask Remove(this VPropertyMask m, int p)
	{
		return m.Remove((VProperty)p);
	}

	public static VPropertyMask Remove(this VPropertyMask m, VProperty p)
	{
		return m & ~p.ToMask();
	}

	public static VPropertyMask ToMask(this VProperty p)
	{
		return (VPropertyMask)(1L << (int)p);
	}

	public static Slinq<int, TemplatedEnumeratorContext<int, LongBitsEnumerator>> Slinq(this LongBitsEnumerable bits)
	{
		return TemplatedEnumeratorContext<int, LongBitsEnumerator>.Slinq(bits.GetEnumerator());
	}
}
