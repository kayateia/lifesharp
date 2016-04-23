/*
	LifeStream - Instant Photo Sharing
	Copyright (C) 2014-2016 Kayateia

	This code is licensed under the GPL v3 or later.
	Please see the file LICENSE for more info.
 */

namespace LifeSharp
{

/// <summary>
/// Private configuration data (GCM keys, server URLs, etc). Copy this file and customize
/// it to fit your needs, then name it Config.cs.
/// </summary>
static public class Config
{
	/// <summary>
	/// The URL where the LifeStream Node server is located.
	/// </summary>
	public const string BaseUrl = "https://your.url.here.com/";

	/// <summary>
	/// GCM notification key - this is typically a string of digits.
	/// </summary>
	public const string GcmNotificationKey = null;
}

}

