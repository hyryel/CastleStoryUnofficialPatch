using System;
using System.Runtime.InteropServices;

namespace RakNet;

public class RakNetStatistics : IDisposable
{
	private HandleRef swigCPtr;

	protected bool swigCMemOwn;

	private bool bytesInSendBufferIsCached;

	private bool messageInSendBufferIsCached;

	private bool runningTotalIsCached;

	private bool valueOverLastSecondIsCached;

	private double[] bytesInSendBufferCache;

	private uint[] messageInSendBufferCache;

	private ulong[] runningTotalCache;

	private ulong[] valueOverLastSecondCache;

	public ulong[] valueOverLastSecond
	{
		get
		{
			ulong[] array;
			if (!valueOverLastSecondIsCached)
			{
				IntPtr source = RakNetPINVOKE.RakNetStatistics_valueOverLastSecond_get(swigCPtr);
				int num = 7;
				if (num <= 0)
				{
					return null;
				}
				array = new ulong[num];
				long[] array2 = new long[num];
				Marshal.Copy(source, array2, 0, num);
				for (int i = 0; i < num; i++)
				{
					array[i] = (ulong)array2[i];
				}
				valueOverLastSecondCache = array;
				valueOverLastSecondIsCached = true;
			}
			else
			{
				array = valueOverLastSecondCache;
			}
			return array;
		}
		set
		{
			valueOverLastSecondCache = value;
			valueOverLastSecondIsCached = true;
			SetValueOverLastSecond(value, value.Length);
		}
	}

	public ulong[] runningTotal
	{
		get
		{
			ulong[] array;
			if (!runningTotalIsCached)
			{
				IntPtr source = RakNetPINVOKE.RakNetStatistics_runningTotal_get(swigCPtr);
				int num = 7;
				if (num <= 0)
				{
					return null;
				}
				array = new ulong[num];
				long[] array2 = new long[num];
				Marshal.Copy(source, array2, 0, num);
				for (int i = 0; i < num; i++)
				{
					array[i] = (ulong)array2[i];
				}
				runningTotalCache = array;
				runningTotalIsCached = true;
			}
			else
			{
				array = runningTotalCache;
			}
			return array;
		}
		set
		{
			runningTotalCache = value;
			runningTotalIsCached = true;
			SetRunningTotal(value, value.Length);
		}
	}

	public ulong connectionStartTime
	{
		get
		{
			return RakNetPINVOKE.RakNetStatistics_connectionStartTime_get(swigCPtr);
		}
		set
		{
			RakNetPINVOKE.RakNetStatistics_connectionStartTime_set(swigCPtr, value);
		}
	}

	public bool isLimitedByCongestionControl
	{
		get
		{
			return RakNetPINVOKE.RakNetStatistics_isLimitedByCongestionControl_get(swigCPtr);
		}
		set
		{
			RakNetPINVOKE.RakNetStatistics_isLimitedByCongestionControl_set(swigCPtr, value);
		}
	}

	public ulong BPSLimitByCongestionControl
	{
		get
		{
			return RakNetPINVOKE.RakNetStatistics_BPSLimitByCongestionControl_get(swigCPtr);
		}
		set
		{
			RakNetPINVOKE.RakNetStatistics_BPSLimitByCongestionControl_set(swigCPtr, value);
		}
	}

	public bool isLimitedByOutgoingBandwidthLimit
	{
		get
		{
			return RakNetPINVOKE.RakNetStatistics_isLimitedByOutgoingBandwidthLimit_get(swigCPtr);
		}
		set
		{
			RakNetPINVOKE.RakNetStatistics_isLimitedByOutgoingBandwidthLimit_set(swigCPtr, value);
		}
	}

	public ulong BPSLimitByOutgoingBandwidthLimit
	{
		get
		{
			return RakNetPINVOKE.RakNetStatistics_BPSLimitByOutgoingBandwidthLimit_get(swigCPtr);
		}
		set
		{
			RakNetPINVOKE.RakNetStatistics_BPSLimitByOutgoingBandwidthLimit_set(swigCPtr, value);
		}
	}

	public uint[] messageInSendBuffer
	{
		get
		{
			uint[] array;
			if (!messageInSendBufferIsCached)
			{
				IntPtr source = RakNetPINVOKE.RakNetStatistics_messageInSendBuffer_get(swigCPtr);
				int num = 4;
				if (num <= 0)
				{
					return null;
				}
				array = new uint[num];
				int[] array2 = new int[num];
				Marshal.Copy(source, array2, 0, num);
				for (int i = 0; i < num; i++)
				{
					array[i] = (uint)array2[i];
				}
				messageInSendBufferCache = array;
				messageInSendBufferIsCached = true;
			}
			else
			{
				array = messageInSendBufferCache;
			}
			return array;
		}
		set
		{
			messageInSendBufferCache = value;
			messageInSendBufferIsCached = true;
			SetMessageInSendBuffer(value, value.Length);
		}
	}

