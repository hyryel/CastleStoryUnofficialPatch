using System;
using UnityEngine;

namespace Brix.Engine.FlatWorld;

public static class Shaders
{
	public static void Add(Tex resultTex, Texture previousTex, Texture secondTex, Vector4 mul1, Vector4 mul2, Vector4 add)
	{
		Material worldMat = GetWorldMat(Mat.Add);
		worldMat.SetTexture("_SecondTex", secondTex);
		worldMat.SetVector("_Mul1", mul1);
		worldMat.SetVector("_Mul2", mul2);
		worldMat.SetVector("_Add", add);
		Render(resultTex, previousTex, worldMat);
	}

	public static void AddIfBetween(Tex resultTex, Texture previousTex, Texture dataTex, Color32 lowColor, Color32 highColor, Vector4 addIf, Vector4 addElse = default(Vector4))
	{
		Material worldMat = GetWorldMat(Mat.AddIfBetween);
		worldMat.SetTexture("_DataTex", dataTex);
		worldMat.SetColor("_Low", lowColor);
		worldMat.SetColor("_High", highColor);
		worldMat.SetVector("_AddIf", addIf);
		worldMat.SetVector("_AddElse", addElse);
		Render(resultTex, previousTex, worldMat);
	}

	public static void AddNonZero(Tex resultTex, Texture previousTex, Texture dataTex, Vector4 add, Vector4 zeroMask = default(Vector4))
	{
		Material worldMat = GetWorldMat(Mat.AddNonZero);
		worldMat.SetTexture("_DataTex", dataTex);
		worldMat.SetVector("_Add", add);
		worldMat.SetVector("_ZeroMask", zeroMask);
		Render(resultTex, previousTex, worldMat);
	}

	public static void BitwiseAnd(Tex resultTex, Texture previousTex, Texture secondTex, bool flipSecond = false)
	{
		Material worldMat = GetWorldMat(Mat.BitwiseAnd);
		worldMat.SetTexture("_SecondTex", secondTex);
		worldMat.SetFloat("_FlipSecond", flipSecond ? 1f : 0f);
		Render(resultTex, previousTex, worldMat);
	}

	public static void Borders(Tex resultTex, Texture previousTex, Vector2 min, Vector2 max, Color32 outColor = default(Color32))
	{
		Material worldMat = GetWorldMat(Mat.Borders);
		worldMat.SetVector("_InputSize", new Vector4(previousTex.width, previousTex.height));
		worldMat.SetVector("_Min", min);
		worldMat.SetVector("_Max", max);
		worldMat.SetColor("_OutColor", outColor);
		Render(resultTex, previousTex, worldMat);
	}

	public static void Branching(Tex resultTex, Texture dataTex)
	{
		Material worldMat = GetWorldMat(Mat.Branching);
		Render(resultTex, dataTex, worldMat);
	}

	public static void BranchingConfig(Tex resultTex, Texture branchingTex, Texture dataLowMidHighTex, Texture levelTex)
	{
		Material worldMat = GetWorldMat(Mat.BranchingConfig);
		worldMat.SetTexture("_DataLowMidHighTex", dataLowMidHighTex);
		worldMat.SetTexture("_LevelTex", levelTex);
		Render(resultTex, branchingTex, worldMat);
	}

	public static void BranchingLevel(Tex resultTex, Texture offsetTex, Texture xzTex, Texture columnTex, Texture volumeTex, Texture levelTex, Texture topBottomTex, Vector3 translation, Vector4 levelMask)
	{
		Material worldMat = GetWorldMat(Mat.BranchingLevel);
		worldMat.SetTexture("_XZTex", xzTex);
		worldMat.SetTexture("_ColumnTex", columnTex);
		worldMat.SetTexture("_VolumeTex", volumeTex);
		worldMat.SetTexture("_LevelTex", levelTex);
		worldMat.SetTexture("_TopBottomTex", topBottomTex);
		worldMat.SetVector("_InputSize", new Vector4(volumeTex.width, volumeTex.height));
		worldMat.SetVector("_ColumnSize", new Vector4(columnTex.width, columnTex.height));
		worldMat.SetVector("_Translation", translation);
		worldMat.SetVector("_LevelMask", levelMask);
		Render(resultTex, offsetTex, worldMat);
	}

