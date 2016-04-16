
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

		DateTimeOffset newCheckTime = DateTimeOffset.UtcNow;

		var json = await Network.HttpGetToJsonAsync(Settings.BaseUrl + "api/stream/1/contents", settings.authToken);
		Log.Info(LogTag, "Got back json: {0}", json);
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
				string imgpath = Path.Combine(GetPath(img.userLogin), img.filename);
				if (imgpath.Contains(".."))
				{
					Log.Error(LogTag, String.Format("Image name '{0}' is invalid. Skipping.", img.filename));
					continue;
				}
				Log.Info(LogTag, String.Format("Download image {0}/{1}", img.id, img.filename));

				await Network.HttpDownloadAsync(Settings.BaseUrl + "api/image/get/" + img.id, settings.authToken, imgpath);
				scanner.addFile(imgpath, "New picture from " + img.userLogin);
			}

			Log.Info(LogTag, "Finished downloads");

			scanner.scan();
			settings.lastCheck = newCheckTime;
			settings.commit();
		}

		Log.Info(LogTag, "File check complete.");
	}

	static string GetPath(string user)
	{
		string sdcard = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;
		string path = Path.Combine(sdcard, "Pictures", "LifeSharp", user);
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}

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
