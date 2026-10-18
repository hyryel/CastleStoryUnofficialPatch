using System.Collections.Generic;

namespace ThirdParty.GameAnalytics;

public interface IGameAnalyticsStore
{
	void AddEvent(GameAnalyticEvent.Types type, GameAnalyticsContext context = null);

	IEnumerable<GameAnalyticEvent> GetEvents();
}
