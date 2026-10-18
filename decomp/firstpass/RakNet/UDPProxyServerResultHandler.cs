using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace RakNet;

public class UDPProxyServerResultHandler : IDisposable
{
	public delegate void SwigDelegateUDPProxyServerResultHandler_0(IntPtr usedPassword, IntPtr proxyServerPlugin);

	public delegate void SwigDelegateUDPProxyServerResultHandler_1(IntPtr usedPassword, IntPtr proxyServerPlugin);

	public delegate void SwigDelegateUDPProxyServerResultHandler_2(IntPtr usedPassword, IntPtr proxyServerPlugin);

	public delegate void SwigDelegateUDPProxyServerResultHandler_3(IntPtr usedPassword, IntPtr proxyServerPlugin);

	private HandleRef swigCPtr;

	protected bool swigCMemOwn;

	private SwigDelegateUDPProxyServerResultHandler_0 swigDelegate0;

	private SwigDelegateUDPProxyServerResultHandler_1 swigDelegate1;

	private SwigDelegateUDPProxyServerResultHandler_2 swigDelegate2;

	private SwigDelegateUDPProxyServerResultHandler_3 swigDelegate3;

	private static Type[] swigMethodTypes0 = new Type[2]
	{
		typeof(RakString),
		typeof(UDPProxyServer)
	};

	private static Type[] swigMethodTypes1 = new Type[2]
	{
		typeof(RakString),
		typeof(UDPProxyServer)
	};

	private static Type[] swigMethodTypes2 = new Type[2]
	{
		typeof(RakString),
		typeof(UDPProxyServer)
	};

	private static Type[] swigMethodTypes3 = new Type[2]
	{
		typeof(RakString),
		typeof(UDPProxyServer)
	};

	internal UDPProxyServerResultHandler(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = new HandleRef(this, cPtr);
	}

	public UDPProxyServerResultHandler()
		: this(RakNetPINVOKE.new_UDPProxyServerResultHandler(), cMemoryOwn: true)
	{
		SwigDirectorConnect();
	}

	internal static HandleRef getCPtr(UDPProxyServerResultHandler obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	~UDPProxyServerResultHandler()
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
					RakNetPINVOKE.delete_UDPProxyServerResultHandler(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			GC.SuppressFinalize(this);
		}
	}

	public virtual void OnLoginSuccess(RakString usedPassword, UDPProxyServer proxyServerPlugin)
	{
		RakNetPINVOKE.UDPProxyServerResultHandler_OnLoginSuccess(swigCPtr, RakString.getCPtr(usedPassword), UDPProxyServer.getCPtr(proxyServerPlugin));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public virtual void OnAlreadyLoggedIn(RakString usedPassword, UDPProxyServer proxyServerPlugin)
	{
		RakNetPINVOKE.UDPProxyServerResultHandler_OnAlreadyLoggedIn(swigCPtr, RakString.getCPtr(usedPassword), UDPProxyServer.getCPtr(proxyServerPlugin));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public virtual void OnNoPasswordSet(RakString usedPassword, UDPProxyServer proxyServerPlugin)
	{
		RakNetPINVOKE.UDPProxyServerResultHandler_OnNoPasswordSet(swigCPtr, RakString.getCPtr(usedPassword), UDPProxyServer.getCPtr(proxyServerPlugin));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public virtual void OnWrongPassword(RakString usedPassword, UDPProxyServer proxyServerPlugin)
	{
		RakNetPINVOKE.UDPProxyServerResultHandler_OnWrongPassword(swigCPtr, RakString.getCPtr(usedPassword), UDPProxyServer.getCPtr(proxyServerPlugin));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	private void SwigDirectorConnect()
	{
		if (SwigDerivedClassHasMethod("OnLoginSuccess", swigMethodTypes0))
		{
			swigDelegate0 = SwigDirectorOnLoginSuccess;
		}
		if (SwigDerivedClassHasMethod("OnAlreadyLoggedIn", swigMethodTypes1))
		{
			swigDelegate1 = SwigDirectorOnAlreadyLoggedIn;
		}
		if (SwigDerivedClassHasMethod("OnNoPasswordSet", swigMethodTypes2))
		{
			swigDelegate2 = SwigDirectorOnNoPasswordSet;
		}
		if (SwigDerivedClassHasMethod("OnWrongPassword", swigMethodTypes3))
		{
			swigDelegate3 = SwigDirectorOnWrongPassword;
		}
		RakNetPINVOKE.UDPProxyServerResultHandler_director_connect(swigCPtr, swigDelegate0, swigDelegate1, swigDelegate2, swigDelegate3);
	}

	private bool SwigDerivedClassHasMethod(string methodName, Type[] methodTypes)
	{
		MethodInfo method = GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, methodTypes, null);
		return method.DeclaringType.IsSubclassOf(typeof(UDPProxyServerResultHandler));
	}

	private void SwigDirectorOnLoginSuccess(IntPtr usedPassword, IntPtr proxyServerPlugin)
	{
		OnLoginSuccess(new RakString(usedPassword, cMemoryOwn: false), (!(proxyServerPlugin == IntPtr.Zero)) ? new UDPProxyServer(proxyServerPlugin, cMemoryOwn: false) : null);
	}

	private void SwigDirectorOnAlreadyLoggedIn(IntPtr usedPassword, IntPtr proxyServerPlugin)
	{
		OnAlreadyLoggedIn(new RakString(usedPassword, cMemoryOwn: false), (!(proxyServerPlugin == IntPtr.Zero)) ? new UDPProxyServer(proxyServerPlugin, cMemoryOwn: false) : null);
	}

	private void SwigDirectorOnNoPasswordSet(IntPtr usedPassword, IntPtr proxyServerPlugin)
	{
		OnNoPasswordSet(new RakString(usedPassword, cMemoryOwn: false), (!(proxyServerPlugin == IntPtr.Zero)) ? new UDPProxyServer(proxyServerPlugin, cMemoryOwn: false) : null);
	}

	private void SwigDirectorOnWrongPassword(IntPtr usedPassword, IntPtr proxyServerPlugin)
	{
		OnWrongPassword(new RakString(usedPassword, cMemoryOwn: false), (!(proxyServerPlugin == IntPtr.Zero)) ? new UDPProxyServer(proxyServerPlugin, cMemoryOwn: false) : null);
	}
}
