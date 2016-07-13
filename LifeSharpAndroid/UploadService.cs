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
				checkForNewImagesInner(context, settings);
			}
		});
	}

	void checkForNewImagesInner(Context context, Settings settings)
	{
		Log.Info(LogTag, "Checking for new images to scale/etc/send");

		var db = ImageDatabaseAndroid.GetSingleton(context);
		Image[] images = db.getItemsToUpload();

		foreach (Image i in images)
		{
			string destfn = GetThumbnailPath(context, i);
			if (!File.Exists(destfn))
				scaleImage(i.sourcePath, destfn);
			if (uploadImage(i, destfn, settings.authToken, (int)settings.defaultStream).Result) {
				db.markSent(i.id);
				if (settings.uploadNotifications) {
					Notifications.NotifyUpload(context, 200, true, "Uploaded image", "Uploaded image", "", i.sourcePath, destfn);
				}
			}
		}
	}

	void scaleImage(string source, string dest)
	{
		Log.Info(LogTag, "Scaling image from {0} to {1}", source, dest);

		// Open once to get the file size.
		var opts = new BitmapFactory.Options()
		{
			InJustDecodeBounds = true
		};
		BitmapFactory.DecodeFile(source, opts);

		// Also verify the image orientation. Some phones like to set EXIF instead of rotating the pixels.
		var exif = new ExifInterface(source);
		int orientation = exif.GetAttributeInt(ExifInterface.TagOrientation, (int)Orientation.Normal);
		int rotate = 0;
		switch (orientation)
		{
			case (int)Orientation.Rotate270:
				rotate = 270;
				break;
			case (int)Orientation.Rotate180:
				rotate = 180;
				break;
			case (int)Orientation.Rotate90:
				rotate = 90;
				break;
		}

		// Make this configurable again later.
		const int largestSize = 800;

		// Figure out the target size;
		int width = -1, height = -1;
		if (opts.OutWidth > largestSize || opts.OutHeight > largestSize)
		{
			if (opts.OutWidth > opts.OutHeight)
			{
				width = largestSize;
				height = opts.OutHeight * width / opts.OutWidth;
			}
			else
			{
				height = largestSize;
				width = opts.OutWidth * height / opts.OutHeight;
			}
		}

		Bitmap image = BitmapFactory.DecodeFile(source);
		image = Bitmap.CreateScaledBitmap(image, width, height, true);

		// Do rotation if needed.
		if (rotate != 0)
		{
			Log.Info(LogTag, "{0}: rotation by {1}", source, rotate);
			var matrix = new Matrix();
			matrix.PreRotate(rotate);
			image = Bitmap.CreateBitmap(image, 0, 0, image.Width, image.Height, matrix, true);
		}

		using (FileStream fs = File.OpenWrite(dest))
			image.Compress(Bitmap.CompressFormat.Jpeg, 70, fs);
	}

	public static string GetThumbnailPath(Context context, Image img)
	{
		return Path.Combine(GetUserPath(context), img.filename);
	}

	static string GetUserPath(Context context)
	{
		// This is the app-private path. We can put our thumbnails in there since there's no need
		// to share them out to the gallery or anything.
		string sdcard = context.GetExternalFilesDir(Android.OS.Environment.DirectoryPictures).AbsolutePath;
		string p = Path.Combine(sdcard, "Thumbnails");
		if (!Directory.Exists(p))
			Directory.CreateDirectory(p);

		return p;
	}

	async Task<bool> uploadImage(Image image, string thumbPath, string authToken, int streamId)
	{
		Log.Info(LogTag, "Uploading image {0}", thumbPath);
		try
		{
			JsonValue response = await Network.HttpPostFileToJsonAsync(Settings.BaseUrl + "api/image", authToken, new Dictionary<string, string>()
				{
					{ "streamid", streamId.ToString() }
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

