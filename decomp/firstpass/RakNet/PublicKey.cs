using System;
using System.Runtime.InteropServices;

namespace RakNet;

public class PublicKey : IDisposable
{
	private HandleRef swigCPtr;

	protected bool swigCMemOwn;

	public PublicKeyMode publicKeyMode
	{
		get
		{
			return (PublicKeyMode)RakNetPINVOKE.PublicKey_publicKeyMode_get(swigCPtr);
		}
		set
		{
			RakNetPINVOKE.PublicKey_publicKeyMode_set(swigCPtr, (int)value);
		}
	}

	public string remoteServerPublicKey
	{
		get
		{
			return RakNetPINVOKE.PublicKey_remoteServerPublicKey_get(swigCPtr);
		}
		set
		{
			RakNetPINVOKE.PublicKey_remoteServerPublicKey_set(swigCPtr, value);
		}
	}

	public string myPublicKey
	{
		get
		{
			return RakNetPINVOKE.PublicKey_myPublicKey_get(swigCPtr);
		}
		set
		{
			RakNetPINVOKE.PublicKey_myPublicKey_set(swigCPtr, value);
		}
	}

	public string myPrivateKey
	{
		get
		{
			return RakNetPINVOKE.PublicKey_myPrivateKey_get(swigCPtr);
		}
		set
		{
			RakNetPINVOKE.PublicKey_myPrivateKey_set(swigCPtr, value);
		}
	}

	internal PublicKey(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = new HandleRef(this, cPtr);
	}

	public PublicKey()
		: this(RakNetPINVOKE.new_PublicKey(), cMemoryOwn: true)
	{
	}

	internal static HandleRef getCPtr(PublicKey obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	~PublicKey()
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
					RakNetPINVOKE.delete_PublicKey(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			GC.SuppressFinalize(this);
		}
	}
}