	public double[] bytesInSendBuffer
	{
		get
		{
			double[] array;
			if (!bytesInSendBufferIsCached)
			{
				IntPtr source = RakNetPINVOKE.RakNetStatistics_bytesInSendBuffer_get(swigCPtr);
				int num = 4;
				if (num <= 0)
				{
					return null;
				}
				array = new double[num];
				double[] array2 = new double[num];
				Marshal.Copy(source, array2, 0, num);
				for (int i = 0; i < num; i++)
				{
					array[i] = array2[i];
				}
				bytesInSendBufferCache = array;
				bytesInSendBufferIsCached = true;
			}
			else
			{
				array = bytesInSendBufferCache;
			}
			return array;
		}
		set
		{
			bytesInSendBufferCache = value;
			bytesInSendBufferIsCached = true;
			SetBytesInSendBuffer(value, value.Length);
		}
	}

	public uint messagesInResendBuffer
	{
		get
		{
			return RakNetPINVOKE.RakNetStatistics_messagesInResendBuffer_get(swigCPtr);
		}
		set
		{
			RakNetPINVOKE.RakNetStatistics_messagesInResendBuffer_set(swigCPtr, value);
		}
	}

	public ulong bytesInResendBuffer
	{
		get
		{
			return RakNetPINVOKE.RakNetStatistics_bytesInResendBuffer_get(swigCPtr);
		}
		set
		{
			RakNetPINVOKE.RakNetStatistics_bytesInResendBuffer_set(swigCPtr, value);
		}
	}

	public float packetlossLastSecond
	{
		get
		{
			return RakNetPINVOKE.RakNetStatistics_packetlossLastSecond_get(swigCPtr);
		}
		set
		{
			RakNetPINVOKE.RakNetStatistics_packetlossLastSecond_set(swigCPtr, value);
		}
	}

	public float packetlossTotal
	{
		get
		{
			return RakNetPINVOKE.RakNetStatistics_packetlossTotal_get(swigCPtr);
		}
		set
		{
			RakNetPINVOKE.RakNetStatistics_packetlossTotal_set(swigCPtr, value);
		}
	}

	internal RakNetStatistics(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = new HandleRef(this, cPtr);
	}

	public RakNetStatistics()
		: this(RakNetPINVOKE.new_RakNetStatistics(), cMemoryOwn: true)
	{
	}

	internal static HandleRef getCPtr(RakNetStatistics obj)
	{
		if (obj != null)
		{
			if (obj.bytesInSendBufferIsCached)
			{
				obj.SetBytesInSendBuffer(obj.bytesInSendBuffer, obj.bytesInSendBuffer.Length);
			}
			if (obj.messageInSendBufferIsCached)
			{
				obj.SetMessageInSendBuffer(obj.messageInSendBuffer, obj.messageInSendBuffer.Length);
			}
			if (obj.runningTotalIsCached)
			{
				obj.SetRunningTotal(obj.runningTotal, obj.runningTotal.Length);
			}
			if (obj.valueOverLastSecondIsCached)
			{
				obj.SetValueOverLastSecond(obj.valueOverLastSecond, obj.valueOverLastSecond.Length);
			}
			obj.bytesInSendBufferIsCached = false;
			obj.messageInSendBufferIsCached = false;
			obj.runningTotalIsCached = false;
			obj.valueOverLastSecondIsCached = false;
		}
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	~RakNetStatistics()
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
					RakNetPINVOKE.delete_RakNetStatistics(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			GC.SuppressFinalize(this);
		}
	}

	public void SetBytesInSendBuffer(double[] inDoubleArray, int numDoubles)
	{
		RakNetPINVOKE.RakNetStatistics_SetBytesInSendBuffer(swigCPtr, inDoubleArray, numDoubles);
	}

	public void SetMessageInSendBuffer(uint[] inUnsignedIntArray, int numInts)
	{
		RakNetPINVOKE.RakNetStatistics_SetMessageInSendBuffer(swigCPtr, inUnsignedIntArray, numInts);
	}

	public void SetRunningTotal(ulong[] inUint64Array, int numUint64)
	{
		RakNetPINVOKE.RakNetStatistics_SetRunningTotal(swigCPtr, inUint64Array, numUint64);
	}

	public void SetValueOverLastSecond(ulong[] inUint64Array, int numUint64)
	{
		RakNetPINVOKE.RakNetStatistics_SetValueOverLastSecond(swigCPtr, inUint64Array, numUint64);
	}
}
