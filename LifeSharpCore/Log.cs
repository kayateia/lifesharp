/*
	LifeStream - Instant Photo Sharing
	Copyright (C) 2014-2016 Kayateia

	This code is licensed under the GPL v3 or later.
	Please see the file LICENSE for more info.
 */

using System;

namespace LifeSharp
{

public interface ILog
{
	void info(string tag, string text);
	void warn(string tag, string text);
	void error(string tag, string text);
}

static public class Log
{
	static public void SetLogger(ILog logger)
	{
		_logger = logger;
	}

	static public void Info(string tag, string text)
	{
		if (_logger != null)
			_logger.info(tag, text);
	}

	static public void Info(string tag, string fmt, params object[] p)
	{
		if (_logger != null)
			_logger.info(tag, String.Format(fmt, p));
	}

	static public void Warn(string tag, string text)
	{
		if (_logger != null)
			_logger.warn(tag, text);
	}

	static public void Warn(string tag, string fmt, params object[] p)
	{
		if (_logger != null)
			_logger.warn(tag, String.Format(fmt, p));
	}

	static public void Error(string tag, string text)
	{
		if (_logger != null)
			_logger.error(tag, text);
	}

	static public void Error(string tag, string fmt, params object[] p)
	{
		if (_logger != null)
			_logger.error(tag, String.Format(fmt, p));
	}

	static ILog _logger;
}

}

