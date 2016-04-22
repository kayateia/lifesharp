/*
	LifeStream - Instant Photo Sharing
	Copyright (C) 2014-2016 Kayateia

	This code is licensed under the GPL v3 or later.
	Please see the file LICENSE for more info.
 */

using System;
using System.IO;
using Android.Database.Sqlite;
using Android.Content;
using Android.Database;
using Android.Util;

using Path = System.IO.Path;
using System.Globalization;

namespace LifeSharp
{

/// <summary>
/// Implementation of IImageDatabase that works with Android SQLite and sdcard.
/// </summary>
public class ImageDatabaseAndroid : SQLiteOpenHelper, IImageDatabase
{
	const string LogTag = "LifeSharp/ImageDatabase";

	const int ImageDatabaseVersion = 2;
	const string ImageDatabaseName = "LifeSharpImageDatabase";
	const string TableName = "images";

	const string KeyState = "state";
	const string KeyFilename = "filename";
	const string KeySourcePath = "sourcepath";
	const string KeyUserLogin = "userlogin";
	const string KeyQueuestamp = "queuestamp";
	const string KeySendTimeout = "sendtimeout";
	const string KeyComment = "sendcomment";

	// This has to be done through the same object to get database locking, unfortunately.
	static object s_lock = new object();
	static ImageDatabaseAndroid s_global = null;
	static public ImageDatabaseAndroid GetSingleton(Context context)
	{
		lock (s_lock)
		{
			if (s_global == null)
				s_global = new ImageDatabaseAndroid(context.ApplicationContext);
			return s_global;
		}
	}

	public ImageDatabaseAndroid(Context context)
		: base(context, ImageDatabaseName, null, ImageDatabaseVersion)
	{
	}

	public override void OnCreate(SQLiteDatabase db)
	{
		string createdProcessedTable = "create table " + TableName + "("
			+ KeyState + " integer,"
			+ KeyFilename + " filename,"
			+ KeySourcePath + " sourcepath,"
			+ KeyUserLogin + " userlogin,"
			+ KeyQueuestamp + " integer,"
			+ KeySendTimeout + " integer,"
			+ KeyComment + " string"
			+ ")";
		db.ExecSQL(createdProcessedTable);
	}

	public override void OnUpgrade(SQLiteDatabase db, int oldVersion, int newVersion)
	{
		// This will be changed if/when we change the tables.
		// throw new NotImplementedException("Can't upgrade image database");

		// This will only be done during heavy database development. This needs to go away once
		// the app is actually used in production, and be replaced by an upgrade method.

		// Drop older table if existed
		db.ExecSQL("drop table if exists " + TableName);

		// Create tables again
		OnCreate(db);
	}

	void performWritable(string func, Action<SQLiteDatabase> guts)
	{
		using (SQLiteDatabase db = this.WritableDatabase)
		{
			try
			{
				guts(db);
			}
			catch (SQLiteException e)
			{
				Log.Error(LogTag, "SQL exception during {0}: {1}", func, e);
			}
		}
	}

	T performReadable<T>(string func, Func<SQLiteDatabase, T> guts)
	{
		using (SQLiteDatabase db = this.ReadableDatabase)
		{
			try
			{
				return guts(db);
			}
			catch (SQLiteException e)
			{
				Log.Error(LogTag, "SQL exception during {0}: {1}", func, e);
				return default(T);
			}
		}
	}

	public void addToUploadQueue(string fullSourcePath, DateTimeOffset sendTimeout, string comment)
	{
		performWritable("addToUploadQueue", (db) =>
		{
			// Get the filename.
			string fn = Path.GetFileName(fullSourcePath);

			// Check if it's there already.
			using (ICursor cursor = db.Query(TableName,
				new string[] { KeyFilename },
				KeyFilename + "=?",
				new string[] { fn },
				null, null, null, null))
			{
				if (cursor.MoveToFirst())
					return;
			}

			// Go ahead and insert.
			ContentValues values = new ContentValues();
			values.Put(KeyState, (int)Image.State.NewForUpload);
			values.Put(KeyFilename, fn);
			values.Put(KeySourcePath, fullSourcePath);
			values.Put(KeyQueuestamp, Utils.UnixNow());
			values.Put(KeySendTimeout, Utils.DateTimeToUnix(sendTimeout));
			values.Put(KeyComment, comment);
			db.Insert(TableName, null, values);
		});
	}

