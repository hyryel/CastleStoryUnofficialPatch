using System;
using System.Collections.Generic;
using System.Linq;
using Brix.Core;
using Brix.Utils.Logging;
using UnityEngine;

namespace Brix.Engine.Geometrie;

[Serializable]
public class Config
{
	private static BrixLogger logger = new BrixLogger();

	public const int Dessus = 0;

	public const int Devant = 1;

	public const int Derriere = 2;

	public const int Droite = 3;

	public const int Gauche = 4;

	public const int Dessous = 5;

	public static readonly int[] Cotes = new int[4] { 1, 2, 3, 4 };

	public static readonly int[] Faces = new int[6] { 0, 1, 2, 3, 4, 5 };

	public static readonly int[] FacesInverses = new int[6] { 5, 2, 1, 4, 3, 0 };

	public int[] sommetsUtiles;

	public int[] normalesUtiles;

	public int[] uvUtiles;

	public int[] iSommets;

	public int[] iNormales;

	public int[] iUv;

	public int[] triangles;

	private static int[] SommetIndices = CubeModele.SommetIndices;

	public static int FromFaceIndices(IEnumerable<int> faceIndices)
	{
		return faceIndices.Aggregate(0, (int c, int k) => c | (1 << k));
	}

	public static int FromFaceIndex(int faceIndex)
	{
		return 1 << faceIndex;
	}

	public Config Clone()
	{
		return new Config
		{
			sommetsUtiles = sommetsUtiles,
			normalesUtiles = normalesUtiles,
			uvUtiles = uvUtiles,
			iSommets = iSommets,
			iNormales = iNormales,
			iUv = iUv,
			triangles = triangles
		};
	}

	public static Config FromBitmask(int config, int centres = 0, int normales = 0)
	{
		IntClasse intClasse = new IntClasse();
		IntClasse intClasse2 = new IntClasse();
		IntClasse intClasse3 = new IntClasse();
		for (int i = 0; i < 6; i++)
		{
			if ((config & (1 << i)) == 0)
			{
				continue;
			}
			bool flag = (centres & (1 << i)) != 0;
			bool flag2 = (normales & (1 << i)) != 0;
			for (int j = 0; j < 4; j++)
			{
				intClasse.Ajout(SommetIndices[4 * i + j]);
				intClasse3.Ajout(j);
			}
			if (flag)
			{
				intClasse.Ajout(i + 8);
				intClasse3.Ajout(4);
			}
			if (flag2)
			{
				for (int k = 0; k < 4; k++)
				{
					intClasse2.Ajout(4 * i + k + 6);
				}
			}
			if (flag || !flag2)
			{
				intClasse2.Ajout(i);
			}
		}
		IntClasse intClasse4 = new IntClasse();
		IntClasse intClasse5 = new IntClasse();
		IntClasse intClasse6 = new IntClasse();
		IntClasse intClasse7 = new IntClasse();
		for (int l = 0; l < 6; l++)
		{
			if ((config & (1 << l)) != 0)
			{
				bool flag3 = (centres & (1 << l)) != 0;
				bool flag4 = (normales & (1 << l)) != 0;
				int count = intClasse4.Count;
				int num = intClasse2.Find(l);
				for (int m = 0; m < 4; m++)
				{
					intClasse4.Add(intClasse.Find(SommetIndices[4 * l + m]));
					intClasse5.Add(flag4 ? intClasse2.Find(4 * l + m + 6) : num);
					intClasse6.Add(intClasse3.Find(m));
				}
				if (flag3)
				{
					intClasse4.Add(intClasse.Find(l + 8));
					intClasse5.Add(num);
					intClasse6.Add(intClasse3.Find(4));
					intClasse7.Concat(new int[12]
					{
						count,
						count + 1,
						count + 4,
						count + 1,
						count + 3,
						count + 4,
						count + 3,
						count + 2,
						count + 4,
						count + 2,
						count,
						count + 4
					});
				}
				else
				{
					intClasse7.Concat(new int[6]
					{
						count,
						count + 1,
						count + 3,
						count,
						count + 3,
						count + 2
					});
				}
			}
		}
		return new Config
		{
			sommetsUtiles = intClasse,
			normalesUtiles = intClasse2,
			uvUtiles = intClasse3,
			iSommets = intClasse4,
			iNormales = intClasse5,
			iUv = intClasse6,
			triangles = intClasse7
		};
	}

