/*
	LifeStream - Instant Photo Sharing
	Copyright (C) 2014-2016 Kayateia

	This code is licensed under the GPL v3 or later.
	Please see the file LICENSE for more info.
 */

using System;
using Android.Content;

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
		public const string Name = "LifeSharpPrefs";
		public const string User = "username";
		public const string UserId = "userid";
		public const string Pass = "password";
		public const string Auth = "authToken";
		public const string LastCheck = "lastCheck";
		public const string Enabled = "enabled";
		public const string LastImageTimestamp = "lastImageTimestamp";
		public const string Vibration = "vibration";
		public const string Sounds = "sounds";
		public const string GcmId = "gcmid";
		public const string UploadNotifications = "uploadNotifications";
		public const string DefaultStream = "defaultStream";
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
		_settings = _context.GetSharedPreferences(Prefs.Name, FileCreationMode.Private);
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

	public string userName
	{
		get
		{
			return getString(Prefs.Name, "");
		}
		set
		{
			setString(Prefs.Name, value);
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
			edit();
			setLong(Prefs.LastImageTimestamp, value);
		}
	}

	public bool vibration
	{
		get
		{
			return getBool(Prefs.Vibration, true);
		}

		set
		{
			edit();
			setBool(Prefs.Vibration, value);
		}
	}

	public bool sounds
	{
		get
		{
			return getBool(Prefs.Sounds, true);
		}

		set
		{
			edit();
			setBool(Prefs.Sounds, value);
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
			edit();
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
			edit();
			setLong(Prefs.DefaultStream, value);
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
			edit();
			setBool(Prefs.UploadNotifications, value);
		}
	}

}

}