	public void addDownloadedFile(string fullDownloadedPath, string filename, string userLogin, DateTimeOffset fileTime, string comment)
	{
		performWritable("addDownloadedFile", (db) =>
		{
			// Check if it's there already.
			using (ICursor cursor = db.Query(TableName,
				new string[] { KeyFilename },
				KeyUserLogin + "=? and " + KeyFilename + "=?",
				new string[] { userLogin, filename },
				null, null, null, null))
			{
				if (cursor.MoveToFirst())
					return;
			}

			// Go ahead and insert.
			ContentValues values = new ContentValues();
			values.Put(KeyState, (int)Image.State.Downloaded);
			values.Put(KeyFilename, filename);
			values.Put(KeySourcePath, fullDownloadedPath);
			values.Put(KeyUserLogin, userLogin);
			values.Put(KeyQueuestamp, Utils.DateTimeToUnix(fileTime));
			values.Put(KeySendTimeout, 0);
			values.Put(KeyComment, comment);
			db.Insert(TableName, null, values);
		});
	}

	public string getScaledPath(Image image)
	{
		throw new NotImplementedException();
	}

	public Image getImageById(int id)
	{
		return performReadable("getImageById", (db) =>
		{
			return GetOneImage(db, id);
		});
	}

	public Image getImageByUserAndFileName(string userLogin, string filename)
	{
		return performReadable("getImageByFileName", (db) =>
		{
			using (ICursor cursor = db.Query(TableName, AllColumns,
				KeyUserLogin + "=? and " + KeyFilename + "=?", new string[] { userLogin, filename },
				null, null, null ))
			{
				if (!cursor.MoveToFirst())
					return null;

				return GetImage(cursor);
			}
		});
	}

	public Image[] getItemsToUpload()
	{
		return performReadable("getItemsToUpload", (db) =>
		{
			using (ICursor cursor = db.Query(TableName,
				AllColumns,
				KeyState + "=" + (int)Image.State.NewForUpload
					+ " and " + KeySendTimeout + " <= ?",
				new string[] { Utils.UnixNow().ToString(CultureInfo.InvariantCulture) },
				null, null, KeyQueuestamp))
			{
				if (!cursor.MoveToFirst())
					return new Image[0];

				Image[] rv = new Image[cursor.Count];
				for (int i=0; i<cursor.Count; ++ i)
				{
					rv[i] = GetImage(cursor);
					cursor.MoveToNext();
				}

				return rv;
			}
		});
	}

	static string[] AllColumns
	{
		get
		{
			return new String[] { "rowid", KeyState, KeyFilename, KeySourcePath, KeyQueuestamp, KeySendTimeout, KeyComment };
		}
	}

	static Image GetImage(ICursor cursor)
	{
		return new Image()
		{
			id = cursor.GetInt(0),
			state = (Image.State)cursor.GetInt(1),
			filename = cursor.GetString(2),
			sourcePath = cursor.GetString(3),
			queueStamp = Utils.UnixToDateTime(cursor.GetInt(4)),
			sendTimeout = Utils.UnixToDateTime(cursor.GetInt(5)),
			comment = cursor.GetString(6)
		};
	}

	static Image GetOneImage(SQLiteDatabase db, int id)
	{
		using (ICursor cursor = db.Query(TableName, AllColumns, "rowid=?", new string[] { id.ToString() }, null, null, null ))
		{
			if (!cursor.MoveToFirst())
				return null;

			return GetImage(cursor);
		}
	}

	public void deleteImage(int id)
	{
		performWritable("deleteImage", (db) =>
		{
			Image oldImage = GetOneImage(db, id);
			if (oldImage == null)
				return;

			// Delete the database row.
			db.Delete(TableName,
				"rowid=?",
				new string[] { id.ToString() });

			// Delete any external storage associated with it.
			string scaled = getScaledPath(oldImage);
			if (File.Exists(scaled))
				File.Delete(scaled);
		});
	}

	public void markSent(int id)
	{
		performWritable("markSent", (db) =>
		{
			ContentValues values = new ContentValues();
			values.Put(KeyState, (int)Image.State.Sent);
			db.Update(TableName, values, "rowid=?", new string[] { id.ToString() });
		});
	}

	public void updateComment(int id, string comment)
	{
		performWritable("updateComment", (db) =>
		{
			// FIXME? This is a potential race condition.
			Image image = GetOneImage(db, id);
			if (image == null)
				return;

			Image.State newState = image.state;

			switch (image.state)
			{
			case Image.State.Sent:
				newState = Image.State.CommentsUpdated;
				break;
			case Image.State.Downloaded:
				return;
			}

			ContentValues values = new ContentValues();
			values.Put(KeyState, (int)newState);
			values.Put(KeyComment, comment);
			db.Update(TableName, values, "rowid=?", new string[] { id.ToString() });
		});
	}
}

}

