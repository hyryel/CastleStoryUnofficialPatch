using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using Brix.Utils;
using UnityEngine;

public class FastReflectionTestScript : MonoBehaviour
{
	public struct TestStruct
	{
		private int value;

		public TestStruct(int value)
		{
			this.value = value;
		}

		public void ByRef(ref int x, int y, out string z)
		{
			x = -1;
			y = 10;
			z = "OUT";
		}
	}

	public class TestClass
	{
		public string TestProperty { get; set; }

		public int Add(int x, int y)
		{
			return x + y;
		}

		public static void Print(string msg)
		{
			if (!string.IsNullOrEmpty(msg))
			{
				UnityEngine.Debug.Log(msg);
			}
		}
	}

	public class BenchmarkSubject
	{
		public int InstanceField;

		public static Transform StaticField;

		public string InstanceProperty { get; set; }

		public static Vector3 StaticProperty { get; set; }

		public void InstanceMethod()
		{
		}

		public static void StaticMethod(float x)
		{
		}
	}

	public int loops = 100000;

	private static FieldInfo instField = typeof(BenchmarkSubject).GetField("InstanceField");

	private static FieldInfo staticField = typeof(BenchmarkSubject).GetField("StaticField");

	private static PropertyInfo instProperty = typeof(BenchmarkSubject).GetProperty("InstanceProperty");

	private static PropertyInfo staticProperty = typeof(BenchmarkSubject).GetProperty("StaticProperty");

	private static MethodInfo instanceMethod = typeof(BenchmarkSubject).GetMethod("InstanceMethod");

	private static MethodInfo staticMethod = typeof(BenchmarkSubject).GetMethod("StaticMethod");

	private static object[] args = new object[1] { 3.14f };

	private BenchmarkSubject strong = new BenchmarkSubject();

	private Stopwatch sw = new Stopwatch();

	private StringBuilder builder = new StringBuilder();

	private string benchmarkResult;