	public static void BranchingTranslate(Tex resultTex, Texture offsetTex, Texture xzTex, Texture columnTex, Texture dataTex, Texture branchingTex, Color32 emptyColor, Color32 internalColor, Vector4 dataMask, bool partial, Vector4 transU, Vector4 transV)
	{
		Material worldMat = GetWorldMat(Mat.BranchingTranslate);
		worldMat.SetTexture("_XZTex", xzTex);
		worldMat.SetTexture("_ColumnTex", columnTex);
		worldMat.SetTexture("_DataTex", dataTex);
		worldMat.SetTexture("_BranchingTex", branchingTex);
		worldMat.SetVector("_InputSize", new Vector4(dataTex.width, dataTex.height));
		worldMat.SetVector("_ColumnSize", new Vector4(columnTex.width, columnTex.height));
		worldMat.SetColor("_EmptyColor", emptyColor);
		worldMat.SetColor("_InternalColor", internalColor);
		worldMat.SetVector("_DataMask", dataMask);
		worldMat.SetFloat("_Partial", partial ? 1 : 0);
		worldMat.SetVector("_TransU", transU);
		worldMat.SetVector("_TransV", transV);
		Render(resultTex, offsetTex, worldMat);
	}

	public static void Clear(Tex resultTex, Texture emptyTex, Color32 color = default(Color32), int width = 0, int height = 0)
	{
		Material worldMat = GetWorldMat(Mat.Clear);
		worldMat.color = color;
		Render(resultTex, emptyTex, worldMat, width, height);
	}

	public static void ColumnCountInit(Tex resultTex, Texture columnTex, Vector3 sampleStart, Vector3 sampleSize)
	{
		int width = Mathf.RoundToInt(sampleSize.x);
		int height = Mathf.RoundToInt(sampleSize.z);
		Material worldMat = GetWorldMat(Mat.ColumnCountInit);
		worldMat.SetVector("_InputSize", new Vector4(columnTex.width, columnTex.height));
		worldMat.SetVector("_SampleStart", sampleStart);
		worldMat.SetVector("_SampleTotalSize", sampleSize);
		Render(resultTex, columnTex, worldMat, width, height);
	}

	public static void Columns(Tex columnTex, Texture fillingTex)
	{
		Material worldMat = GetWorldMat(Mat.Columns);
		worldMat.SetVector("_InputSize", new Vector4(fillingTex.width, fillingTex.height));
		Render(columnTex, fillingTex, worldMat);
	}

	public static void Copy(Tex resultTex, Texture dataTex)
	{
		Material worldMat = GetWorldMat(Mat.Copy);
		Render(resultTex, dataTex, worldMat);
	}

	public static void CopyIfBetween(Tex resultTex, Texture previousTex, Texture compareTex, Texture nextTex, Color32 lowColor, Color32 highColor, Vector4 addIf = default(Vector4), Vector4 addElse = default(Vector4))
	{
		Material worldMat = GetWorldMat(Mat.CopyIfBetween);
		worldMat.SetTexture("_DataTex", compareTex);
		worldMat.SetTexture("_DataTex2", nextTex);
		worldMat.SetColor("_Low", lowColor);
		worldMat.SetColor("_High", highColor);
		worldMat.SetVector("_AddIf", addIf);
		worldMat.SetVector("_AddElse", addElse);
		Render(resultTex, previousTex, worldMat);
	}

	public static void CrustLevel(Tex resultTex, Texture cornerVolumeTex, Texture volumeTex, Texture smoothTex)
	{
		Material worldMat = GetWorldMat(Mat.CrustLevel);
		worldMat.SetTexture("_VolumeTex", volumeTex);
		worldMat.SetTexture("_SmoothTex", smoothTex);
		Render(resultTex, cornerVolumeTex, worldMat);
	}

	public static void CrustNeighborHeight(Tex resultTex, Texture dataMidTex, Texture dataLowTex, Texture dataHighTex, Texture levelTex)
	{
		Material worldMat = GetWorldMat(Mat.CrustNeighborHeight);
		worldMat.SetTexture("_DataLow", dataLowTex);
		worldMat.SetTexture("_DataHigh", dataHighTex);
		worldMat.SetTexture("_LevelTex", levelTex);
		Render(resultTex, dataMidTex, worldMat);
	}

	public static void CrustNeighbors(Tex resultTex, Texture dataMidTex, Texture dataLowTex, Texture dataHighTex, Texture levelTex)
	{
		Material worldMat = GetWorldMat(Mat.CrustNeighbors);
		worldMat.SetTexture("_DataLow", dataLowTex);
		worldMat.SetTexture("_DataHigh", dataHighTex);
		worldMat.SetTexture("_LevelTex", levelTex);
		Render(resultTex, dataMidTex, worldMat);
	}

