using System.Collections.Generic;

internal class ItemComparer : IComparer<GA_Submit.Item>
{
	public int Compare(GA_Submit.Item item1, GA_Submit.Item item2)
	{
		if (item1.Type != GA_Submit.CategoryType.GA_Event && item2.Type == GA_Submit.CategoryType.GA_Event)
		{
			return 1;
		}
		if (item2.Type != GA_Submit.CategoryType.GA_Event && item1.Type == GA_Submit.CategoryType.GA_Event)
		{
			return -1;
		}
		float addTime = item1.AddTime;
		float addTime2 = item2.AddTime;
		if (addTime < addTime2)
		{
			return 1;
		}
		if (addTime == addTime2)
		{
			return 0;
		}
		return -1;
	}
}
