using Brix.Utils.Logging;
using UnityEngine;

namespace Brix.Engine.Loop;

public class TerrainLoop : MonoBehaviour
{
	private static BrixLogger logger = new BrixLogger(typeof(TerrainLoop).FullName);

	private int boucleTerrainCount;

	private void Start()
	{
		GameSignals.DisableUntil(GameSignals.LevelLoaded, this);
	}

	private void Update()
	{
		BoucleTerrain();
	}

	private void BoucleTerrain()
	{
		if (boucleTerrainCount <= 5)
		{
			logger.InfoFormat("Architecte.BoucleTerrain() - loop #{0}", boucleTerrainCount);
			boucleTerrainCount++;
		}
		VoxelEngine.ApplyChanges();
		Architecte.AppliquerChangements();
		Architecte.instance.UpdateJardinier();
	}
}
