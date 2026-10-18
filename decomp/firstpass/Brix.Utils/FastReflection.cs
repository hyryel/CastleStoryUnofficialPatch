using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Brix.Utils;

public static class FastReflection
{
	private class ILEmitter
	{
		public ILGenerator il;

		public ILEmitter ret()
		{
			lock (emit)
			{
				il.Emit(OpCodes.Ret);
				return this;
			}
		}

		public ILEmitter cast(Type type)
		{
			lock (emit)
			{
				il.Emit(OpCodes.Castclass, type);
				return this;
			}
		}

		public ILEmitter box(Type type)
		{
			lock (emit)
			{
				il.Emit(OpCodes.Box, type);
				return this;
			}
		}

		public ILEmitter unbox_any(Type type)
		{
			lock (emit)
			{
				il.Emit(OpCodes.Unbox_Any, type);
				return this;
			}
		}

		public ILEmitter unbox(Type type)
		{
			lock (emit)
			{
				il.Emit(OpCodes.Unbox, type);
				return this;
			}
		}

		public ILEmitter call(MethodInfo method)
		{
			lock (emit)
			{
				il.Emit(OpCodes.Call, method);
				return this;
			}
		}

		public ILEmitter callvirt(MethodInfo method)
		{
			lock (emit)
			{
				il.Emit(OpCodes.Callvirt, method);
				return this;
			}
		}

		public ILEmitter ldnull()
		{
			lock (emit)
			{
				il.Emit(OpCodes.Ldnull);
				return this;
			}
		}

		public ILEmitter bne_un(Label target)
		{
			lock (emit)
			{
				il.Emit(OpCodes.Bne_Un, target);
				return this;
			}
		}

		public ILEmitter beq(Label target)
		{
			lock (emit)
			{
				il.Emit(OpCodes.Beq, target);
				return this;
			}
		}

		public ILEmitter ldc_i4_0()
		{
			lock (emit)
			{
				il.Emit(OpCodes.Ldc_I4_0);
				return this;
			}
		}

		public ILEmitter ldc_i4_1()
		{
			lock (emit)
			{
				il.Emit(OpCodes.Ldc_I4_1);
				return this;
			}
		}

		public ILEmitter ldc_i4(int c)
		{
			lock (emit)
			{
				il.Emit(OpCodes.Ldc_I4, c);
				return this;
			}
		}

		public ILEmitter ldarg0()
		{
			lock (emit)
			{
				il.Emit(OpCodes.Ldarg_0);
				return this;
			}
		}

		public ILEmitter ldarg1()
		{
			lock (emit)
			{
				il.Emit(OpCodes.Ldarg_1);
				return this;
			}
		}

		public ILEmitter ldarg2()
		{
			lock (emit)
			{
				il.Emit(OpCodes.Ldarg_2);
				return this;
			}
		}

		public ILEmitter ldarga(int idx)
		{
			lock (emit)
			{
				il.Emit(OpCodes.Ldarga, idx);
				return this;
			}
		}

		public ILEmitter ldarga_s(int idx)
		{
			lock (emit)
			{
				il.Emit(OpCodes.Ldarga_S, idx);
				return this;
			}
		}

		public ILEmitter ldarg(int idx)
		{
			lock (emit)
			{
				il.Emit(OpCodes.Ldarg, idx);
				return this;
			}
		}

		public ILEmitter ldarg_s(int idx)
		{
			lock (emit)
			{
				il.Emit(OpCodes.Ldarg_S, idx);
				return this;
			}
		}

		public ILEmitter ifclass_ldind_ref(Type type)
		{
			lock (emit)
			{
				if (!type.IsValueType)
				{
					il.Emit(OpCodes.Ldind_Ref);
				}
				return this;
			}
		}

		public ILEmitter ldloc0()
		{
			lock (emit)
			{
				il.Emit(OpCodes.Ldloc_0);
				return this;
			}
		}

		public ILEmitter ldloc1()
		{
			lock (emit)
			{
				il.Emit(OpCodes.Ldloc_1);
				return this;
			}
		}

		public ILEmitter ldloc2()
		{
			lock (emit)
			{
				il.Emit(OpCodes.Ldloc_2);
				return this;
			}
		}

		public ILEmitter ldloca_s(int idx)
		{
			lock (emit)
			{
				il.Emit(OpCodes.Ldloca_S, idx);
				return this;
			}
		}

