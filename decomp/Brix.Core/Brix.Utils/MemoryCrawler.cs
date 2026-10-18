using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Brix.Utils;

public class MemoryCrawler
{
	private class ArrayItemInfo : MemberInfo
	{
		public Type ItemType;

		public int Index;

		public override Type DeclaringType
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public override MemberTypes MemberType
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public override string Name
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public override Type ReflectedType
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public override object[] GetCustomAttributes(Type attributeType, bool inherit)
		{
			throw new NotImplementedException();
		}

		public override bool IsDefined(Type attributeType, bool inherit)
		{
			throw new NotImplementedException();
		}

		public override object[] GetCustomAttributes(bool inherit)
		{
			throw new NotImplementedException();
		}
	}

	private class MonoBehaviourInfo : MemberInfo
	{
		public MonoBehaviour M;

		public override Type DeclaringType
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public override MemberTypes MemberType
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public override string Name => M.name;

		public override Type ReflectedType => M.GetType();

		public override object[] GetCustomAttributes(Type attributeType, bool inherit)
		{
			throw new NotImplementedException();
		}

		public override bool IsDefined(Type attributeType, bool inherit)
		{
			throw new NotImplementedException();
		}

		public override object[] GetCustomAttributes(bool inherit)
		{
			throw new NotImplementedException();
		}
	}

	private readonly HashSet<object> _visitedInstances = new HashSet<object>();

	public int GenericTypesFound = 0;

	public int MaxDepth = -1;

	public int LogDepth = -1;

	public string LogPath = "dump.txt";

	public long GetTrackableMemoryUsage()
	{
		_visitedInstances.Clear();
		GenericTypesFound = 0;
		if (LogDepth >= 0)
		{
			File.WriteAllText(LogPath, "");
		}
		long num = 0L;
		Stack<MemberInfo> stack = new Stack<MemberInfo>();
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (Assembly assembly in assemblies)
		{
			if (assembly.FullName.StartsWith("Assembly-CSharp") || assembly.FullName.StartsWith("Brix.Core"))
			{
				Type[] types = assembly.GetTypes();
				foreach (Type type in types)
				{
					stack.Push(type);
					num += GetTypeStaticMemoryUsage(stack, type);
					stack.Pop();
				}
			}
		}
		return num + GetMonoBehaviourMemoryUsage(stack);
	}

	private long GetMonoBehaviourMemoryUsage(Stack<MemberInfo> path)
	{
		long num = 0L;
		MonoBehaviour[] array = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>();
		foreach (MonoBehaviour monoBehaviour in array)
		{
			path.Push(new MonoBehaviourInfo
			{
				M = monoBehaviour
			});
			num += GetRecursiveInstanceMemoryUsage(path, monoBehaviour.GetType(), monoBehaviour, 0);
			path.Pop();
		}
		return num;
	}

	private long GetTypeStaticMemoryUsage(Stack<MemberInfo> path, Type type)
	{
		if (type.IsImport || !type.IsVisible)
		{
			return 0L;
		}
		if (type.IsGenericType)
		{
			if (type.ContainsGenericParameters)
			{
				return 0L;
			}
			GenericTypesFound++;
		}
		if (!InitType(type))
		{
			return 0L;
		}
		long num = 0L;
		FieldInfo[] fields = type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		FieldInfo[] array = fields;
		foreach (FieldInfo fieldInfo in array)
		{
			if (!IsCompilerGenerated(fieldInfo))
			{
				path.Push(fieldInfo);
				num += GetRecursiveInstanceMemoryUsage(path, fieldInfo.FieldType, fieldInfo.GetValue(null), 0);
				path.Pop();
			}
		}
		return num;
	}

	private long GetRecursiveInstanceMemoryUsage(Stack<MemberInfo> path, Type type, object o, int depth)
	{
		if (type.IsPrimitive)
		{
			return Marshal.SizeOf(type);
		}
		if (type.IsEnum)
		{
			return 4L;
		}
		long num = ((!type.IsValueType) ? IntPtr.Size : 0);
		if (o == null || (!type.IsValueType && !_visitedInstances.Add(o)) || depth == MaxDepth)
		{
			return num;
		}
		type = o.GetType();
		if (IsSystemType(type) && !typeof(ICollection).IsAssignableFrom(type))
		{
			return num;
		}
		if (!InitType(type))
		{
			return num;
		}
		FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		FieldInfo[] array = fields;
		foreach (FieldInfo fieldInfo in array)
		{
			if (!IsCompilerGenerated(fieldInfo))
			{
				path.Push(fieldInfo);
				num += GetRecursiveInstanceMemoryUsage(path, fieldInfo.FieldType, fieldInfo.GetValue(o), depth + 1);
				path.Pop();
			}
		}
		if (o is Array array2)
		{
			Type elementType = type.GetElementType();
			ArrayItemInfo arrayItemInfo = new ArrayItemInfo();
			foreach (object item in array2)
			{
				Type type2 = (arrayItemInfo.ItemType = ((elementType != null && elementType.IsValueType) ? elementType : ((item != null) ? item.GetType() : typeof(object))));
				path.Push(arrayItemInfo);
				num += GetRecursiveInstanceMemoryUsage(path, type2, item, depth + 1);
				path.Pop();
				arrayItemInfo.Index++;
			}
		}
		if (LogDepth >= depth)
		{
			File.AppendAllText(LogPath, string.Format("{3,-12} {0} (Type='{1}', Depth='{2}')\n", TranslatePath(path), type, depth, num));
		}
		return num;
	}

