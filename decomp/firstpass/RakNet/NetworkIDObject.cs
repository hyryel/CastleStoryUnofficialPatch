using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace RakNet;

public class NetworkIDObject : IDisposable
{
	public delegate void SwigDelegateNetworkIDObject_0(IntPtr manager);

	public delegate IntPtr SwigDelegateNetworkIDObject_1();

	public delegate ulong SwigDelegateNetworkIDObject_2();

	public delegate void SwigDelegateNetworkIDObject_3(ulong id);

	private HandleRef swigCPtr;

	protected bool swigCMemOwn;

	private NetworkIDManager oldManager;

	private SwigDelegateNetworkIDObject_0 swigDelegate0;

	private SwigDelegateNetworkIDObject_1 swigDelegate1;

	private SwigDelegateNetworkIDObject_2 swigDelegate2;

	private SwigDelegateNetworkIDObject_3 swigDelegate3;

	private static Type[] swigMethodTypes0 = new Type[1] { typeof(NetworkIDManager) };

	private static Type[] swigMethodTypes1 = new Type[0];

	private static Type[] swigMethodTypes2 = new Type[0];

	private static Type[] swigMethodTypes3 = new Type[1] { typeof(ulong) };

	internal NetworkIDObject(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = new HandleRef(this, cPtr);
	}

	public NetworkIDObject()
		: this(RakNetPINVOKE.new_NetworkIDObject(), cMemoryOwn: true)
	{
		SwigDirectorConnect();
	}

	internal static HandleRef getCPtr(NetworkIDObject obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	~NetworkIDObject()
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
					RakNetPINVOKE.delete_NetworkIDObject(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			GC.SuppressFinalize(this);
		}
	}

	public virtual void SetNetworkIDManager(NetworkIDManager manager)
	{
		if (oldManager != null)
		{
			oldManager.pointerDictionary.Remove(GetIntPtr());
		}
		if (manager != null)
		{
			manager.pointerDictionary.Add(GetIntPtr(), this);
			oldManager = manager;
		}
		SetNetworkIDManagerOrig(manager);
	}

	public IntPtr GetIntPtr()
	{
		return swigCPtr.Handle;
	}

	protected void SetNetworkIDManagerOrig(NetworkIDManager manager)
	{
		if (SwigDerivedClassHasMethod("SetNetworkIDManagerOrig", swigMethodTypes0))
		{
			RakNetPINVOKE.NetworkIDObject_SetNetworkIDManagerOrigSwigExplicitNetworkIDObject(swigCPtr, NetworkIDManager.getCPtr(manager));
		}
		else
		{
			RakNetPINVOKE.NetworkIDObject_SetNetworkIDManagerOrig(swigCPtr, NetworkIDManager.getCPtr(manager));
		}
	}

	public virtual NetworkIDManager GetNetworkIDManager()
	{
		IntPtr intPtr = ((!SwigDerivedClassHasMethod("GetNetworkIDManager", swigMethodTypes1)) ? RakNetPINVOKE.NetworkIDObject_GetNetworkIDManager(swigCPtr) : RakNetPINVOKE.NetworkIDObject_GetNetworkIDManagerSwigExplicitNetworkIDObject(swigCPtr));
		return (!(intPtr == IntPtr.Zero)) ? new NetworkIDManager(intPtr, cMemoryOwn: false) : null;
	}

	public virtual ulong GetNetworkID()
	{
		return (!SwigDerivedClassHasMethod("GetNetworkID", swigMethodTypes2)) ? RakNetPINVOKE.NetworkIDObject_GetNetworkID(swigCPtr) : RakNetPINVOKE.NetworkIDObject_GetNetworkIDSwigExplicitNetworkIDObject(swigCPtr);
	}

	public virtual void SetNetworkID(ulong id)
	{
		if (SwigDerivedClassHasMethod("SetNetworkID", swigMethodTypes3))
		{
			RakNetPINVOKE.NetworkIDObject_SetNetworkIDSwigExplicitNetworkIDObject(swigCPtr, id);
		}
		else
		{
			RakNetPINVOKE.NetworkIDObject_SetNetworkID(swigCPtr, id);
		}
	}

	private void SwigDirectorConnect()
	{
		if (SwigDerivedClassHasMethod("SetNetworkIDManagerOrig", swigMethodTypes0))
		{
			swigDelegate0 = SwigDirectorSetNetworkIDManagerOrig;
		}
		if (SwigDerivedClassHasMethod("GetNetworkIDManager", swigMethodTypes1))
		{
			swigDelegate1 = SwigDirectorGetNetworkIDManager;
		}
		if (SwigDerivedClassHasMethod("GetNetworkID", swigMethodTypes2))
		{
			swigDelegate2 = SwigDirectorGetNetworkID;
		}
		if (SwigDerivedClassHasMethod("SetNetworkID", swigMethodTypes3))
		{
			swigDelegate3 = SwigDirectorSetNetworkID;
		}
		RakNetPINVOKE.NetworkIDObject_director_connect(swigCPtr, swigDelegate0, swigDelegate1, swigDelegate2, swigDelegate3);
	}

	private bool SwigDerivedClassHasMethod(string methodName, Type[] methodTypes)
	{
		MethodInfo method = GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, methodTypes, null);
		return method.DeclaringType.IsSubclassOf(typeof(NetworkIDObject));
	}

	private void SwigDirectorSetNetworkIDManagerOrig(IntPtr manager)
	{
		SetNetworkIDManagerOrig((!(manager == IntPtr.Zero)) ? new NetworkIDManager(manager, cMemoryOwn: false) : null);
	}

	private IntPtr SwigDirectorGetNetworkIDManager()
	{
		return NetworkIDManager.getCPtr(GetNetworkIDManager()).Handle;
	}

	private ulong SwigDirectorGetNetworkID()
	{
		return GetNetworkID();
	}

	private void SwigDirectorSetNetworkID(ulong id)
	{
		SetNetworkID(id);
	}
}