		public ILEmitter ldloca_s(LocalBuilder local)
		{
			lock (emit)
			{
				il.Emit(OpCodes.Ldloca_S, local);
				return this;
			}
		}

		public ILEmitter ldloc_s(int idx)
		{
			lock (emit)
			{
				il.Emit(OpCodes.Ldloc_S, idx);
				return this;
			}
		}

		public ILEmitter ldloc_s(LocalBuilder local)
		{
			lock (emit)
			{
				il.Emit(OpCodes.Ldloc_S, local);
				return this;
			}
		}

		public ILEmitter ldloca(int idx)
		{
			lock (emit)
			{
				il.Emit(OpCodes.Ldloca, idx);
				return this;
			}
		}

		public ILEmitter ldloca(LocalBuilder local)
		{
			lock (emit)
			{
				il.Emit(OpCodes.Ldloca, local);
				return this;
			}
		}

		public ILEmitter ldloc(int idx)
		{
			lock (emit)
			{
				il.Emit(OpCodes.Ldloc, idx);
				return this;
			}
		}

		public ILEmitter ldloc(LocalBuilder local)
		{
			lock (emit)
			{
				il.Emit(OpCodes.Ldloc, local);
				return this;
			}
		}

		public ILEmitter initobj(Type type)
		{
			lock (emit)
			{
				il.Emit(OpCodes.Initobj, type);
				return this;
			}
		}

		public ILEmitter newobj(ConstructorInfo ctor)
		{
			lock (emit)
			{
				il.Emit(OpCodes.Newobj, ctor);
				return this;
			}
		}

		public ILEmitter Throw()
		{
			lock (emit)
			{
				il.Emit(OpCodes.Throw);
				return this;
			}
		}

		public ILEmitter throw_new(Type type)
		{
			ConstructorInfo constructor = type.GetConstructor(Type.EmptyTypes);
			newobj(constructor).Throw();
			return this;
		}

		public ILEmitter stelem_ref()
		{
			lock (emit)
			{
				il.Emit(OpCodes.Stelem_Ref);
				return this;
			}
		}

		public ILEmitter ldelem_ref()
		{
			lock (emit)
			{
				il.Emit(OpCodes.Ldelem_Ref);
				return this;
			}
		}

		public ILEmitter ldlen()
		{
			lock (emit)
			{
				il.Emit(OpCodes.Ldlen);
				return this;
			}
		}

		public ILEmitter stloc(int idx)
		{
			lock (emit)
			{
				il.Emit(OpCodes.Stloc, idx);
				return this;
			}
		}

		public ILEmitter stloc_s(int idx)
		{
			lock (emit)
			{
				il.Emit(OpCodes.Stloc_S, idx);
				return this;
			}
		}

		public ILEmitter stloc(LocalBuilder local)
		{
			lock (emit)
			{
				il.Emit(OpCodes.Stloc, local);
				return this;
			}
		}

		public ILEmitter stloc_s(LocalBuilder local)
		{
			lock (emit)
			{
				il.Emit(OpCodes.Stloc_S, local);
				return this;
			}
		}

		public ILEmitter stloc0()
		{
			lock (emit)
			{
				il.Emit(OpCodes.Stloc_0);
				return this;
			}
		}

		public ILEmitter stloc1()
		{
			lock (emit)
			{
				il.Emit(OpCodes.Stloc_1);
				return this;
			}
		}

		public ILEmitter mark(Label label)
		{
			lock (emit)
			{
				il.MarkLabel(label);
				return this;
			}
		}

		public ILEmitter ldfld(FieldInfo field)
		{
			lock (emit)
			{
				il.Emit(OpCodes.Ldfld, field);
				return this;
			}
		}

		public ILEmitter ldsfld(FieldInfo field)
		{
			lock (emit)
			{
				il.Emit(OpCodes.Ldsfld, field);
				return this;
			}
		}

		public ILEmitter lodfld(FieldInfo field)
		{
			if (field.IsStatic)
			{
				ldsfld(field);
			}
			else
			{
				ldfld(field);
			}
			return this;
		}

		public ILEmitter ifvaluetype_box(Type type)
		{
			lock (emit)
			{
				if (type.IsValueType)
				{
					il.Emit(OpCodes.Box, type);
				}
				return this;
			}
		}

		public ILEmitter stfld(FieldInfo field)
		{
			lock (emit)
			{
				il.Emit(OpCodes.Stfld, field);
				return this;
			}
		}

		public ILEmitter stsfld(FieldInfo field)
		{
			lock (emit)
			{
				il.Emit(OpCodes.Stsfld, field);
				return this;
			}
		}

