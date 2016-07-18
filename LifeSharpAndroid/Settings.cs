/*
	LifeStream - Instant Photo Sharing
	Copyright (C) 2014-2016 Kayateia and Deciare

	This code is licensed under the GPL v3 or later.
	Please see the file LICENSE for more info.
 */

using System;
using Android.Content;
using Android.OS;
using Android.Preferences;

namespace LifeSharp
{

/// <summary>
/// Encapsulates our Android app settings
/// </summary>
public class Settings
{
	// These are the names of the pref items we'll be storing.
	class Prefs
	{
		public const string AfterFirstRun = "afterFirstRun";
		public const string User = "username";
		public const string UserId = "userid";
		public const string Pass = "password";
		public const string Auth = "authToken";
		public const string LastCheck = "lastCheck";
		public const string Enabled = "enabled";
		public const string LastImageTimestamp = "lastImageTimestamp";
		public const string GcmId = "gcmid";
		public const string DefaultStream = "defaultStream";
		public const string DownloadNotifications = "downloadNotifications";
		public const string DownloadSound = "downloadSound";
		public const string DownloadVibration = "downloadVibration";
		public const string UploadNotifications = "uploadNotifications";
		public const string UploadSound = "uploadSound";
		public const string UploadVibration = "uploadVibration";
	}

	// Default time, in seconds, to set the "last timestamp" if none exists. This will
	// cause us to auto-upload anything from one day ago.
	public const int DefaultDuration = 1 * 24 * 60 * 60;

	Context _context;
	ISharedPreferences _settings;
	ISharedPreferencesEditor _editor;


	// This allows us to swap out URLs later if we want to.
	static public string BaseUrl
	{
		get
		{
			return Config.BaseUrl;
		}
	}

	// Returns the Android ID of the device, if one exists; otherwise returns a new random GUID.
	static public string GetAndroidID(Context context)
	{
		string id = Android.Provider.Settings.Secure.GetString(context.ContentResolver,
			Android.Provider.Settings.Secure.AndroidId);
		if (string.IsNullOrEmpty(id))
			id = Guid.NewGuid().ToString("N");

		return id;
	}

	public Settings(Context context)
	{
		_context = context;
		// Get default shared preferences for this context. This is the SharedPreferences object
		// that will be used by all PreferenceFragment objects in this application.
		_settings = PreferenceManager.GetDefaultSharedPreferences(_context);
	}

	void edit()
	{
		if (_editor == null)
			_editor = _settings.Edit();
	}

	public void commit()
	{
		edit();
		_editor.Commit();
		_editor = null;
	}

	string getString(string id, string def)
	{
		return _settings.GetString(id, def);
	}
	void setString(string id, string val)
	{
		edit();
		_editor.PutString(id, val);
	}

	bool getBool(string id, bool def)
	{
		return _settings.GetBoolean(id, def);
	}
	void setBool(string id, bool val)
	{
		edit();
		_editor.PutBoolean(id, val);
	}

	long getLong(string id, long def)
	{
		return _settings.GetLong(id, def);
	}
	void setLong(string id, long val)
	{
		edit();
		_editor.PutLong(id, val);
	}

	public bool afterFirstRun
	{
		get
		{
			return getBool(Prefs.AfterFirstRun, false);
		}

		set
		{
			setBool(Prefs.AfterFirstRun, value);
		}
	}

	public string userName
	{
		get
		{
			return getString(Prefs.User, "");
		}
		set
		{
			setString(Prefs.User, value);
		}
	}

	public long userId
	{
		get
		{
			return getLong(Prefs.UserId, 0);
		}
		set
		{
			setLong(Prefs.UserId, value);
		}
	}

	public string password
	{
		get
		{
			return getString(Prefs.Pass, "");
		}
		set
		{
			setString(Prefs.Pass, value);
		}
	}

	public bool enabled
	{
		get
		{
			return getBool(Prefs.Enabled, false);
		}
		set
		{
			setBool(Prefs.Enabled, value);
		}
	}

	public string authToken
	{
		get
		{
			return getString(Prefs.Auth, "");
		}

		set
		{
			setString(Prefs.Auth, value);
		}
	}

	static readonly DateTimeOffset Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

	public DateTimeOffset lastCheck
	{
		get
		{
			return Epoch.AddSeconds(getLong(Prefs.LastCheck, 0));
		}

		set
		{
			setLong(Prefs.LastCheck, (long)((value - Epoch).TotalSeconds));
		}
	}

	public long lastImageProcessedTimestamp
	{
		get
		{	
			return getLong(Prefs.LastImageTimestamp, (long)((DateTimeOffset.UtcNow.Subtract(new TimeSpan(0, 0, DefaultDuration)) - Epoch).TotalSeconds));
		}

		set
		{
			setLong(Prefs.LastImageTimestamp, value);
		}
	}

	public string gcmId
	{
		get
		{
			return getString(Prefs.GcmId, "");
		}

		set
		{
			setString(Prefs.GcmId, value);
		}
	}

	public long defaultStream
	{
		get
		{
			return getLong(Prefs.DefaultStream, 0);
		}

		set
		{
			setLong(Prefs.DefaultStream, value);
		}
	}

	public bool downloadNotifications
	{
		get
		{
			return getBool(Prefs.DownloadNotifications, true);
		}

		set
		{
			setBool(Prefs.DownloadNotifications, value);
		}
	}

	public string downloadSound
	{
		get
		{
			return getString(Prefs.DownloadSound, null);
		}

		set
		{
			setString(Prefs.DownloadSound, value);
		}
	}

	public bool downloadVibration
	{
		get
		{
			return getBool(Prefs.DownloadVibration, true);
		}

		set
		{
			setBool(Prefs.DownloadVibration, value);
		}
	}

	public bool uploadNotifications
	{
		get
		{
			return getBool(Prefs.UploadNotifications, true);
		}

		set
		{
			setBool(Prefs.UploadNotifications, value);
		}
	}

	public string uploadSound
	{
		get
		{
			return getString(Prefs.UploadSound, null);
		}

		set
		{
			setString(Prefs.UploadSound, value);
		}
	}

	public bool uploadVibration
	{
		get
		{
			return getBool(Prefs.UploadVibration, true);
		}

		set
		{
			setBool(Prefs.UploadVibration, value);
		}
	}

}

}