	private bool IsSystemType(Type type)
	{
		return type.Assembly.FullName.StartsWith("mscorlib");
	}

	private bool IsCompilerGenerated(FieldInfo field)
	{
		return field.IsAssembly || field.IsDefined(typeof(CompilerGeneratedAttribute), inherit: false) || field.IsDefined(typeof(DllImportAttribute), inherit: false) || field.IsDefined(typeof(MethodImplAttribute), inherit: false);
	}

	private bool InitType(Type type)
	{
		try
		{
			RuntimeHelpers.RunClassConstructor(type.TypeHandle);
		}
		catch (Exception arg)
		{
			if (LogDepth >= 0)
			{
				File.AppendAllText(LogPath, $"Ctor error for '{type.Name}': {arg}");
			}
			return false;
		}
		return true;
	}

	public string FindTypeInstanceInStaticMemory(Type targetType)
	{
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (Assembly assembly in assemblies)
		{
			if (!assembly.FullName.StartsWith("Assembly-CSharp") && !assembly.FullName.StartsWith("Brix.Core"))
			{
				continue;
			}
			Type[] types = assembly.GetTypes();
			foreach (Type type in types)
			{
				string text = FindTypeInstanceInType(targetType, type, new Stack<MemberInfo>());
				if (!string.IsNullOrEmpty(text))
				{
					return text;
				}
			}
		}
		return null;
	}

	private string FindTypeInstanceInType(Type targetType, Type type, Stack<MemberInfo> path)
	{
		if (type.IsImport || !type.IsVisible)
		{
			return null;
		}
		if (type.IsGenericType)
		{
			if (type.ContainsGenericParameters)
			{
				return null;
			}
			GenericTypesFound++;
		}
		if (!InitType(type))
		{
			return null;
		}
		path.Push(type);
		FieldInfo[] fields = type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		FieldInfo[] array = fields;
		foreach (FieldInfo fieldInfo in array)
		{
			if (!IsCompilerGenerated(fieldInfo))
			{
				path.Push(fieldInfo);
				string text = FindRecursiveTypeInstance(targetType, path, fieldInfo.FieldType, fieldInfo.GetValue(null), 0);
				path.Pop();
				if (!string.IsNullOrEmpty(text))
				{
					return text;
				}
			}
		}
		path.Pop();
		return null;
	}

	private string FindRecursiveTypeInstance(Type targetType, Stack<MemberInfo> path, Type type, object o, int depth)
	{
		if (type.IsPrimitive)
		{
			return null;
		}
		if (type.IsEnum)
		{
			return null;
		}
		if (o == null || (!type.IsValueType && !_visitedInstances.Add(o)) || depth == MaxDepth)
		{
			return null;
		}
		if (targetType.IsInstanceOfType(o))
		{
			return TranslatePath(path);
		}
		type = o.GetType();
		if (IsSystemType(type) && !typeof(ICollection).IsAssignableFrom(type))
		{
			return null;
		}
		if (LogDepth >= depth)
		{
			File.AppendAllText(LogPath, $"{path} (Type='{type}', Depth='{depth}')\n");
		}
		if (!InitType(type))
		{
			return null;
		}
		FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		FieldInfo[] array = fields;
		foreach (FieldInfo fieldInfo in array)
		{
			if (!IsCompilerGenerated(fieldInfo))
			{
				path.Push(fieldInfo);
				string text = FindRecursiveTypeInstance(targetType, path, fieldInfo.FieldType, fieldInfo.GetValue(o), depth + 1);
				path.Pop();
				if (!string.IsNullOrEmpty(text))
				{
					return text;
				}
			}
		}
		if (o is Array { Length: not 0 } array2)
		{
			Type elementType = type.GetElementType();
			ArrayItemInfo arrayItemInfo = new ArrayItemInfo();
			foreach (object item in array2)
			{
				Type type2 = (arrayItemInfo.ItemType = ((elementType != null && elementType.IsValueType) ? elementType : ((item != null) ? item.GetType() : typeof(object))));
				path.Push(arrayItemInfo);
				string text2 = FindRecursiveTypeInstance(targetType, path, type2, item, depth + 1);
				path.Pop();
				if (!string.IsNullOrEmpty(text2))
				{
					return text2;
				}
				arrayItemInfo.Index++;
			}
		}
		return null;
	}

	private string TranslatePath(Stack<MemberInfo> path)
	{
		string text = "";
		foreach (MemberInfo item in path)
		{
			if (text != "")
			{
				text = "/" + text;
			}
			if (item is FieldInfo)
			{
				text = string.Concat(item.Name, "(", (item as FieldInfo).FieldType, ")", text);
			}
			else if (item is Type)
			{
				text = item.Name + text;
			}
			else if (item is ArrayItemInfo)
			{
				text = string.Concat(":", (item as ArrayItemInfo).Index, "(", (item as ArrayItemInfo).ItemType, ")", text);
			}
			else if (item is MonoBehaviourInfo)
			{
				text = string.Concat(item.Name, "(", item.ReflectedType, ")", text);
			}
		}
		return text;
	}
}
