/*
    LifeStream - Instant Photo Sharing
    Copyright (C) 2014-2016 Kayateia

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

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

	static public bool EqualsIgnoreCase(this string s, string t)
	{
		return s.Equals(t, StringComparison.OrdinalIgnoreCase);
	}
}

}

