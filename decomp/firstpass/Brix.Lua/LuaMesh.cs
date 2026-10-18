using Brix.External.Factories;
using Brix.Utils;
using MoonSharp.Interpreter;
using UnityEngine;

namespace Brix.Lua;

public abstract class LuaMesh
{
	public static LuaObjectDescriptor<Mesh> desc = new LuaObjectDescriptor<Mesh>(typeof(LuaMesh));

	public static void RegisterClass(Script script)
	{
		script.Globals["Mesh"] = desc.GetStaticProxy();
	}

	[LuaDrivable("New")]
	public static DynValue New(ScriptExecutionContext executionContext, CallbackArguments args)
	{
		return UserData.CreateWithDescriptor(new Mesh(), desc);
	}

	[LuaDrivable("SetOnAssetKey")]
	public static DynValue SetMeshOnKey(ScriptExecutionContext executionContext, CallbackArguments args)
	{
		Factory.AssetKey key = args.AsUserData<Factory.AssetKey>(0, "SetOnAssetKey");
		Mesh sharedMesh = args.AsUserData<Mesh>(1, "SetOnAssetKey");
		string text = args.AsString(2, "SetOnAssetKey", allowNil: true);
		GameObject gameObject = Factory.Peek(key);
		bool v = false;
		if (gameObject != null)
		{
			Transform transform = ((!string.IsNullOrEmpty(text)) ? gameObject.transform.Find(text) : gameObject.transform);
			if (transform != null)
			{
				MeshFilter componentInChildren = transform.GetComponentInChildren<MeshFilter>();
				if (componentInChildren != null)
				{
					componentInChildren.sharedMesh = sharedMesh;
					v = true;
				}
			}
		}
		return DynValue.NewBoolean(v);
	}

	[LuaDrivable("SetOnDriver")]
	public static DynValue SetMeshOnDriver(ScriptExecutionContext executionContext, CallbackArguments args)
	{
		GameObject gameObject = args.AsGameObject(0, "SetOnDriver");
		Mesh sharedMesh = args.AsUserData<Mesh>(1, "SetOnDriver");
		string text = args.AsString(2, "SetOnDriver", allowNil: true);
		bool v = false;
		Transform transform = ((!string.IsNullOrEmpty(text)) ? gameObject.transform.Find(text) : gameObject.transform);
		if (transform != null)
		{
			MeshFilter componentInChildren = transform.GetComponentInChildren<MeshFilter>();
			if (componentInChildren != null)
			{
				componentInChildren.sharedMesh = sharedMesh;
				v = true;
			}
		}
		return DynValue.NewBoolean(v);
	}

	[LuaDrivable("LoadOBJ")]
	private static DynValue LoadOBJ(ScriptExecutionContext context, CallbackArguments args)
	{
		string filePath = args.AsString(0, "LoadOBJ");
		Quaternion quaternion = args.AsUserData<Quaternion>(1, "LoadOBJ", allowNil: true);
		Mesh mesh = ObjImporter.ImportFile(filePath);
		if (quaternion != Quaternion.identity)
		{
			Vector3[] normals = mesh.normals;
			for (int i = 0; i != normals.Length; i++)
			{
				ref Vector3 reference = ref normals[i];
				reference = quaternion * normals[i];
			}
			mesh.normals = normals;
		}
		RecalculateTangents(mesh);
		return UserData.CreateWithDescriptor(mesh, desc);
	}

	public static void RecalculateTangents(Mesh theMesh)
	{
		int vertexCount = theMesh.vertexCount;
		Vector3[] vertices = theMesh.vertices;
		Vector3[] normals = theMesh.normals;
		Vector2[] uv = theMesh.uv;
		int[] triangles = theMesh.triangles;
		int num = triangles.Length / 3;
		Vector4[] array = new Vector4[vertexCount];
		Vector3[] array2 = new Vector3[vertexCount];
		Vector3[] array3 = new Vector3[vertexCount];
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			int num3 = triangles[num2];
			int num4 = triangles[num2 + 1];
			int num5 = triangles[num2 + 2];
			Vector3 vector = vertices[num3];
			Vector3 vector2 = vertices[num4];
			Vector3 vector3 = vertices[num5];
			Vector2 vector4 = uv[num3];
			Vector2 vector5 = uv[num4];
			Vector2 vector6 = uv[num5];
			float num6 = vector2.x - vector.x;
			float num7 = vector3.x - vector.x;
			float num8 = vector2.y - vector.y;
			float num9 = vector3.y - vector.y;
			float num10 = vector2.z - vector.z;
			float num11 = vector3.z - vector.z;
			float num12 = vector5.x - vector4.x;
			float num13 = vector6.x - vector4.x;
			float num14 = vector5.y - vector4.y;
			float num15 = vector6.y - vector4.y;
			float num16 = 1f / (num12 * num15 - num13 * num14);
			Vector3 vector7 = new Vector3((num15 * num6 - num14 * num7) * num16, (num15 * num8 - num14 * num9) * num16, (num15 * num10 - num14 * num11) * num16);
			Vector3 vector8 = new Vector3((num12 * num7 - num13 * num6) * num16, (num12 * num9 - num13 * num8) * num16, (num12 * num11 - num13 * num10) * num16);
			array2[num3] += vector7;
			array2[num4] += vector7;
			array2[num5] += vector7;
			array3[num3] += vector8;
			array3[num4] += vector8;
			array3[num5] += vector8;
			num2 += 3;
		}
		for (int j = 0; j < vertexCount; j++)
		{
			Vector3 normal = normals[j];
			Vector3 tangent = array2[j];
			Vector3.OrthoNormalize(ref normal, ref tangent);
			array[j].x = tangent.x;
			array[j].y = tangent.y;
			array[j].z = tangent.z;
			array[j].w = ((!(Vector3.Dot(Vector3.Cross(normal, tangent), array3[j]) < 0f)) ? 1f : (-1f));
		}
		theMesh.tangents = array;
	}
}
