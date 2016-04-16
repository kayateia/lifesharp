/*
	LifeStream - Instant Photo Sharing
	Copyright (C) 2014-2016 Kayateia

	This code is licensed under the GPL v3 or later.
	Please see the file LICENSE for more info.
 */

using System;
using System.Collections.Generic;
using Android.Media;
using Android.Content;
using Android.Util;
using Uri = Android.Net.Uri;

namespace LifeSharp
{

/// <summary>
/// Provides a MediaScannerConnection implementation that lets us hook into media scanner events,
/// as well as asking the media scanner to operate.
/// </summary>
/// <remarks>
/// This is based on the following:
/// http://stackoverflow.com/questions/4157724/dynamically-add-pictures-to-gallery-widget
/// </remarks>
public class MediaScannerWrapper : Java.Lang.Object, MediaScannerConnection.IMediaScannerConnectionClient
{
	const string LogTag = "LifeStream/MediaScannerWrapper";

	MediaScannerConnection _connection;
	List<string> _paths;
	Dictionary<string,string> _messages;

	public MediaScannerWrapper(Context ctx)
	{
		_connection = new MediaScannerConnection(ctx, this);
		_paths = new List<String>();
		_messages = new Dictionary<String,String>();
	}

	/// <summary>
	/// Adds a file for consideration.
	/// </summary>
	/// <param name="fn">The filename</param>
	/// <param name="message">A message that goes with the file, to be sent along with the callback later</param>
	public void addFile(string fn, string message)
	{
		_paths.Add(fn);
		_messages[fn] = message;
	}

	/// <summary>
	/// Returns true if anything was scanned.
	/// </summary>
	public bool scannedAny()
	{
		return _paths.Count > 0;
	}

	/// <summary>
	/// Actually do the scanning.
	/// </summary>
	public void scan()
	{
		_connection.Connect();
	}

	public void OnMediaScannerConnected()
	{
		foreach (string p in _paths)
		{
			_connection.ScanFile(p, getMime(p));
			Log.Info("MediaScannerWrapper", "media file submitted: " + p);
		}
	}

	/// <summary>
	/// Returns the MIME type of the file in question.
	/// </summary>
	string getMime(string fn)
	{
		int i = fn.LastIndexOf('.');
		if (i > 0)
		{
			string extension = fn.Substring(i+1);
			if (extension.EqualsIgnoreCase("jpg") || extension.EqualsIgnoreCase("jpeg"))
				return "image/jpeg";
			else if (extension.EqualsIgnoreCase("png"))
				return "image/png";
		}
		return "image/*";
	}

	/// <summary>
	/// Scanner callback; set this value before starting any scanning, and the delegate
	/// will be called with results.
	/// </summary>
	public Action<string /*path*/, Uri, string /*message*/> scanned;

	public void OnScanCompleted(string path, Uri uri)
	{
		// when scan is completes, update media file tags
		Log.Info("MediaScannerWrapper", "media file scanned: " + path + " - " + uri.ToString());
		this.scanned(path, uri, getMessage(path));
	}

	string getMessage(string path)
	{
		string rv;

		if (_messages.TryGetValue(path, out rv))
			return rv;
		else
			return "Itsa stuff!";
	}
}

}