		public ILEmitter setfld(FieldInfo field)
		{
			if (field.IsStatic)
			{
				stsfld(field);
			}
			else
			{
				stfld(field);
			}
			return this;
		}

		public ILEmitter unboxorcast(Type type)
		{
			if (type.IsValueType)
			{
				unbox(type);
			}
			else
			{
				cast(type);
			}
			return this;
		}

		public ILEmitter callorvirt(MethodInfo method)
		{
			lock (emit)
			{
				if (method.IsVirtual)
				{
					il.Emit(OpCodes.Callvirt, method);
				}
				else
				{
					il.Emit(OpCodes.Call, method);
				}
				return this;
			}
		}

		public ILEmitter stind_ref()
		{
			lock (emit)
			{
				il.Emit(OpCodes.Stind_Ref);
				return this;
			}
		}

		public ILEmitter ldind_ref()
		{
			lock (emit)
			{
				il.Emit(OpCodes.Ldind_Ref);
				return this;
			}
		}

		public LocalBuilder declocal(Type type)
		{
			return il.DeclareLocal(type);
		}

		public Label deflabel()
		{
			return il.DefineLabel();
		}

		public ILEmitter ifclass_ldarg_else_ldarga(int idx, Type type)
		{
			lock (emit)
			{
				if (type.IsValueType)
				{
					emit.ldarga(idx);
				}
				else
				{
					emit.ldarg(idx);
				}
				return this;
			}
		}

		public ILEmitter ifclass_ldloc_else_ldloca(int idx, Type type)
		{
			lock (emit)
			{
				if (type.IsValueType)
				{
					emit.ldloca(idx);
				}
				else
				{
					emit.ldloc(idx);
				}
				return this;
			}
		}

		public ILEmitter perform(Action<ILEmitter, MemberInfo> action, MemberInfo member)
		{
			action(this, member);
			return this;
		}

		public ILEmitter ifbyref_ldloca_else_ldloc(LocalBuilder local, Type type)
		{
			if (type.IsByRef)
			{
				ldloca(local);
			}
			else
			{
				ldloc(local);
			}
			return this;
		}
	}

	private static ILEmitter emit = new ILEmitter();

	private static Dictionary<int, Delegate> cache = new Dictionary<int, Delegate>();

	private const string kCtorInvokerName = "CI<>";

	private const string kMethodCallerName = "MC<>";

	private const string kFieldSetterName = "FS<>";

	private const string kFieldGetterName = "FG<>";

	private const string kPropertySetterName = "PS<>";

	private const string kPropertyGetterName = "PG<>";

	public static Ctor<T> DelegateForCtor<T>(this Type type, params Type[] paramTypes)
	{
		lock (emit)
		{
			int num = "CI<>".GetHashCode() ^ type.GetHashCode();
			for (int i = 0; i < paramTypes.Length; i++)
			{
				num ^= paramTypes[i].GetHashCode();
			}
			if (cache.TryGetValue(num, out var value))
			{
				return (Ctor<T>)value;
			}
			DynamicMethod dynamicMethod = new DynamicMethod("CI<>", typeof(T), new Type[1] { typeof(object[]) });
			emit.il = dynamicMethod.GetILGenerator();
			GenCtor<T>(type, paramTypes);
			value = dynamicMethod.CreateDelegate(typeof(Ctor<T>));
			cache[num] = value;
			return (Ctor<T>)value;
		}
	}

	public static Ctor<object> DelegateForCtor(this Type type, params Type[] ctorParamTypes)
	{
		return type.DelegateForCtor<object>(ctorParamTypes);
	}

	public static Getter<TTarget, TReturn> DelegateForGet<TTarget, TReturn>(this PropertyInfo property)
	{
		if (!property.CanRead)
		{
			throw new InvalidOperationException("Property is not readable " + property.Name);
		}
		int key = GetKey<TTarget, TReturn>(property, "PG<>");
		if (cache.TryGetValue(key, out var value))
		{
			return (Getter<TTarget, TReturn>)value;
		}
		return GenDelegateForMember<Getter<TTarget, TReturn>, PropertyInfo>(property, key, "PG<>", GenPropertyGetter<TTarget>, typeof(TReturn), new Type[1] { typeof(TTarget) });
	}

	public static Getter<object, object> DelegateForGet(this PropertyInfo property)
	{
		return property.DelegateForGet<object, object>();
	}

