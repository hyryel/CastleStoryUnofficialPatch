using System;
using System.IO;
using Brix.Utils.Logging;
using UnityEngine;

namespace Brix.Engine;

public class RopeBridgePlankBlockInfoMeta : BlockInfoMeta
{
	private class RopeBridgePlankBlockInfo : BlockInfo
	{
		public override int CalculateNiveaux(IBlockData bd)
		{
			RopeBridgeConfigurator ropeBridgeFromPlankAt = RopeBridgeConfigurator.GetRopeBridgeFromPlankAt(bd.Position);
			if (ropeBridgeFromPlankAt == null)
			{
				logger.WarnFormat("RopeBridgePlankBlockInfo.CalculateNiveaux() : missing bridge at {0}", bd.Position);
				return 8738;
			}
			ropeBridgeFromPlankAt.GetPlankTransform(bd.Position, out var position, out var rotation);
			Plane plane = new Plane(rotation * Vector3.up, position);
			Vector3 bottomCenter = bd.Position.ToVector3() - Vector3.up * 0.5f;
			int niveau = GetNiveau(plane, bottomCenter, -0.5f, 0.5f);
			int niveau2 = GetNiveau(plane, bottomCenter, 0.5f, 0.5f);
			int niveau3 = GetNiveau(plane, bottomCenter, -0.5f, -0.5f);
			int niveau4 = GetNiveau(plane, bottomCenter, 0.5f, -0.5f);
			return (niveau << 0) | (niveau2 << 4) | (niveau4 << 8) | (niveau3 << 12);
		}

		private int GetNiveau(Plane plane, Vector3 bottomCenter, float dx, float dz)
		{
			plane.Raycast(new Ray(bottomCenter + new Vector3(dx, 0f, dz), Vector3.up), out var enter);
			return GetNiveau(enter);
		}

		private int GetNiveau(float h)
		{
			return Mathf.RoundToInt(Mathf.Clamp01(h) * 3f);
		}
	}

	private static BrixLogger logger = new BrixLogger(typeof(RopeBridgePlankBlockInfoMeta).FullName);

	protected override BlockInfo InstantiateConcreteBlocInfoInstance()
	{
		return new RopeBridgePlankBlockInfo();
	}

	protected override void Validate()
	{
		if (Type != BlockType.Block)
		{
			throw new InvalidDataException($"Configuration error! Block '{base.gameObject.name}' was set as BlockType.{Enum.GetName(typeof(BlockType), Type)} but it is configured using a StairBlockInfoMeta in the Factory.");
		}
	}
}