	public static void DepthFill(Tex resultTex, Texture depthLowTex, Texture depthMidTex, Texture depthHighTex, Color32 emptyColor, Color32 internalColor)
	{
		Material worldMat = GetWorldMat(Mat.DepthFill);
		worldMat.SetTexture("_DepthLow", depthLowTex);
		worldMat.SetTexture("_DepthHigh", depthHighTex);
		worldMat.SetColor("_EmptyColor", emptyColor);
		worldMat.SetColor("_InternalColor", internalColor);
		Render(resultTex, depthMidTex, worldMat);
	}

	public static void Depths(Tex resultTex, Texture offsetTex, Texture worldTex, Vector3 translation = default(Vector3))
	{
		Material worldMat = GetWorldMat(Mat.Depths);
		worldMat.SetTexture("_WorldTex", worldTex);
		worldMat.SetVector("_ColumnSize", new Vector4(worldTex.width, worldTex.height));
		worldMat.SetVector("_Translation", translation);
		Render(resultTex, offsetTex, worldMat);
	}

	public static void Filling(Tex resultTex, Texture worldTex)
	{
		Material worldMat = GetWorldMat(Mat.Filling);
		Render(resultTex, worldTex, worldMat);
	}

	public static void FillingAdd(Tex resultTex, Texture fillingTex, Texture fillingTex2, Vector3 translationSecondTex)
	{
		Material worldMat = GetWorldMat(Mat.FillingAdd);
		worldMat.SetTexture("_SecondTex", fillingTex2);
		worldMat.SetVector("_InputSize", new Vector4(fillingTex.width, fillingTex.height));
		worldMat.SetVector("_SecondSize", new Vector4(fillingTex2.width, fillingTex2.height));
		worldMat.SetVector("_Translation", translationSecondTex);
		Render(resultTex, fillingTex, worldMat);
	}

	public static void FractalNoise(Tex resultTex, Texture offsetTex, Texture permTex, Vector3 translation, Vector3 scale, float scaleFactor = 2f, float amplitudeFalloff = 0.5f, int octaves = 4)
	{
		Material worldMat = GetWorldMat(Mat.FractalNoise);
		worldMat.SetTexture("_PermTex", permTex);
		worldMat.SetVector("_Translation", translation);
		worldMat.SetVector("_Scale", scale);
		worldMat.SetFloat("_ScaleFactor", scaleFactor);
		worldMat.SetFloat("_AmplitudeFalloff", amplitudeFalloff);
		worldMat.SetFloat("_Octaves", octaves);
		Render(resultTex, offsetTex, worldMat);
	}

	public static void MergeAdd(Tex resultTex, Texture dataTex, float mulFactor = 0.25f)
	{
		Material worldMat = GetWorldMat(Mat.MergeAdd);
		worldMat.SetVector("_InputSize", new Vector4(dataTex.width, dataTex.height));
		worldMat.SetFloat("_MulFactor", mulFactor);
		Render(resultTex, dataTex, worldMat, dataTex.width / 2, dataTex.height / 2);
	}

	public static void MergeColumns(Tex resultTex, Texture fillingTex)
	{
		Material worldMat = GetWorldMat(Mat.MergeColumns);
		worldMat.SetVector("_InputSize", new Vector4(fillingTex.width, fillingTex.height));
		Render(resultTex, fillingTex, worldMat, fillingTex.width / 2, fillingTex.height / 2);
	}

	public static void MergeTypes(Tex resultTex, Texture offsetTex, Texture types1, Texture types2, float internalType = 255f)
	{
		Material worldMat = GetWorldMat(Mat.MergeTypes);
		worldMat.SetTexture("_Types1", types1);
		worldMat.SetTexture("_Types2", types2);
		worldMat.SetFloat("_InternalType", internalType);
		Render(resultTex, offsetTex, worldMat);
	}

	public static void NormalAverage(Tex resultTex, Texture levelTex, Texture neighbors1, Texture neighbors2, float xzRange = 1f)
	{
		Material worldMat = GetWorldMat(Mat.NormalAverage);
		worldMat.SetTexture("_Neighbors1", neighbors1);
		worldMat.SetTexture("_Neighbors2", neighbors2);
		worldMat.SetFloat("_XZRange", xzRange);
		Render(resultTex, levelTex, worldMat);
	}

