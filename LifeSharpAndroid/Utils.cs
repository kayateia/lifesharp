using System;

namespace LifeSharp
{

static public class Utils
{
	static DateTimeOffset Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
	static public int DateTimeToUnix(DateTimeOffset dto)
	{
		return (int)(dto - Epoch).TotalSeconds;
	}

	static public int UnixNow()
	{
		return DateTimeToUnix(DateTimeOffset.UtcNow);
	}

	static public DateTimeOffset UnixToDateTime(int unix)
	{
		return Epoch.AddSeconds(unix);
	}
}

}

