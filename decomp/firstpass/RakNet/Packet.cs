using System;
using System.Runtime.InteropServices;

namespace RakNet;

public class Packet : IDisposable
{
	private HandleRef swigCPtr;

	protected bool swigCMemOwn;

	private bool dataIsCached;

	private byte[] dataCache;

	public SystemAddress systemAddress
	{
		get
		{
			IntPtr intPtr = RakNetPINVOKE.Packet_systemAddress_get(swigCPtr);
			return (!(intPtr == IntPtr.Zero)) ? new SystemAddress(intPtr, cMemoryOwn: false) : null;
		}
		set
		{
			RakNetPINVOKE.Packet_systemAddress_set(swigCPtr, SystemAddress.getCPtr(value));
		}
	}

	public RakNetGUID guid
	{
		get
		{
			IntPtr intPtr = RakNetPINVOKE.Packet_guid_get(swigCPtr);
			return (!(intPtr == IntPtr.Zero)) ? new RakNetGUID(intPtr, cMemoryOwn: false) : null;
		}
		set
		{
			RakNetPINVOKE.Packet_guid_set(swigCPtr, RakNetGUID.getCPtr(value));
		}
	}

	public uint length
	{
		get
		{
			return RakNetPINVOKE.Packet_length_get(swigCPtr);
		}
		set
		{
			RakNetPINVOKE.Packet_length_set(swigCPtr, value);
		}
	}

	public uint bitSize
	{
		get
		{
			return RakNetPINVOKE.Packet_bitSize_get(swigCPtr);
		}
		set
		{
			RakNetPINVOKE.Packet_bitSize_set(swigCPtr, value);
		}
	}

	public byte[] data
	{
		get
		{
			byte[] array;
			if (!dataIsCached)
			{
				IntPtr source = RakNetPINVOKE.Packet_data_get(swigCPtr);
				int num = (int)((Packet)swigCPtr.Wrapper).length;
				if (num <= 0)
				{
					return null;
				}
				array = new byte[num];
				Marshal.Copy(source, array, 0, num);
				dataCache = array;
				dataIsCached = true;
			}
			else
			{
				array = dataCache;
			}
			return array;
		}
		set
		{
			dataCache = value;
			dataIsCached = true;
			SetPacketData(value, value.Length);
		}
	}

	public bool deleteData
	{
		get
		{
			return RakNetPINVOKE.Packet_deleteData_get(swigCPtr);
		}
		set
		{
			RakNetPINVOKE.Packet_deleteData_set(swigCPtr, value);
		}
	}

	public bool wasGeneratedLocally
	{
		get
		{
			return RakNetPINVOKE.Packet_wasGeneratedLocally_get(swigCPtr);
		}
		set
		{
			RakNetPINVOKE.Packet_wasGeneratedLocally_set(swigCPtr, value);
		}
	}

	internal Packet(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = new HandleRef(this, cPtr);
	}

	public Packet()
		: this(RakNetPINVOKE.new_Packet(), cMemoryOwn: true)
	{
	}

	internal static HandleRef getCPtr(Packet obj)
	{
		if (obj != null)
		{
			if (obj.dataIsCached)
			{
				obj.SetPacketData(obj.data, obj.data.Length);
			}
			obj.dataIsCached = false;
		}
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	~Packet()
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
					RakNetPINVOKE.delete_Packet(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			GC.SuppressFinalize(this);
		}
	}

	public void SetPacketData(byte[] inByteArray, int numBytes)
	{
		RakNetPINVOKE.Packet_SetPacketData(swigCPtr, inByteArray, numBytes);
	}
}
