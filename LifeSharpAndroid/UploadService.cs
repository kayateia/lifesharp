/*
	LifeStream - Instant Photo Sharing
	Copyright (C) 2014-2016 Kayateia

	This code is licensed under the GPL v3 or later.
	Please see the file LICENSE for more info.
 */

using System;
using System.IO;
using Android.Content;
using Android.Graphics;
using Android.Media;

using Path = System.IO.Path;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Json;

namespace LifeSharp
{

[LifeSharpService]
public class UploadService : ILifeSharpService
{
	const string LogTag = "LifeSharp/UploadService";
	object _lock = new object();

	public UploadService()
	{
	}

	public void start(Context context, Settings settings)
	{
		checkForNewImages(context, settings);
	}

	public void stop(Context context, Settings settings)
	{
	}

	public void kick(Context context, Settings settings)
	{
		checkForNewImages(context, settings);
	}

	void checkForNewImages(Context context, Settings settings)
	{
		Task.Run(() =>
		{
			// Only allow one background instance at a time. Otherwise we end up with duplicate sends.
			lock (_lock)
			{
				Log.Info(LogTag, "Checking for new images to upload");

				var db = ImageDatabaseAndroid.GetSingleton(context);
				Image[] images = db.getItemsToUpload();

				foreach (Image i in images)
				{
					if (uploadImage(i, CaptureService.GetThumbnailPath(context, i), settings.authToken).Result)
						db.markSent(i.id);
				}
			}
		});
	}

	async Task<bool> uploadImage(Image image, string thumbPath, string authToken)
	{
		try
		{
			JsonValue response = await Network.HttpPostFileToJsonAsync(Settings.BaseUrl + "api/image/post", authToken, new Dictionary<string, string>()
				{
					{ "streamid", "1" }
				},
				"image", thumbPath);

			if (Protocol.Basic.Succeeded(response))
				return true;
			else
			{
				Log.Error(LogTag, "Can't upload image {0}: {1}", image.filename, response);
				return false;
			}
		}
		catch (Exception e)
		{
			Log.Error(LogTag, "Exception: {0}", e);
			return false;
		}
	}
}

}

