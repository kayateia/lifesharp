
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

		var json = await Network.HttpGetToJsonAsync(Settings.BaseUrl + "api/stream/1/contents", settings.authToken);
		Log.Info(LogTag, "Got back json: {0}", json);
		var model = new Protocol.StreamContents(json);
		if (model.error != null)
		{
			Log.Error(LogTag, "Error checking for images on the server");
			return;
		}

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
		Task.Run(() => doCheck(context, settings).Wait());
	}
}

}
