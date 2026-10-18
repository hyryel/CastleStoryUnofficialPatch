using System;
using System.Collections.Generic;
using Brix.IO.Saves.Filters;
using Newtonsoft.Json.Linq;

namespace Brix.IO.Saves;

public abstract class BaseFetcherBuilder<BLDR, FETCHER> where BLDR : BaseFetcherBuilder<BLDR, FETCHER> where FETCHER : AbstractFetcher, new()
{
	protected List<IJObjectFilter> filters = new List<IJObjectFilter>();

	public BLDR Id(string id)
	{
		return FieldEqualsTo("$id", id);
	}

	public BLDR Type(string type)
	{
		return FieldEqualsTo("$type", type);
	}

	public BLDR IdOrRef(string id)
	{
		filters.Add(new EitherFilter(new FieldValueEqualFilter<string>("$id", id), new FieldValueEqualFilter<string>("$ref", id)));
		return (BLDR)this;
	}

	internal BLDR WithId()
	{
		return HasField("$id");
	}

	internal BLDR WithRef()
	{
		return HasField("$ref");
	}

	internal BLDR WithIdRefOrParent()
	{
		filters.Add(new AnyFieldNameEquals("$id", "$ref", "$parent"));
		return (BLDR)this;
	}

	public BLDR FieldEqualsTo<T>(string fieldname, T wantedValue)
	{
		filters.Add(new FieldValueEqualFilter<T>(fieldname, wantedValue));
		return (BLDR)this;
	}

	public BLDR FieldEqualsTo<T>(string fieldname, T wantedValue, T defaultValue)
	{
		filters.Add(new FieldValueEqualFilterWithDefaultValue<T>(fieldname, wantedValue, defaultValue));
		return (BLDR)this;
	}

	public BLDR FieldIsNull(string fieldname)
	{
		filters.Add(new FieldValueIsNullFilter(fieldname));
		return (BLDR)this;
	}

	public BLDR FieldIsNotNull(string fieldname)
	{
		filters.Add(new FieldValueIsNotNullFilter(fieldname));
		return (BLDR)this;
	}

	public BLDR HasField(string fieldname)
	{
		filters.Add(new FieldNameEqual(fieldname));
		return (BLDR)this;
	}

	public BLDR Matches(Func<JContainer, bool> predicate)
	{
		filters.Add(new PredicateFilter(predicate));
		return (BLDR)this;
	}

	public virtual FETCHER Make()
	{
		return new FETCHER
		{
			filters = filters
		};
	}
}
