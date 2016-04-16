using System;
using System.Collections.Generic;
using Android.Media;
using Android.Content;
using Android.Util;
using Uri = Android.Net.Uri;

namespace LifeSharp
{

// This is based on the following:
// http://stackoverflow.com/questions/4157724/dynamically-add-pictures-to-gallery-widget
public class MediaScannerWrapper : Java.Lang.Object, MediaScannerConnection.IMediaScannerConnectionClient
{
	const string LogTag = "LifeStream/MediaScannerWrapper";

	MediaScannerConnection _connection;
	List<string> _paths;
	Dictionary<string,string> _messages;

	// filePath - where to scan;
	// mime type of media to scan i.e. "image/jpeg".
	// use "*/*" for any media
	public MediaScannerWrapper(Context ctx)
	{
		_connection = new MediaScannerConnection(ctx, this);
		_paths = new List<String>();
		_messages = new Dictionary<String,String>();
	}

	public void addFile(String fn, String message)
	{
		_paths.Add(fn);
		_messages[fn] = message;
	}

	public bool scannedAny()
	{
		return _paths.Count > 0;
	}

	// do the scanning
	public void scan()
	{
		_connection.Connect();
	}

	// start the scan when scanner is ready
	public void OnMediaScannerConnected()
	{
		foreach (string p in _paths)
		{
			_connection.ScanFile(p, getMime(p));
			Log.Warn("MediaScannerWrapper", "media file submitted: " + p);
		}
	}

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

	public Action<string, Uri, string> scanned;

	public void OnScanCompleted(string path, Uri uri)
	{
		// when scan is completes, update media file tags
		Log.Warn("MediaScannerWrapper", "media file scanned: " + path + " - " + uri.ToString());
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