	public static void NormalEncode(Tex resultTex, Texture normalTex, float xzRange = 1f)
	{
		Material worldMat = GetWorldMat(Mat.NormalEncode);
		worldMat.SetFloat("_XZRange", xzRange);
		Render(resultTex, normalTex, worldMat);
	}

	public static void Normals(Tex resultTex, Texture levelTex, Texture configTex)
	{
		Material worldMat = GetWorldMat(Mat.Normals);
		worldMat.SetTexture("_ConfigTex", configTex);
		Render(resultTex, levelTex, worldMat);
	}

	public static void OffsetSearch(Tex resultTex, Texture previousTex, Texture xzTex, float step = 1f)
	{
		Material worldMat = GetWorldMat(Mat.OffsetSearch);
		worldMat.SetTexture("_XZTex", xzTex);
		worldMat.SetVector("_InputSize", new Vector4(previousTex.width, previousTex.height));
		worldMat.SetVector("_ColumnSize", new Vector4(xzTex.width, xzTex.height));
		worldMat.SetFloat("_Step", step);
		Render(resultTex, previousTex, worldMat);
	}

	public static void OffsetXYZ(Tex resultTex, Texture previousTex, Texture xzTex, Texture columnTex)
	{
		Material worldMat = GetWorldMat(Mat.OffsetXYZ);
		worldMat.SetTexture("_XZTex", xzTex);
		worldMat.SetTexture("_ColumnTex", columnTex);
		worldMat.SetVector("_InputSize", new Vector4(previousTex.width, previousTex.height));
		worldMat.SetVector("_ColumnSize", new Vector4(xzTex.width, xzTex.height));
		Render(resultTex, previousTex, worldMat);
	}

	public static void OverwriteData(Tex resultTex, Texture offsetTex, Texture dataTex, Texture xzTex2, Texture columnTex2, Texture dataTex2, Vector3 translation2, Color32 internalColor)
	{
		Material worldMat = GetWorldMat(Mat.OverwriteData);
		worldMat.SetTexture("_DataTex", dataTex);
		worldMat.SetTexture("_XZTex2", xzTex2);
		worldMat.SetTexture("_ColumnTex2", columnTex2);
		worldMat.SetTexture("_DataTex2", dataTex2);
		worldMat.SetVector("_ColumnSize2", new Vector4(columnTex2.width, columnTex2.height));
		worldMat.SetVector("_DataSize2", new Vector4(dataTex2.width, dataTex2.height));
		worldMat.SetVector("_Translation2", translation2);
		worldMat.SetColor("_InternalColor", internalColor);
		Render(resultTex, offsetTex, worldMat);
	}

	public static void OverwriteData2(Tex resultTex, Texture offsetTex, Texture xzTex, Texture columnTex, Texture dataTex, Texture xzTex2, Texture columnTex2, Texture dataTex2, Vector3 translation, Vector3 translation2, Color32 emptyColor, Color32 internalColor)
	{
		Material worldMat = GetWorldMat(Mat.OverwriteData2);
		worldMat.SetTexture("_XZTex", xzTex);
		worldMat.SetTexture("_ColumnTex", columnTex);
		worldMat.SetTexture("_DataTex", dataTex);
		worldMat.SetTexture("_XZTex2", xzTex2);
		worldMat.SetTexture("_ColumnTex2", columnTex2);
		worldMat.SetTexture("_DataTex2", dataTex2);
		worldMat.SetVector("_InputSize", new Vector4(offsetTex.width, offsetTex.height));
		worldMat.SetVector("_ColumnSize", new Vector4(columnTex.width, columnTex.height));
		worldMat.SetVector("_DataSize", new Vector4(dataTex.width, dataTex.height));
		worldMat.SetVector("_ColumnSize2", new Vector4(columnTex2.width, columnTex2.height));
		worldMat.SetVector("_DataSize2", new Vector4(dataTex2.width, dataTex2.height));
		worldMat.SetVector("_Translation", translation);
		worldMat.SetVector("_Translation2", translation2);
		worldMat.SetColor("_EmptyColor", emptyColor);
		worldMat.SetColor("_InternalColor", internalColor);
		Render(resultTex, offsetTex, worldMat);
	}

