/*
    LifeStream - Instant Photo Sharing
    Copyright (C) 2014-2016 Kayateia

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
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
					Log.Info(LogTag, "Found file: {0} {1}", filePath, mimeType);
				}
			}
		}

		// Update?
		if (newLastTimestamp > lastImageTimestamp)
		{
			_settings.lastImageProcessedTimestamp = newLastTimestamp;
			_settings.commit();
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
}
}

