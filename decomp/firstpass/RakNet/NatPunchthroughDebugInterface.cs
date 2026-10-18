using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace RakNet;

public class NatPunchthroughDebugInterface : IDisposable
{
	public delegate void SwigDelegateNatPunchthroughDebugInterface_0(string msg);

	private HandleRef swigCPtr;

	protected bool swigCMemOwn;

	private SwigDelegateNatPunchthroughDebugInterface_0 swigDelegate0;

	private static Type[] swigMethodTypes0 = new Type[1] { typeof(string) };

	internal NatPunchthroughDebugInterface(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = new HandleRef(this, cPtr);
	}

	public NatPunchthroughDebugInterface()
		: this(RakNetPINVOKE.new_NatPunchthroughDebugInterface(), cMemoryOwn: true)
	{
		SwigDirectorConnect();
	}

	internal static HandleRef getCPtr(NatPunchthroughDebugInterface obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	~NatPunchthroughDebugInterface()
	{
		Dispose();
	}

	public virtual void Dispose()
	{
		lock (this)
		{
			if (swigCPtr.Handle != IntPtr.Zero)
			{
				if (swigCMemOwn)
				{
					swigCMemOwn = false;
					RakNetPINVOKE.delete_NatPunchthroughDebugInterface(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			GC.SuppressFinalize(this);
		}
	}

	public virtual void OnClientMessage(string msg)
	{
		RakNetPINVOKE.NatPunchthroughDebugInterface_OnClientMessage(swigCPtr, msg);
	}

	private void SwigDirectorConnect()
	{
		if (SwigDerivedClassHasMethod("OnClientMessage", swigMethodTypes0))
		{
			swigDelegate0 = SwigDirectorOnClientMessage;
		}
		RakNetPINVOKE.NatPunchthroughDebugInterface_director_connect(swigCPtr, swigDelegate0);
	}

	private bool SwigDerivedClassHasMethod(string methodName, Type[] methodTypes)
	{
		MethodInfo method = GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, methodTypes, null);
		return method.DeclaringType.IsSubclassOf(typeof(NatPunchthroughDebugInterface));
	}

	private void SwigDirectorOnClientMessage(string msg)
	{
		OnClientMessage(msg);
	}
}
