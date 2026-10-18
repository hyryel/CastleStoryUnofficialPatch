using System.Collections.Generic;
using UnityEngine;

public class InfoTable
{
	private struct InfoDefault
	{
		private Dictionary<string, object> dico;

		private string s;

		public object this[string t]
		{
			get
			{
				InfoTable infoTable = new InfoTable();
				dico.Add(s, infoTable);
				return infoTable[t];
			}
			set
			{
				InfoTable infoTable = new InfoTable();
				dico.Add(s, infoTable);
				infoTable[t] = value;
			}
		}

		public InfoDefault(Dictionary<string, object> dico, string s)
		{
			this.dico = dico;
			this.s = s;
		}

		public static implicit operator InfoTable(InfoDefault d)
		{
			InfoTable infoTable = new InfoTable();
			d.dico.Add(d.s, infoTable);
			return infoTable;
		}

		public static implicit operator bool(InfoDefault d)
		{
			return false;
		}

		public static implicit operator int(InfoDefault d)
		{
			return 0;
		}

		public static implicit operator float(InfoDefault d)
		{
			return 0f;
		}

		public static implicit operator Vector3(InfoDefault d)
		{
			return Vector3.zero;
		}

		public static float operator +(InfoDefault x, float y)
		{
			return y;
		}

		public static float operator +(float x, InfoDefault y)
		{
			return x;
		}

		public static InfoDefault operator +(InfoDefault x, InfoDefault y)
		{
			return x;
		}
	}

	private Dictionary<string, object> dico;

	public object this[string s]
	{
		get
		{
			if (dico.TryGetValue(s, out var value) && value != null)
			{
				return value;
			}
			return new InfoDefault(dico, s);
		}
		set
		{
			dico[s] = value;
		}
	}

	public InfoTable()
	{
		dico = new Dictionary<string, object>();
	}
}
