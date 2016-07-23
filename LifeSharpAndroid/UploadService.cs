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

			// Upload the image and save the image ID returned from the server
			var newId = uploadImage(i, destfn, settings.authToken, (int)settings.defaultStream).Result;
			// ID -1 comes from Protocol, and indicates the request failed
			if (newId > -1)
			{
				// ID 0 comes from Protocol, and indicates the the request failed because an image with
				// the same filename was already present on the server.
				db.markSent(i.id);
				// ID > 0 comes from the server, and indicates the server-side ID of the newly-uploaded image.
				if (newId > 0 && settings.uploadNotifications)
				{
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

		// TODO: Make this configurable again later.
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

	/// <summary>
	/// Uploads an image to the server.
	/// </summary>
	/// <returns>
	/// If upload was successful, return the server-side image ID of the uploaded image.
	/// If upload failed because the image already existed server-side, return 0.
	/// If upload failed for any other reason, return -1.
	/// </returns>
	/// <param name="image">Image.</param>
	/// <param name="thumbPath">Thumb path.</param>
	/// <param name="authToken">Auth token.</param>
	/// <param name="streamId">Stream identifier.</param>
	async Task<int> uploadImage(Image image, string thumbPath, string authToken, int streamId)
	{
		Log.Info(LogTag, "Uploading image {0}", thumbPath);
		try
		{
			JsonValue response = await Network.HttpPostFileToJsonAsync(Settings.BaseUrl + "api/image", authToken, new Dictionary<string, string>()
				{
					{ "streamid", streamId.ToString() }
				},
				"image", thumbPath);

			var newId = Protocol.Basic.SucceededWithId(response);
			if (newId == -1)
				Log.Error(LogTag, "Can't upload image {0}: {1}", image.filename, response);

			return newId;
		}
		catch (Exception e)
		{
			Log.Error(LogTag, "Exception: {0}", e);
			return -1;
		}
	}
}

}