	public static Setter<TTarget, TValue> DelegateForSet<TTarget, TValue>(this PropertyInfo property)
	{
		if (!property.CanWrite)
		{
			throw new InvalidOperationException("Property is not writable " + property.Name);
		}
		int key = GetKey<TTarget, TValue>(property, "PS<>");
		if (cache.TryGetValue(key, out var value))
		{
			return (Setter<TTarget, TValue>)value;
		}
		return GenDelegateForMember<Setter<TTarget, TValue>, PropertyInfo>(property, key, "PS<>", GenPropertySetter<TTarget>, typeof(void), new Type[2]
		{
			typeof(TTarget).MakeByRefType(),
			typeof(TValue)
		});
	}

	public static Setter<object, object> DelegateForSet(this PropertyInfo property)
	{
		return property.DelegateForSet<object, object>();
	}

	public static Getter<TTarget, TReturn> DelegateForGet<TTarget, TReturn>(this FieldInfo field)
	{
		int key = GetKey<TTarget, TReturn>(field, "FG<>");
		if (cache.TryGetValue(key, out var value))
		{
			return (Getter<TTarget, TReturn>)value;
		}
		return GenDelegateForMember<Getter<TTarget, TReturn>, FieldInfo>(field, key, "FG<>", GenFieldGetter<TTarget>, typeof(TReturn), new Type[1] { typeof(TTarget) });
	}

	public static Getter<object, object> DelegateForGet(this FieldInfo field)
	{
		return field.DelegateForGet<object, object>();
	}

	public static Setter<TTarget, TValue> DelegateForSet<TTarget, TValue>(this FieldInfo field)
	{
		int key = GetKey<TTarget, TValue>(field, "FS<>");
		if (cache.TryGetValue(key, out var value))
		{
			return (Setter<TTarget, TValue>)value;
		}
		return GenDelegateForMember<Setter<TTarget, TValue>, FieldInfo>(field, key, "FS<>", GenFieldSetter<TTarget>, typeof(void), new Type[2]
		{
			typeof(TTarget).MakeByRefType(),
			typeof(TValue)
		});
	}

	public static Setter<object, object> DelegateForSet(this FieldInfo field)
	{
		return field.DelegateForSet<object, object>();
	}

	public static Caller<TTarget, TReturn> DelegateForCall<TTarget, TReturn>(this MethodInfo method)
	{
		int key = GetKey<TTarget, TReturn>(method, "MC<>");
		if (cache.TryGetValue(key, out var value))
		{
			return (Caller<TTarget, TReturn>)value;
		}
		return GenDelegateForMember<Caller<TTarget, TReturn>, MethodInfo>(method, key, "MC<>", GenMethodInvocation<TTarget>, typeof(TReturn), new Type[2]
		{
			typeof(TTarget),
			typeof(object[])
		});
	}

	public static Caller<object, object> DelegateForCall(this MethodInfo method)
	{
		return method.DelegateForCall<object, object>();
	}

	public static void SafeInvoke<TTarget, TValue>(this Caller<TTarget, TValue> caller, TTarget target, params object[] args)
	{
		caller?.Invoke(target, args);
	}

	public static void SafeInvoke<TTarget, TValue>(this Setter<TTarget, TValue> setter, ref TTarget target, TValue value)
	{
		setter?.Invoke(ref target, value);
	}

	public static TReturn SafeInvoke<TTarget, TReturn>(this Getter<TTarget, TReturn> getter, TTarget target)
	{
		if (getter != null)
		{
			return getter(target);
		}
		return default(TReturn);
	}

	public static void GenDebugAssembly(string name, FieldInfo field, PropertyInfo property, MethodInfo method, Type targetType, Type[] ctorParamTypes)
	{
		GenDebugAssembly<object>(name, field, property, method, targetType, ctorParamTypes);
	}

