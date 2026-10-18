using System;
using System.Runtime.InteropServices;

namespace RakNet;

public class ColumnDescriptor : IDisposable
{
	private HandleRef swigCPtr;

	protected bool swigCMemOwn;

	public string columnName
	{
		get
		{
			return RakNetPINVOKE.ColumnDescriptor_columnName_get(swigCPtr);
		}
		set
		{
			RakNetPINVOKE.ColumnDescriptor_columnName_set(swigCPtr, value);
		}
	}

	public Table.ColumnType columnType
	{
		get
		{
			return (Table.ColumnType)RakNetPINVOKE.ColumnDescriptor_columnType_get(swigCPtr);
		}
		set
		{
			RakNetPINVOKE.ColumnDescriptor_columnType_set(swigCPtr, (int)value);
		}
	}

	internal ColumnDescriptor(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = new HandleRef(this, cPtr);
	}

	public ColumnDescriptor()
		: this(RakNetPINVOKE.new_ColumnDescriptor__SWIG_0(), cMemoryOwn: true)
	{
	}

	public ColumnDescriptor(string cn, Table.ColumnType ct)
		: this(RakNetPINVOKE.new_ColumnDescriptor__SWIG_1(cn, (int)ct), cMemoryOwn: true)
	{
	}

	internal static HandleRef getCPtr(ColumnDescriptor obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	~ColumnDescriptor()
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
					RakNetPINVOKE.delete_ColumnDescriptor(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			GC.SuppressFinalize(this);
		}
	}
}
