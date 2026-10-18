using System;
using System.Runtime.InteropServices;

namespace RakNet;

public class PunchthroughConfiguration : IDisposable
{
	private HandleRef swigCPtr;

	protected bool swigCMemOwn;

	public ulong TIME_BETWEEN_PUNCH_ATTEMPTS_INTERNAL
	{
		get
		{
			return RakNetPINVOKE.PunchthroughConfiguration_TIME_BETWEEN_PUNCH_ATTEMPTS_INTERNAL_get(swigCPtr);
		}
		set
		{
			RakNetPINVOKE.PunchthroughConfiguration_TIME_BETWEEN_PUNCH_ATTEMPTS_INTERNAL_set(swigCPtr, value);
		}
	}

	public ulong TIME_BETWEEN_PUNCH_ATTEMPTS_EXTERNAL
	{
		get
		{
			return RakNetPINVOKE.PunchthroughConfiguration_TIME_BETWEEN_PUNCH_ATTEMPTS_EXTERNAL_get(swigCPtr);
		}
		set
		{
			RakNetPINVOKE.PunchthroughConfiguration_TIME_BETWEEN_PUNCH_ATTEMPTS_EXTERNAL_set(swigCPtr, value);
		}
	}

	public int UDP_SENDS_PER_PORT_INTERNAL
	{
		get
		{
			return RakNetPINVOKE.PunchthroughConfiguration_UDP_SENDS_PER_PORT_INTERNAL_get(swigCPtr);
		}
		set
		{
			RakNetPINVOKE.PunchthroughConfiguration_UDP_SENDS_PER_PORT_INTERNAL_set(swigCPtr, value);
		}
	}

	public int UDP_SENDS_PER_PORT_EXTERNAL
	{
		get
		{
			return RakNetPINVOKE.PunchthroughConfiguration_UDP_SENDS_PER_PORT_EXTERNAL_get(swigCPtr);
		}
		set
		{
			RakNetPINVOKE.PunchthroughConfiguration_UDP_SENDS_PER_PORT_EXTERNAL_set(swigCPtr, value);
		}
	}

	public int INTERNAL_IP_WAIT_AFTER_ATTEMPTS
	{
		get
		{
			return RakNetPINVOKE.PunchthroughConfiguration_INTERNAL_IP_WAIT_AFTER_ATTEMPTS_get(swigCPtr);
		}
		set
		{
			RakNetPINVOKE.PunchthroughConfiguration_INTERNAL_IP_WAIT_AFTER_ATTEMPTS_set(swigCPtr, value);
		}
	}

	public int MAX_PREDICTIVE_PORT_RANGE
	{
		get
		{
			return RakNetPINVOKE.PunchthroughConfiguration_MAX_PREDICTIVE_PORT_RANGE_get(swigCPtr);
		}
		set
		{
			RakNetPINVOKE.PunchthroughConfiguration_MAX_PREDICTIVE_PORT_RANGE_set(swigCPtr, value);
		}
	}

	public int EXTERNAL_IP_WAIT_AFTER_FIRST_TTL
	{
		get
		{
			return RakNetPINVOKE.PunchthroughConfiguration_EXTERNAL_IP_WAIT_AFTER_FIRST_TTL_get(swigCPtr);
		}
		set
		{
			RakNetPINVOKE.PunchthroughConfiguration_EXTERNAL_IP_WAIT_AFTER_FIRST_TTL_set(swigCPtr, value);
		}
	}

	public int EXTERNAL_IP_WAIT_BETWEEN_PORTS
	{
		get
		{
			return RakNetPINVOKE.PunchthroughConfiguration_EXTERNAL_IP_WAIT_BETWEEN_PORTS_get(swigCPtr);
		}
		set
		{
			RakNetPINVOKE.PunchthroughConfiguration_EXTERNAL_IP_WAIT_BETWEEN_PORTS_set(swigCPtr, value);
		}
	}

	public int EXTERNAL_IP_WAIT_AFTER_ALL_ATTEMPTS
	{
		get
		{
			return RakNetPINVOKE.PunchthroughConfiguration_EXTERNAL_IP_WAIT_AFTER_ALL_ATTEMPTS_get(swigCPtr);
		}
		set
		{
			RakNetPINVOKE.PunchthroughConfiguration_EXTERNAL_IP_WAIT_AFTER_ALL_ATTEMPTS_set(swigCPtr, value);
		}
	}

	public int MAXIMUM_NUMBER_OF_INTERNAL_IDS_TO_CHECK
	{
		get
		{
			return RakNetPINVOKE.PunchthroughConfiguration_MAXIMUM_NUMBER_OF_INTERNAL_IDS_TO_CHECK_get(swigCPtr);
		}
		set
		{
			RakNetPINVOKE.PunchthroughConfiguration_MAXIMUM_NUMBER_OF_INTERNAL_IDS_TO_CHECK_set(swigCPtr, value);
		}
	}

	public bool retryOnFailure
	{
		get
		{
			return RakNetPINVOKE.PunchthroughConfiguration_retryOnFailure_get(swigCPtr);
		}
		set
		{
			RakNetPINVOKE.PunchthroughConfiguration_retryOnFailure_set(swigCPtr, value);
		}
	}

	internal PunchthroughConfiguration(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = new HandleRef(this, cPtr);
	}

	public PunchthroughConfiguration()
		: this(RakNetPINVOKE.new_PunchthroughConfiguration(), cMemoryOwn: true)
	{
	}

	internal static HandleRef getCPtr(PunchthroughConfiguration obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	~PunchthroughConfiguration()
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
					RakNetPINVOKE.delete_PunchthroughConfiguration(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			GC.SuppressFinalize(this);
		}
	}
}
