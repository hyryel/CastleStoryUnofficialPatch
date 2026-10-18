public class Legitimization
{
	public string path;

	public string hash;

	public override bool Equals(object obj)
	{
		if (obj.GetType() == typeof(Legitimization))
		{
			return Same(this, (Legitimization)obj);
		}
		return base.Equals(obj);
	}

	public override int GetHashCode()
	{
		return (((path != null) ? path.GetHashCode() : 0) * 397) ^ ((hash != null) ? hash.GetHashCode() : 0);
	}

	private bool Same(Legitimization legit1, Legitimization legit2)
	{
		string text = legit1.path.Replace("\\", "/").Replace("//", "/");
		string text2 = legit2.path.Replace("\\", "/").Replace("//", "/");
		bool flag = text == text2;
		bool flag2 = legit1.hash == legit2.hash;
		return flag && flag2;
	}
}