	public void OnGUI()
	{
		if (GUILayout.Button("Class demo"))
		{
			ClassDemo();
		}
		if (GUILayout.Button("Struct demo"))
		{
			StructDemo();
		}
		if (GUILayout.Button("Benchmark"))
		{
			benchmarkResult = Benchmarks();
		}
		if (!string.IsNullOrEmpty(benchmarkResult))
		{
			GUILayout.TextArea(benchmarkResult);
		}
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("DbgAsm1"))
		{
			GenDbgAsm1();
		}
		if (GUILayout.Button("DbgAsm2"))
		{
			GenDbgAsm2();
		}
		if (GUILayout.Button("DbgAsm3"))
		{
			GenDbgAsm3();
		}
		GUILayout.EndHorizontal();
	}

	public void ClassDemo()
	{
		Type typeFromHandle = typeof(TestClass);
		PropertyInfo property = typeFromHandle.GetProperty("TestProperty");
		MethodInfo method = typeFromHandle.GetMethod("Add");
		MethodInfo method2 = typeFromHandle.GetMethod("Print", BindingFlags.Static | BindingFlags.Public);
		Ctor<object> ctor = typeFromHandle.DelegateForCtor(Type.EmptyTypes);
		object target = ctor(null);
		Setter<object, object> setter = property.DelegateForSet();
		Getter<object, object> getter = property.DelegateForGet();
		setter(ref target, "Hello");
		UnityEngine.Debug.Log(getter(target));
		Caller<object, object> caller = method.DelegateForCall();
		object message = caller(target, new object[2] { 1, 2 });
		UnityEngine.Debug.Log(message);
		Caller<object, object> caller2 = method2.DelegateForCall();
		caller2(null, new object[1] { "IL is pretty metal!" });
	}

	public void StructDemo()
	{
		Type typeFromHandle = typeof(TestStruct);
		FieldInfo field = typeFromHandle.GetField("value", BindingFlags.Instance | BindingFlags.NonPublic);
		MethodInfo method = typeFromHandle.GetMethod("ByRef");
		Ctor<TestStruct> ctor = typeFromHandle.DelegateForCtor<TestStruct>(new Type[1] { typeof(int) });
		TestStruct target = ctor(new object[1] { 10 });
		Setter<TestStruct, int> setter = field.DelegateForSet<TestStruct, int>();
		Getter<TestStruct, int> getter = field.DelegateForGet<TestStruct, int>();
		setter(ref target, 13);
		UnityEngine.Debug.Log(getter(target));
		object[] array = new object[3] { 5, 2, "IN" };
		Caller<object, object> caller = method.DelegateForCall();
		caller(target, array);
		UnityEngine.Debug.Log(array[0]);
		UnityEngine.Debug.Log(array[1]);
		UnityEngine.Debug.Log(array[2]);
	}

	public string Benchmarks()
	{
		Action<string, Action> action = delegate(string msg, Action code)
		{
			sw.Start();
			for (int i = 0; i < loops; i++)
			{
				code();
			}
			sw.Stop();
			builder.Append(msg);
			builder.Append(": ");
			builder.Append(sw.ElapsedMilliseconds.ToString());
			builder.AppendLine("ms");
			sw.Reset();
		};
		builder.Length = 0;
		object weak = strong;
		Transform xform = base.transform;
		Vector3 vec = Vector3.zero;
		action("Direct set instance field", delegate
		{
			strong.InstanceField = 10;
		});
		action("Direct get instance field", delegate
		{
			int instanceField = strong.InstanceField;
		});
		action("Direct set instance property", delegate
		{
			strong.InstanceProperty = "Str";
		});
		action("Direct get instance property", delegate
		{
			string instanceProperty = strong.InstanceProperty;
		});
		action("Direct set static field", delegate
		{
			BenchmarkSubject.StaticField = xform;
		});
		action("Direct get static field", delegate
		{
			Transform transform = BenchmarkSubject.StaticField;
		});
		action("Direct set static property", delegate
		{
			BenchmarkSubject.StaticProperty = vec;
		});
		action("Direct get static property", delegate
		{
			Vector3 vector = BenchmarkSubject.StaticProperty;
		});
		action("Direct invoke instance method", delegate
		{
			strong.InstanceMethod();
		});
		action("Direct invoke static method", delegate
		{
			BenchmarkSubject.StaticMethod(3.14f);
		});
		builder.AppendLine("-----");
		action("Reflection instance set field", delegate
		{
			instField.SetValue(strong, 10);
		});
		action("Reflection instance get field", delegate
		{
			instField.GetValue(strong);
		});
		action("Reflection instance set property", delegate
		{
			instProperty.SetValue(strong, "Str", null);
		});
		action("Reflection instance get property", delegate
		{
			instProperty.GetValue(strong, null);
		});
		action("Reflection static set field", delegate
		{
			staticField.SetValue(null, xform);
		});
		action("Reflection static get field", delegate
		{
			staticField.GetValue(null);
		});
		action("Reflection static set property", delegate
		{
			staticProperty.SetValue(null, vec, null);
		});
		action("Reflection static get property", delegate
		{
			staticProperty.GetValue(null, null);
		});
		action("Reflection invoke instance method", delegate
		{
			instanceMethod.Invoke(strong, null);
		});
		action("Reflection invoke static method", delegate
		{
			staticMethod.Invoke(null, args);
		});
		builder.AppendLine("-----");
		Setter<object, object> weakInstanceFieldSetter = instField.DelegateForSet();
		Getter<object, object> weakInstanceFieldGetter = instField.DelegateForGet();
		Setter<object, object> weakInstancePropertySetter = instProperty.DelegateForSet();
		Getter<object, object> weakInstancePropertyGetter = instProperty.DelegateForGet();
		Setter<object, object> weakStaticFieldSetter = staticField.DelegateForSet();
		Getter<object, object> weakStaticFieldGetter = staticField.DelegateForGet();
		Setter<object, object> weakStaticPropertySetter = staticProperty.DelegateForSet();
		Getter<object, object> weakStaticPropertyGetter = staticProperty.DelegateForGet();
		Caller<object, object> weakInstanceMethodCaller = instanceMethod.DelegateForCall();
		Caller<object, object> weakStaticMethodCaller = staticMethod.DelegateForCall();
		action("weak cached delegate instance set field", delegate
		{
			weakInstanceFieldSetter(ref weak, 10);
		});
		action("weak cached delegate instance get field", delegate
		{
			weakInstanceFieldGetter(weak);
		});
		action("weak cached delegate instance set property", delegate
		{
			weakInstancePropertySetter(ref weak, "Str");
		});
		action("weak cached delegate instance get property", delegate
		{
			weakInstancePropertyGetter(weak);
		});
		action("weak cached delegate static set field", delegate
		{
			weakStaticFieldSetter(ref weak, xform);
		});
		action("weak cached delegate static get field", delegate
		{
			weakStaticFieldGetter(null);
		});
		action("weak cached delegate static set property", delegate
		{
			weakStaticPropertySetter(ref weak, vec);
		});
		action("weak cached delegate static get property", delegate
		{
			weakStaticPropertyGetter(null);
		});
		action("weak cached delegate invoke instance method", delegate
		{
			weakInstanceMethodCaller(weak, null);
		});
		action("weak cached delegate invoke static method", delegate
		{
			weakStaticMethodCaller(null, args);
		});
		builder.AppendLine("-----");
		Setter<BenchmarkSubject, int> strongInstanceFieldSetter = instField.DelegateForSet<BenchmarkSubject, int>();
		Getter<BenchmarkSubject, int> strongInstanceFieldGetter = instField.DelegateForGet<BenchmarkSubject, int>();
		Setter<BenchmarkSubject, string> strongInstancePropertySetter = instProperty.DelegateForSet<BenchmarkSubject, string>();
		Getter<BenchmarkSubject, string> strongInstancePropertyGetter = instProperty.DelegateForGet<BenchmarkSubject, string>();
		Setter<BenchmarkSubject, Transform> strongStaticFieldSetter = staticField.DelegateForSet<BenchmarkSubject, Transform>();
		Getter<BenchmarkSubject, Transform> strongStaticFieldGetter = staticField.DelegateForGet<BenchmarkSubject, Transform>();
		Setter<BenchmarkSubject, Vector3> strongStaticPropertySetter = staticProperty.DelegateForSet<BenchmarkSubject, Vector3>();
		Getter<BenchmarkSubject, Vector3> strongStaticPropertyGetter = staticProperty.DelegateForGet<BenchmarkSubject, Vector3>();
		Caller<BenchmarkSubject, object> strongInstanceMethodCaller = instanceMethod.DelegateForCall<BenchmarkSubject, object>();
		Caller<object, object> strongStaticMethodCaller = staticMethod.DelegateForCall<object, object>();
		action("strong cached delegate instance set field", delegate
		{
			strongInstanceFieldSetter(ref strong, 10);
		});
		action("strong cached delegate instance get field", delegate
		{
			strongInstanceFieldGetter(strong);
		});
		action("strong cached delegate instance set property", delegate
		{
			strongInstancePropertySetter(ref strong, "Str");
		});
		action("strong cached delegate instance get property", delegate
		{
			strongInstancePropertyGetter(strong);
		});
		action("strong cached delegate static set field", delegate
		{
			strongStaticFieldSetter(ref strong, xform);
		});
		action("strong cached delegate static get field", delegate
		{
			strongStaticFieldGetter(null);
		});
		action("strong cached delegate static set property", delegate
		{
			strongStaticPropertySetter(ref strong, vec);
		});
		action("strong cached delegate static get property", delegate
		{
			strongStaticPropertyGetter(null);
		});
		action("strong cached delegate invoke instance method", delegate
		{
			strongInstanceMethodCaller(strong, null);
		});
		action("strong cached delegate invoke static method", delegate
		{
			strongStaticMethodCaller(null, args);
		});
		return builder.ToString();
	}

	private void Update()
	{
		object target = strong;
		Getter<BenchmarkSubject, int> getter = instField.DelegateForGet<BenchmarkSubject, int>();
		Setter<BenchmarkSubject, int> setter = instField.DelegateForSet<BenchmarkSubject, int>();
		Getter<object, object> getter2 = instField.DelegateForGet();
		Setter<object, object> setter2 = instField.DelegateForSet();
		Getter<object, object> getter3 = instProperty.DelegateForGet();
		Setter<object, object> setter3 = instProperty.DelegateForSet();
		Caller<object, object> caller = instanceMethod.DelegateForCall();
		Caller<object, object> caller2 = staticMethod.DelegateForCall();
		setter(ref strong, 10);
		int num = getter(strong);
		setter2(ref target, 10);
		object obj = getter2(target);
		setter3(ref target, "SomeStr");
		object obj2 = getter3(strong);
		caller(target, null);
		caller2(strong, args);
	}

	public void GenDbgAsm1()
	{
		Type typeFromHandle = typeof(TestStruct);
		FieldInfo field = typeFromHandle.GetField("value", BindingFlags.Instance | BindingFlags.NonPublic);
		MethodInfo method = typeFromHandle.GetMethod("ByRef");
		FastReflection.GenDebugAssembly("dbg1.dll", field, null, method, null, null);
	}

	public void GenDbgAsm2()
	{
		Type typeFromHandle = typeof(TestClass);
		PropertyInfo property = typeFromHandle.GetProperty("TestProperty");
		MethodInfo method = typeFromHandle.GetMethod("Add");
		FastReflection.GenDebugAssembly<TestClass>("dbg2.dll", null, property, method, null, null);
	}

	public void GenDbgAsm3()
	{
		Type typeFromHandle = typeof(BenchmarkSubject);
		PropertyInfo property = typeFromHandle.GetProperty("StaticProperty");
		FieldInfo field = typeFromHandle.GetField("StaticField");
		FastReflection.GenDebugAssembly("dbg3.dll", field, property, null, null, null);
	}
}
