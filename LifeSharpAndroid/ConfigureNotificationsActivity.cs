/*
	LifeStream - Instant Photo Sharing
	Copyright (C) 2016 Dove

	This code is licensed under the GPL v3 or later.
	Please see the file LICENSE for more info.
 */

using System;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Preferences;
using Android.Provider;

using Uri = Android.Net.Uri;

namespace LifeSharp
{

public class NotificationPreferencesFragment : PreferenceFragment
{
	Context _context;
	Settings _settings;

	string ConvertSoundUriToTitle(string soundUri)
	{
		string title = null;

		if (soundUri == null) {
			return null;
		}

		// Query the ContentProvider for information about the Uri.
		using (var cursor = _context.ContentResolver.Query(
			Uri.Parse(soundUri),
			new String[] { MediaStore.MediaColumns.Title },
			null,
			null,
			null))
		{
			// We want the sound file's title metadata.
			var titleColumn = cursor.GetColumnIndexOrThrow(MediaStore.MediaColumns.Title);
			if (cursor != null && cursor.MoveToFirst()) {
				title = cursor.GetString(titleColumn);
			}
		}

		return title;
	}

	void OnRingtoneChange(Object sender, Preference.PreferenceChangeEventArgs e)
	{
		((Preference)sender).Summary = ConvertSoundUriToTitle(e.NewValue.ToString());
	}

	public override void OnAttach(Activity activity)
	{
		base.OnAttach(activity);

		_context = activity;
		_settings = new Settings(_context);
	}

	public override void OnCreate(Bundle savedInstanceState)
	{
		base.OnCreate (savedInstanceState);

		// Read preferences to display from XML file
		AddPreferencesFromResource(Resource.Xml.NotificationPreferences);

		// Set summary text for notification sound preferences on page load.
		var downloadSoundPreference = FindPreference("downloadSound");
		downloadSoundPreference.Summary = ConvertSoundUriToTitle(_settings.downloadSound);
		var uploadSoundPreference = FindPreference("uploadSound");
		uploadSoundPreference.Summary = ConvertSoundUriToTitle(_settings.uploadSound);

		// Update summary text after user selects different notification sound.
		downloadSoundPreference.PreferenceChange += OnRingtoneChange;
		uploadSoundPreference.PreferenceChange += OnRingtoneChange;
	}
}

[Activity(Label = "ConfigureNotificationsActivity")]
public class ConfigureNotificationsActivity : Activity
{
	protected override void OnCreate(Bundle savedInstanceState)
	{
		base.OnCreate(savedInstanceState);

		// Use PreferenceFragment generated from XML file
		FragmentManager.BeginTransaction().Replace(Android.Resource.Id.Content,
			new NotificationPreferencesFragment()).Commit();
	}
}

}

