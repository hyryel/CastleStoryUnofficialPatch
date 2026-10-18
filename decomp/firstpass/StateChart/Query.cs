namespace StateChart;

public class Query<T>
{
	public T Result;

	public Query(T t = default(T))
	{
		Result = t;
	}
}