	public static void Remap(Tex resultTex, Texture previousTex, Texture dataTex, Vector4 rMask, Vector4 gMask, Vector4 bMask, Vector4 aMask)
	{
		Material worldMat = GetWorldMat(Mat.Remap);
		worldMat.SetTexture("_SecondTex", previousTex);
		worldMat.SetVector("_RMask", rMask);
		worldMat.SetVector("_GMask", gMask);
		worldMat.SetVector("_BMask", bMask);
		worldMat.SetVector("_AMask", aMask);
		worldMat.SetVector("_Add", Vector4.zero);
		Render(resultTex, dataTex, worldMat);
	}

	public static void Retarget(Tex resultTex, Texture dataTex, Vector2 outputStart, Vector2 outputSize)
	{
		Material worldMat = GetWorldMat(Mat.Retarget);
		worldMat.SetVector("_InputSize", new Vector4(dataTex.width, dataTex.height));
		worldMat.SetVector("_OutputStart", outputStart);
		worldMat.SetVector("_OutputSize", outputSize);
		Render(resultTex, dataTex, worldMat, (int)outputSize.x, (int)outputSize.y);
	}

	public static void Rotate(Tex resultTex, Texture previousTex, Texture heightmapTex, int dir)
	{
		Material worldMat = GetWorldMat(Mat.Rotate);
		worldMat.SetTexture("_PreviousTex", previousTex);
		float f = (float)Math.PI / 2f * (float)dir;
		worldMat.SetVector("_XMask", new Vector4(Mathf.Cos(f), 0f - Mathf.Sin(f)));
		worldMat.SetVector("_YMask", new Vector4(Mathf.Sin(f), Mathf.Cos(f)));
		worldMat.SetVector("_Pivot", new Vector4(0.5f, 0.5f));
		Render(resultTex, heightmapTex, worldMat);
	}

	public static void Smooths(Tex resultTex, Texture typeProcessedTex, Texture depthTex, Texture typeAtlasTex, int lod = 0)
	{
		Material worldMat = GetWorldMat(Mat.Smooths);
		worldMat.SetTexture("_DepthTex", depthTex);
		worldMat.SetTexture("_TypeAtlasTex", typeAtlasTex);
		worldMat.SetFloat("_Lod", lod);
		Render(resultTex, typeProcessedTex, worldMat);
	}

	public static void Stitch(Tex resultTex, Texture tex1, Texture tex2, Texture tex3, Texture tex4, int border, int inputSize)
	{
		Material worldMat = GetWorldMat(Mat.Stitch);
		worldMat.SetTexture("_Tex2", tex2);
		worldMat.SetTexture("_Tex3", tex3);
		worldMat.SetTexture("_Tex4", tex4);
		worldMat.SetVector("_InputSize", new Vector4(inputSize, inputSize));
		worldMat.SetFloat("_Border", border);
		worldMat.SetVector("_QuadrantMask", new Vector4((tex1 != null) ? 1 : 0, (tex2 != null) ? 1 : 0, (tex3 != null) ? 1 : 0, (tex4 != null) ? 1 : 0));
		Render(resultTex, tex1, worldMat, inputSize * 2, inputSize * 2);
	}

	public static void StitchData(Tex resultTex, Texture offsetTex, Texture xzTex, Texture columnTex, Texture dataTex1, Texture dataTex2, Texture dataTex3, Texture dataTex4, Color32 emptyColor, Color32 internalColor)
	{
		Material worldMat = GetWorldMat(Mat.StitchData);
		worldMat.SetTexture("_XZTex", xzTex);
		worldMat.SetTexture("_ColumnTex", columnTex);
		worldMat.SetTexture("_DataTex1", dataTex1);
		worldMat.SetTexture("_DataTex2", dataTex2);
		worldMat.SetTexture("_DataTex3", dataTex3);
		worldMat.SetTexture("_DataTex4", dataTex4);
		worldMat.SetVector("_ColumnSize", new Vector4(columnTex.width, columnTex.height));
		worldMat.SetVector("_DataSize1", (dataTex1 != null) ? new Vector4(dataTex1.width, dataTex1.height) : Vector4.zero);
		worldMat.SetVector("_DataSize2", (dataTex2 != null) ? new Vector4(dataTex2.width, dataTex2.height) : Vector4.zero);
		worldMat.SetVector("_DataSize3", (dataTex3 != null) ? new Vector4(dataTex3.width, dataTex3.height) : Vector4.zero);
		worldMat.SetVector("_DataSize4", (dataTex4 != null) ? new Vector4(dataTex4.width, dataTex4.height) : Vector4.zero);
		worldMat.SetVector("_QuadrantMask", new Vector4((dataTex1 != null) ? 1 : 0, (dataTex2 != null) ? 1 : 0, (dataTex3 != null) ? 1 : 0, (dataTex4 != null) ? 1 : 0));
		worldMat.SetColor("_EmptyColor", emptyColor);
		worldMat.SetColor("_InternalColor", internalColor);
		Render(resultTex, offsetTex, worldMat);
	}

