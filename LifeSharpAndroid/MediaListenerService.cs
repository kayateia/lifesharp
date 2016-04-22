/*
	LifeStream - Instant Photo Sharing
	Copyright (C) 2014-2016 Kayateia

	This code is licensed under the GPL v3 or later.
	Please see the file LICENSE for more info.
 */

namespace LifeSharp
{
using System;
using System.Collections.Generic;
using Android.App;
using Android.Provider;
using Android.Database;
using Android.Content;
using Android.Util;

public class PhotoMediaObserver : ContentObserver
{
	const string LogTag = "LifeSharp/PhotoMediaObserver";
	Context _context;
	Settings _settings;

	public PhotoMediaObserver(Context context, Settings settings) : base(null)
	{
		_context = context;
		_settings = settings;
	}

	public override void OnChange(bool selfChange)
	{
		// var newMedia = new List<Media>();
		long lastImageTimestamp = _settings.lastImageProcessedTimestamp;
		long newLastTimestamp = -1;
		bool anyNew = false;

		var uri = MediaStore.Images.Media.ExternalContentUri;
		using (var cursor = _context.ContentResolver.Query(uri, null, null, null, "date_added DESC"))
		{
			int dataColumn = cursor.GetColumnIndexOrThrow(MediaStore.MediaColumns.Data);
			int mimeTypeColumn = cursor.GetColumnIndexOrThrow(MediaStore.MediaColumns.MimeType);
			int timestampColumn = cursor.GetColumnIndexOrThrow(MediaStore.MediaColumns.DateAdded);
			while (cursor.MoveToNext())
			{
				string filePath = cursor.GetString(dataColumn);
				string mimeType = cursor.GetString(mimeTypeColumn);
				long timestamp = cursor.GetLong(timestampColumn);

				// This should only happen once because the records should be sorted, but >_>
				if (timestamp > newLastTimestamp)
					newLastTimestamp = timestamp;

				if (timestamp > lastImageTimestamp)
				{
					if (DownloadService.IsOneOfOurs(filePath))
					{
						Log.Info(LogTag, "{0} is one of ours; skipping", filePath);
					}
					else
					{
						Log.Info(LogTag, "Found file: {0} {1}", filePath, mimeType);
						ImageDatabaseAndroid.GetSingleton(_context).addToUploadQueue(filePath, DateTimeOffset.UtcNow/*.AddMinutes(1)*/, "");
						anyNew = true;
					}
				}
			}
		}

		// Update?
		if (newLastTimestamp > lastImageTimestamp)
		{
			_settings.lastImageProcessedTimestamp = newLastTimestamp;
			_settings.commit();
		}

		// If we found anything new, kick our services to get the pipeline moving.
		if (anyNew)
		{
			LifeSharpService.Instance.kickService<UploadService>();
		}
	}
}

[LifeSharpService]
public class MediaListenerService : ILifeSharpService
{
	const string LogTag = "LifeSharp/MediaListenerService";

	public MediaListenerService()
	{
	}

	public void start(Context context, Settings settings)
	{
		Log.Info(LogTag, "Started media listener");
		context.ContentResolver.RegisterContentObserver(MediaStore.Images.Media.ExternalContentUri, false, new PhotoMediaObserver(context, settings));
	}

	public void stop(Context context, Settings settings)
	{
		Log.Info(LogTag, "Stopped media listener");
	}

	public void kick(Context context, Settings settings)
	{
	}
}
}