	public static void GenDebugAssembly<TTarget>(string name, FieldInfo field, PropertyInfo property, MethodInfo method, Type targetType, Type[] ctorParamTypes)
	{
		AssemblyName name2 = new AssemblyName("Asm");
		AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(name2, AssemblyBuilderAccess.RunAndSave);
		ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("Mod", name);
		TypeBuilder typeBuilder = moduleBuilder.DefineType("Test", TypeAttributes.Public);
		bool flag = typeof(TTarget) == typeof(object);
		Func<string, Type, Type[], ILGenerator> func = delegate(string methodName, Type returnType, Type[] parameterTypes)
		{
			MethodBuilder methodBuilder = typeBuilder.DefineMethod(methodName, MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig, CallingConventions.Standard, returnType, parameterTypes);
			return methodBuilder.GetILGenerator();
		};
		if (field != null)
		{
			lock (emit)
			{
				Type type = ((!flag) ? field.FieldType : typeof(object));
				emit.il = func("Setter", typeof(void), new Type[2]
				{
					typeof(TTarget).MakeByRefType(),
					type
				});
				GenFieldSetter<TTarget>(field);
				emit.il = func("Getter", type, new Type[1] { typeof(TTarget) });
				GenFieldGetter<TTarget>(field);
			}
		}
		if (property != null)
		{
			lock (emit)
			{
				Type type2 = ((!flag) ? property.PropertyType : typeof(object));
				emit.il = func("PropertySetter", typeof(void), new Type[2]
				{
					typeof(TTarget).MakeByRefType(),
					type2
				});
				GenPropertySetter<TTarget>(property);
				emit.il = func("PropertyGetter", type2, new Type[1] { typeof(TTarget) });
				GenPropertyGetter<TTarget>(property);
			}
		}
		if (method != null)
		{
			lock (emit)
			{
				Type arg = ((!flag && method.ReturnType != typeof(void)) ? method.ReturnType : typeof(object));
				emit.il = func("Caller", arg, new Type[2]
				{
					typeof(TTarget),
					typeof(object[])
				});
				GenMethodInvocation<TTarget>(method);
			}
		}
		if (targetType != null)
		{
			lock (emit)
			{
				emit.il = func("Ctor", typeof(TTarget), new Type[1] { typeof(object[]) });
				GenCtor<TTarget>(targetType, ctorParamTypes);
			}
		}
		typeBuilder.CreateType();
		assemblyBuilder.Save(name);
	}

	private static int GetKey<T, R>(MemberInfo member, string dynMethodName)
	{
		return member.GetHashCode() ^ dynMethodName.GetHashCode() ^ typeof(T).GetHashCode() ^ typeof(R).GetHashCode();
	}

	private static TDelegate GenDelegateForMember<TDelegate, TMember>(TMember member, int key, string dynMethodName, Action<TMember> generator, Type returnType, params Type[] paramTypes) where TDelegate : class where TMember : MemberInfo
	{
		lock (emit)
		{
			DynamicMethod dynamicMethod = new DynamicMethod(dynMethodName, returnType, paramTypes, restrictedSkipVisibility: true);
			emit.il = dynamicMethod.GetILGenerator();
			generator(member);
			Delegate obj = dynamicMethod.CreateDelegate(typeof(TDelegate));
			cache[key] = obj;
			return (TDelegate)(object)obj;
		}
	}

	private static void GenCtor<T>(Type type, Type[] paramTypes)
	{
		lock (emit)
		{
			Type type2 = ((typeof(T) != typeof(object)) ? typeof(T) : type);
			ConstructorInfo constructor = type2.GetConstructor(paramTypes);
			if (constructor == null)
			{
				throw new Exception("Generating constructor: " + ((paramTypes.Length != 0) ? ("No constructor found that matches the following parameter types: " + string.Join(",", paramTypes.Select((Type x) => x.Name).ToArray())) : "No empty constructor found!"));
			}
			if (type2.IsValueType && paramTypes.Length == 0)
			{
				LocalBuilder local = emit.declocal(type2);
				emit.ldloca(local).initobj(type2).ldloc(0);
			}
			else
			{
				int num = 0;
				for (int num2 = paramTypes.Length; num < num2; num++)
				{
					emit.ldarg0().ldc_i4(num).ldelem_ref()
						.unbox_any(paramTypes[num]);
				}
				emit.newobj(constructor);
			}
			if (typeof(T) == typeof(object) && type2.IsValueType)
			{
				emit.box(type2);
			}
			emit.ret();
		}
	}

