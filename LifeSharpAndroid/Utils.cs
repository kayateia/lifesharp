/*
	LifeStream - Instant Photo Sharing
	Copyright (C) 2014-2016 Kayateia

	This code is licensed under the GPL v3 or later.
	Please see the file LICENSE for more info.
 */

using System;

namespace LifeSharp
{

/// <summary>
/// Various utilities
/// </summary>
static public class Utils
{
	static DateTimeOffset Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

	/// <summary>
	/// Converts a DateTimeOffset into a UNIX timestamp.
	/// </summary>
	static public int DateTimeToUnix(DateTimeOffset dto)
	{
		return (int)(dto - Epoch).TotalSeconds;
	}

	/// <summary>
	/// Gets the current time as a UNIX timestamp.
	/// </summary>
	static public int UnixNow()
	{
		return DateTimeToUnix(DateTimeOffset.UtcNow);
	}

	/// <summary>
	/// Converts a UNIX timestamp to a DateTimeOffset.
	/// </summary>
	static public DateTimeOffset UnixToDateTime(int unix)
	{
		return Epoch.AddSeconds(unix);
	}

	/// <summary>
	/// Adds an EqualsIgnoreCase() call to the string object, which does a comparison
	/// using OrdinalIgnoreCase.
	/// </summary>
	static public bool EqualsIgnoreCase(this string s, string t)
	{
		return s.Equals(t, StringComparison.OrdinalIgnoreCase);
	}
}

}