	public static void Threshold(Tex resultTex, Texture dataTex, Color32 lowColor, Color32 highColor, Vector4 add)
	{
		Material worldMat = GetWorldMat(Mat.Threshold);
		worldMat.SetColor("_Low", lowColor);
		worldMat.SetColor("_High", highColor);
		worldMat.SetVector("_Add", add);
		Render(resultTex, dataTex, worldMat);
	}

	public static void Translate(Tex resultTex, Texture offsetTex, Texture xzTex, Texture columnTex, Texture dataTex, Vector3 translation, Vector3 scale, Color32 emptyColor, Color32 internalColor)
	{
		Material worldMat = GetWorldMat(Mat.Translate);
		worldMat.SetTexture("_XZTex", xzTex);
		worldMat.SetTexture("_ColumnTex", columnTex);
		worldMat.SetTexture("_DataTex", dataTex);
		worldMat.SetVector("_InputSize", new Vector4(dataTex.width, dataTex.height));
		worldMat.SetVector("_ColumnSize", new Vector4(columnTex.width, columnTex.height));
		worldMat.SetVector("_Translation", translation);
		worldMat.SetVector("_Scale", scale);
		worldMat.SetColor("_EmptyColor", emptyColor);
		worldMat.SetColor("_InternalColor", internalColor);
		Render(resultTex, offsetTex, worldMat);
	}

	public static void TypeLookupInit(Tex resultTex, Texture previousTex, Texture typeSampleTex, Color32 sourceColor, float destinationType)
	{
		Material worldMat = GetWorldMat(Mat.TypeLookupInit);
		worldMat.SetTexture("_PreviousTex", previousTex);
		worldMat.SetColor("_SourceColor", sourceColor);
		worldMat.SetFloat("_DestinationType", destinationType);
		Render(resultTex, typeSampleTex, worldMat);
	}

	public static void Types(Tex resultTex, Texture offsetTex, Texture worldTex, Vector3 translation = default(Vector3), int internalType = 8, int crustType = 6, int emptyType = 0)
	{
		Material worldMat = GetWorldMat(Mat.Types);
		worldMat.SetTexture("_WorldTex", worldTex);
		worldMat.SetVector("_ColumnSize", new Vector4(worldTex.width, worldTex.height));
		worldMat.SetVector("_Translation", translation);
		worldMat.SetFloat("_InternalType", internalType);
		worldMat.SetFloat("_CrustType", crustType);
		worldMat.SetFloat("_EmptyType", emptyType);
		Render(resultTex, offsetTex, worldMat);
	}

	public static void TypesDeep(Tex resultTex, Texture offsetTex, Texture noiseTex)
	{
		Material worldMat = GetWorldMat(Mat.TypesDeep);
		worldMat.SetTexture("_NoiseTex", noiseTex);
		Render(resultTex, offsetTex, worldMat);
	}

	public static void TypesSurface(Tex resultTex, Texture depthTex, int internalType = 8, int crustType = 6, int emptyType = 0)
	{
		Material worldMat = GetWorldMat(Mat.TypesSurface);
		worldMat.SetFloat("_InternalType", internalType);
		worldMat.SetFloat("_CrustType", crustType);
		worldMat.SetFloat("_EmptyType", emptyType);
		Render(resultTex, depthTex, worldMat);
	}

	public static void TypeTransitions(Tex resultTex, Texture previousTex, Texture typeProcessedTex, Texture transTex, Texture typeAtlasTex)
	{
		Material worldMat = GetWorldMat(Mat.TypeTransitions);
		worldMat.SetTexture("_TransTex", transTex);
		worldMat.SetTexture("_PreviousTex", previousTex);
		worldMat.SetTexture("_TypeAtlasTex", typeAtlasTex);
		Render(resultTex, typeProcessedTex, worldMat);
	}

