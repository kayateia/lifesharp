/*
	LifeStream - Instant Photo Sharing
	Copyright (C) 2014-2016 Kayateia

	This code is licensed under the GPL v3 or later.
	Please see the file LICENSE for more info.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Json;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Util;
using System.Threading.Tasks;

using Uri = Android.Net.Uri;

namespace LifeSharp
{

/// <summary>
/// Implements a streaming service for new images, pulling them down from the server as they
/// become available, and providing notifications to the user that new images are available.
/// </summary>
[LifeSharpService]
public class StreamService : ILifeSharpService
{
	const string LogTag = "LifeSharp/StreamService";

	public StreamService()
	{
	}

	async Task doCheck(Context context, Settings settings)
	{
		Log.Info(LogTag, "Doing new files check on server");

		// We pull the new check time up front because our snapshot of what images are available to
		// stream will be based on right now.
		//
		// FIXME: This isn't actually used right now; the REST call always pull the top 50.
		DateTimeOffset newCheckTime = DateTimeOffset.UtcNow;

		// Call to the server to get our list of images to pull.
		var json = await Network.HttpGetToJsonAsync(Settings.BaseUrl + "api/stream/1/contents", settings.authToken);
		var model = new Protocol.StreamContents(json);
		if (model.error != null)
		{
			Log.Error(LogTag, "Error checking for images on the server");
			return;
		}

		if (model.images.Length > 0)
		{
			// This will follow along behind our downloads and provide media server IDs that we
			// can feed to a notification, so that when the user taps on it, it'll take them to
			// that picture with their chosen app.
			//
			// This will also make the images available to the wider Android ecosystem by getting them
			// into the media scanner.
			var scanner = new MediaScannerWrapper(context)
			{
				scanned = (string path, Uri uri, string message) =>
				{
					// The user directory will be the last path component. We can hash
					// that and make a unique notification ID. This isn't guaranteed to be
					// unique, but for our test purposes, it should work.
					Notifications.NotifyDownload(context, 100, true, message, message, "", uri);
				}
			};

			foreach (var img in model.images)
			{
				// Figure out where the image will go. Do a sanity check to make sure it doesn't contain an exploit.
				string imgpath = Path.Combine(GetPath(img.userLogin), img.filename);
				if (imgpath.Contains(".."))
				{
					Log.Error(LogTag, String.Format("Image name '{0}' is invalid. Skipping.", img.filename));
					continue;
				}
				Log.Info(LogTag, String.Format("Download image {0}/{1} to {2}", img.id, img.filename, imgpath));

				await Network.HttpDownloadAsync(Settings.BaseUrl + "api/image/get/" + img.id, settings.authToken, imgpath);
				scanner.addFile(imgpath, "New picture from " + img.userLogin);
			}

			scanner.scan();

			settings.lastCheck = newCheckTime;
			settings.commit();
		}

		Log.Info(LogTag, "File check complete.");
	}

	static string GetPath(string user)
	{
		// We make the last component "LifeStream_user" so that it will show up properly in gallery apps.
		string sdcard = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;
		string path = Path.Combine(sdcard, "Pictures", "LifeSharp", "LifeStream_" + user);
		if (!Directory.Exists(path))
			Directory.CreateDirectory(path);

		return path;
	}

	public void start(Context context, Settings settings)
	{
		Log.Info(LogTag, "Started stream service");
	}

	public void stop(Context context, Settings settings)
	{
		Log.Info(LogTag, "Stopped stream service");
	}

	public void kick(Context context, Settings settings)
	{
		Task.Run(() =>
		{
			try
			{
				doCheck(context, settings).Wait();
			}
			catch (Exception e)
			{
				Log.Error(LogTag, "Exception during kick: {0}", e);
			}
		});
	}
}

}
