using System;
using Android.Database.Sqlite;
using Android.Content;
using Android.Database;
using Android.Util;

namespace LifeSharp
{

// Implementation of IImageQueue that works with Android SQLite.
public class ImageQueueAndroid : SQLiteOpenHelper, IImageQueue
{
	const string LogTag = "LifeSharp/ImageQueue";

	const int IQDatabaseVersion = 1;
	const string IQDatabaseName = "LifeSharpImageQueue";
	const string TableQueue = "queue";

	const string KeyID = "id";
	const string KeyName = "name";
	const string KeyTimestamp = "timestamp";
	const string KeyQueuestamp = "queuestamp";

	// This has to be done through the same object to get database locking, unfortunately.
	static object s_lock = new object();
	static ImageQueueAndroid s_global = null;
	static public ImageQueueAndroid GetSingleton(Context context)
	{
		lock (s_lock)
		{
			if (s_global == null)
				s_global = new ImageQueueAndroid(context.ApplicationContext);
			return s_global;
		}
	}

	public ImageQueueAndroid(Context context)
		: base(context, IQDatabaseName, null, IQDatabaseVersion)
	{
	}

	public override void OnCreate(SQLiteDatabase db)
	{
		string createdProcessedTable = "create table " + TableQueue + "("
			+ KeyID + " integer primary key," + KeyName + " text,"
			+ KeyTimestamp + " integer,"
			+ KeyQueuestamp + " integer"
			+ ")";
		db.ExecSQL(createdProcessedTable);
	}

	public override void OnUpgrade(SQLiteDatabase db, int oldVersion, int newVersion)
	{
		// Drop older table if existed
		db.ExecSQL("drop table if exists " + TableQueue);

		// Create tables again
		OnCreate(db);
	}

	public void addToQueue(string imageName)
	{
		using (SQLiteDatabase db = this.WritableDatabase)
		{
			try
			{
				// Check if it's there already.
				ICursor cursor = db.Query(TableQueue,
					new string[] { KeyName },
					KeyName + "=?",
					new string[] { imageName },
					null, null, null, null);
				if (cursor.MoveToFirst())
					return;

				// Go ahead and insert.
				ContentValues values = new ContentValues();
				values.Put(KeyName, imageName);
				values.Put(KeyTimestamp, Utils.UnixNow());
				values.Put(KeyQueuestamp, Utils.UnixNow());
				db.Insert(TableQueue, null, values);
			}
			catch (SQLiteException e)
			{
				Log.Error(LogTag, "SQL exception during addToQueue: {0}", e);
			}
		}
	}

	public Image[] getItemsToProcess()
	{
		using (SQLiteDatabase db = this.ReadableDatabase)
		{
			try {
				ICursor cursor = db.Query(TableQueue,
					new String[] { KeyID, KeyName, KeyTimestamp, KeyQueuestamp },
					null, null,
					null, null, KeyQueuestamp);
				if (!cursor.MoveToFirst())
					return null;

				Image[] rv = new Image[cursor.Count];
				for (int i=0; i<cursor.Count; ++ i)
				{
					rv[i] = new Image()
					{
						id = cursor.GetInt(0),
						pathname = cursor.GetString(1),
						timestamp = cursor.GetInt(2),
						queuestamp = cursor.GetInt(3)
					};
					cursor.MoveToNext();
				}

				return rv;
			}
			catch (SQLiteException e)
			{
				Log.Error(LogTag, "SQL exception during getItemToProcess: {0}", e);
				return null;
			}
		}
	}

	public void markSkipped(int id)
	{
		using (SQLiteDatabase db = this.WritableDatabase)
		{
			ContentValues values = new ContentValues();
			values.Put(KeyQueuestamp, Utils.DateTimeToUnix(DateTimeOffset.UtcNow));
			db.Update(TableQueue, values, KeyID + "=?", new string[] { id.ToString() });
		}
	}

	public void markProcessed(int id)
	{
		using (SQLiteDatabase db = this.WritableDatabase)
		{
			db.Delete(TableQueue,
				KeyID + "=?",
				new string[] { id.ToString() });
		}
	}
}

}