	public static void UvBase(Tex resultTex, Texture typeProcessedTex, Texture typeAtlasTex, Texture noiseTex, Texture offsetTex, Texture xzTex, Vector3 translation)
	{
		Material worldMat = GetWorldMat(Mat.UvBase);
		worldMat.SetTexture("_TypeAtlasTex", typeAtlasTex);
		worldMat.SetTexture("_NoiseTex", noiseTex);
		worldMat.SetTexture("_OffsetTex", offsetTex);
		worldMat.SetVector("_InputSize", new Vector4(typeProcessedTex.width, typeProcessedTex.height));
		worldMat.SetVector("_WorldSize", new Vector4(xzTex.width, 256f, xzTex.height));
		worldMat.SetVector("_NoiseSize", new Vector4(noiseTex.width, noiseTex.height));
		worldMat.SetVector("_Translation", translation);
		Render(resultTex, typeProcessedTex, worldMat);
	}

	public static void UvSpecial(Tex resultTex, Texture typeProcessedTex, Texture typeAtlasTex, Texture uvBaseTex)
	{
		Material worldMat = GetWorldMat(Mat.UvSpecial);
		worldMat.SetTexture("_TypeAtlasTex", typeAtlasTex);
		worldMat.SetTexture("_UvTex", uvBaseTex);
		Render(resultTex, typeProcessedTex, worldMat);
	}

	public static void UvTransitions(Tex resultTex, Texture typeTransTex, Texture typeAtlasTex)
	{
		Material worldMat = GetWorldMat(Mat.UvTransitions);
		worldMat.SetTexture("_TypeAtlasTex", typeAtlasTex);
		Render(resultTex, typeTransTex, worldMat);
	}

	public static void Volumes(Tex volumeTex, Texture offsetTex, Texture worldTex, Vector3 translation = default(Vector3))
	{
		Material worldMat = GetWorldMat(Mat.Volumes);
		worldMat.SetTexture("_WorldTex", worldTex);
		worldMat.SetVector("_ColumnSize", new Vector4(worldTex.width, worldTex.height));
		worldMat.SetVector("_Translation", translation);
		Render(volumeTex, offsetTex, worldMat);
	}

	public static void XZEnd(Tex resultTex, Tex previousTex, Texture columnTex)
	{
		Material worldMat = GetWorldMat(Mat.XZEnd);
		worldMat.SetTexture("_ColumnTex", columnTex);
		Render(resultTex, previousTex, worldMat);
	}

	public static void XZInit(Tex resultTex, Texture columnTex)
	{
		Material worldMat = GetWorldMat(Mat.XZInit);
		Render(resultTex, columnTex, worldMat);
	}

	public static void XZSumCol(Tex resultTex, Tex previousTex, int step = 1)
	{
		Material worldMat = GetWorldMat(Mat.XZSumCol);
		worldMat.SetVector("_InputSize", new Vector4(previousTex.width, previousTex.height));
		worldMat.SetFloat("_Step", step);
		Render(resultTex, previousTex, worldMat);
	}

	public static void XZSumRow(Tex resultTex, Tex previousTex, int step = 1)
	{
		Material worldMat = GetWorldMat(Mat.XZSumRow);
		worldMat.SetVector("_InputSize", new Vector4(previousTex.width, previousTex.height));
		worldMat.SetFloat("_Step", step);
		Render(resultTex, previousTex, worldMat);
	}

	public static void WarpedNoiseLookup(Tex resultTex, Texture noiseTex, Texture typeSampleTex, Texture warpTex)
	{
		Material worldMat = GetWorldMat(Mat.WarpedNoiseLookup);
		worldMat.SetTexture("_WarpTex", warpTex);
		worldMat.SetTexture("_NoiseTex", noiseTex);
		worldMat.SetTexture("_TypeSampleTex", typeSampleTex);
		worldMat.SetVector("_TypeSampleSize", new Vector4(typeSampleTex.width, typeSampleTex.height));
		Render(resultTex, noiseTex, worldMat);
	}

	public static void World(Tex resultTex, Texture loadedTex)
	{
		Material worldMat = GetWorldMat(Mat.World);
		Render(resultTex, loadedTex, worldMat);
	}

	private static Material GetWorldMat(Mat mat)
	{
		return mat.GetMaterial();
	}

	private static void Render(Tex A, Texture B, Material mat, int width = 0, int height = 0)
	{
		A.Render(B, mat, width, height);
	}
}