	private static void GenMethodInvocation<TTarget>(MethodInfo method)
	{
		lock (emit)
		{
			bool flag = typeof(TTarget) == typeof(object);
			if (!method.IsStatic)
			{
				Type type = ((!flag) ? typeof(TTarget) : method.DeclaringType);
				emit.declocal(type);
				emit.ldarg0();
				if (flag)
				{
					emit.unbox_any(type);
				}
				emit.stloc0().ifclass_ldloc_else_ldloca(0, type);
			}
			ParameterInfo[] parameters = method.GetParameters();
			int i = 0;
			for (int num = parameters.Length; i < num; i++)
			{
				emit.ldarg1().ldc_i4(i).ldelem_ref();
				ParameterInfo parameterInfo = parameters[i];
				Type type2 = parameterInfo.ParameterType;
				if (type2.IsByRef)
				{
					type2 = type2.GetElementType();
				}
				LocalBuilder local = emit.declocal(type2);
				emit.unbox_any(type2).stloc(local).ifbyref_ldloca_else_ldloc(local, parameterInfo.ParameterType);
			}
			emit.callorvirt(method);
			int num2 = ((!method.IsStatic) ? 1 : 0);
			for (int j = 0; j < parameters.Length; j++)
			{
				Type parameterType = parameters[j].ParameterType;
				if (parameterType.IsByRef)
				{
					Type elementType = parameterType.GetElementType();
					emit.ldarg1().ldc_i4(j).ldloc(j + num2);
					if (elementType.IsValueType)
					{
						emit.box(elementType);
					}
					emit.stelem_ref();
				}
			}
			if (method.ReturnType == typeof(void))
			{
				emit.ldnull();
			}
			else if (flag)
			{
				emit.ifvaluetype_box(method.ReturnType);
			}
			emit.ret();
		}
	}

	private static void GenFieldGetter<TTarget>(FieldInfo field)
	{
		GenMemberGetter<TTarget>(field, field.FieldType, field.IsStatic, delegate(ILEmitter e, MemberInfo f)
		{
			e.lodfld((FieldInfo)f);
		});
	}

	private static void GenPropertyGetter<TTarget>(PropertyInfo property)
	{
		GenMemberGetter<TTarget>(property, property.PropertyType, property.GetGetMethod(nonPublic: true).IsStatic, delegate(ILEmitter e, MemberInfo p)
		{
			e.callorvirt(((PropertyInfo)p).GetGetMethod(nonPublic: true));
		});
	}

	private static void GenMemberGetter<TTarget>(MemberInfo member, Type memberType, bool isStatic, Action<ILEmitter, MemberInfo> get)
	{
		lock (emit)
		{
			if (typeof(TTarget) == typeof(object))
			{
				if (!isStatic)
				{
					emit.ldarg0().unboxorcast(member.DeclaringType);
				}
				emit.perform(get, member).ifvaluetype_box(memberType);
			}
			else
			{
				if (!isStatic)
				{
					emit.ifclass_ldarg_else_ldarga(0, typeof(TTarget));
				}
				emit.perform(get, member);
			}
			emit.ret();
		}
	}

	private static void GenFieldSetter<TTarget>(FieldInfo field)
	{
		GenMemberSetter<TTarget>(field, field.FieldType, field.IsStatic, delegate(ILEmitter e, MemberInfo f)
		{
			e.setfld((FieldInfo)f);
		});
	}

	private static void GenPropertySetter<TTarget>(PropertyInfo property)
	{
		GenMemberSetter<TTarget>(property, property.PropertyType, property.GetSetMethod(nonPublic: true).IsStatic, delegate(ILEmitter e, MemberInfo p)
		{
			e.callorvirt(((PropertyInfo)p).GetSetMethod(nonPublic: true));
		});
	}

	private static void GenMemberSetter<TTarget>(MemberInfo member, Type memberType, bool isStatic, Action<ILEmitter, MemberInfo> set)
	{
		lock (emit)
		{
			Type typeFromHandle = typeof(TTarget);
			bool flag = typeFromHandle != typeof(object);
			if (isStatic)
			{
				emit.ldarg1();
				if (!flag)
				{
					emit.unbox_any(memberType);
				}
				emit.perform(set, member).ret();
			}
			else if (flag)
			{
				emit.ldarg0().ifclass_ldind_ref(typeFromHandle).ldarg1()
					.perform(set, member)
					.ret();
			}
			else
			{
				typeFromHandle = member.DeclaringType;
				if (!typeFromHandle.IsValueType)
				{
					emit.ldarg0().ldind_ref().cast(typeFromHandle)
						.ldarg1()
						.unbox_any(memberType)
						.perform(set, member)
						.ret();
					return;
				}
				emit.declocal(typeFromHandle);
				emit.ldarg0().ldind_ref().unbox_any(typeFromHandle)
					.stloc0()
					.ldloca(0)
					.ldarg1()
					.unbox_any(memberType)
					.perform(set, member)
					.ldarg0()
					.ldloc0()
					.box(typeFromHandle)
					.stind_ref()
					.ret();
			}
		}
	}
}