	public static Config Simplified(Config config, IGeometrie geometrie)
	{
		Index<int> index = new Index<int>(config.sommetsUtiles);
		Index<int> index2 = index.Modulo((int i) => geometrie.GetSommet(i));
		int[] iSommets = index2.TranslateKeysFrom(index, config.iSommets).ToArray();
		int[] array = index2.Values.ToArray();
		logger.Info("Config.Simplified() : Sommets");
		logger.Info("sommetsUtiles: " + index);
		logger.Info("sommetsUtilesCompacts: " + index2);
		logger.Info("iSommets: " + Donnees.PrintAll(iSommets));
		IntClasse normalesUtiles = (from i in config.normalesUtiles
			group i by geometrie.GetNormale(i) into g
			select g.First()).ToArray();
		int[] iNormales = config.iNormales.Select((int i) => normalesUtiles.Find(config.normalesUtiles[i])).ToArray();
		IntClasse uvUtiles = config.uvUtiles.Select((int i) => i).ToArray();
		int[] iUv = config.iUv.Select((int i) => uvUtiles.Find(config.uvUtiles[i])).ToArray();
		Classeur<int[]> classeur = new Classeur<int[]>();
		for (int num = 0; num < config.triangles.Length; num += 3)
		{
			classeur.Add(new int[3]
			{
				config.triangles[num],
				config.triangles[num + 1],
				config.triangles[num + 2]
			});
		}
		foreach (int[] item in classeur)
		{
			while (iSommets[item[0]] > iSommets[item[1]])
			{
				int num2 = item[0];
				item[0] = item[1];
				item[1] = item[2];
				item[2] = num2;
			}
		}
		classeur = classeur.Where((int[] t) => iSommets[t[0]] != iSommets[t[1]] && iSommets[t[1]] != iSommets[t[2]] && iSommets[t[0]] != iSommets[t[2]]).ToArray();
		classeur = (from t in classeur
			group t by t into g
			select g.First()).ToArray();
		int[] array2 = (from t in classeur
			from i in t
			select i).ToArray();
		Index<int> triangleIndex = new Index<int>(array2);
		iSommets = triangleIndex.Values.Select((int i) => iSommets[i]).ToArray();
		iNormales = triangleIndex.Values.Select((int i) => iNormales[i]).ToArray();
		iUv = triangleIndex.Values.Select((int i) => iUv[i]).ToArray();
		array2 = array2.Select((int t) => triangleIndex.GetKey(t)).ToArray();
		logger.Info("Config.Simplified() : Triangles");
		logger.Info("triangleIndex: " + triangleIndex);
		logger.Info("iSommets: " + Donnees.PrintAll(iSommets));
		logger.Info("iNormales: " + Donnees.PrintAll(iNormales));
		logger.Info("iUv: " + Donnees.PrintAll(iUv));
		return new Config
		{
			sommetsUtiles = array,
			normalesUtiles = normalesUtiles,
			uvUtiles = uvUtiles,
			iSommets = iSommets,
			iNormales = iNormales,
			iUv = iUv,
			triangles = array2
		};
	}

	public static int[] GetSimplifiedVertexIndices(int[] triangles, out int[] trianglesOut)
	{
		int[] sommetsUtiles = triangles.Distinct().ToArray();
		Array.Sort(sommetsUtiles, 0, sommetsUtiles.Length);
		trianglesOut = triangles.Select((int t) => Array.BinarySearch(sommetsUtiles, t)).ToArray();
		return sommetsUtiles;
	}
}
