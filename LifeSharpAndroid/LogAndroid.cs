/*
	LifeStream - Instant Photo Sharing
	Copyright (C) 2014-2016 Kayateia

	This code is licensed under the GPL v3 or later.
	Please see the file LICENSE for more info.
 */

using ALog = Android.Util.Log;

namespace LifeSharp
{

/// <summary>
/// Android implementation of the ILog interface
/// </summary>
public class LogAndroid : ILog
{
	public LogAndroid()
	{
	}

	public void info(string tag, string text)
	{
		ALog.Info(tag, text);
	}

	public void warn(string tag, string text)
	{
		ALog.Warn(tag, text);
	}

	public void error(string tag, string text)
	{
		ALog.Error(tag, text);
	}
}

}

