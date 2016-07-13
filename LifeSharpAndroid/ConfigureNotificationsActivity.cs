/*
	LifeStream - Instant Photo Sharing
	Copyright (C) 2016 Dove

	This code is licensed under the GPL v3 or later.
	Please see the file LICENSE for more info.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using Uri = Android.Net.Uri;

namespace LifeSharp
{

[Activity(Label = "ConfigureNotificationsActivity")]
public class ConfigureNotificationsActivity : Activity
{
	Settings _settings;

	class RequestCode {
		public const int DownloadSound = 100;
		public const int UploadSound = 200;
	}

	protected override void OnCreate(Bundle savedInstanceState)
	{
		base.OnCreate(savedInstanceState);

		SetContentView(Resource.Layout.ConfigureNotifications);

		_settings = new Settings(ApplicationContext);

		var uploadNotifications = FindViewById<CheckBox>(Resource.Id.cbUploadNotifications);
		uploadNotifications.Checked = _settings.uploadNotifications;
		uploadNotifications.CheckedChange += delegate {
			_settings.uploadNotifications = uploadNotifications.Checked;
			_settings.commit();
		};

		var downloadNotifications = FindViewById<CheckBox>(Resource.Id.cbDownloadNotifications);
		downloadNotifications.Checked = _settings.downloadNotifications;
		downloadNotifications.CheckedChange += delegate {
			_settings.downloadNotifications = downloadNotifications.Checked;
			_settings.commit();
		};

		var uploadVibration = FindViewById<CheckBox>(Resource.Id.cbUVibration);
		uploadVibration.Checked = _settings.uploadVibration;
		uploadVibration.CheckedChange += delegate {
			_settings.uploadVibration = uploadVibration.Checked;
			_settings.commit();
		};

		var downloadVibration = FindViewById<CheckBox>(Resource.Id.cbDVibration);
		downloadVibration.Checked = _settings.downloadVibration;
		downloadVibration.CheckedChange += delegate {
			_settings.downloadVibration = downloadVibration.Checked;
			_settings.commit();
		};

		var uploadSound = FindViewById<Button>(Resource.Id.btnUploadSound);
		uploadSound.Click += delegate {
			pickRingtone(RequestCode.UploadSound);
		};

		var downloadSound = FindViewById<Button>(Resource.Id.btnDownloadSound);
		downloadSound.Click += delegate {
			pickRingtone(RequestCode.DownloadSound);
		};

		var done = FindViewById<Button>(Resource.Id.btnOkay);
		done.Click += delegate {
			Finish();
		};
	}

	protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
	{
		Uri pickedUri;

		if (resultCode == Result.Ok)
		{
			switch(requestCode) {
				case RequestCode.DownloadSound:
					pickedUri = (Uri)data.GetParcelableExtra(RingtoneManager.ExtraRingtonePickedUri);
					_settings.downloadSound = pickedUri != null ? pickedUri.ToString() : null;
					_settings.commit();
					Console.WriteLine("Set downloadSound = " + _settings.downloadSound);
					break;
				case RequestCode.UploadSound:
					pickedUri = (Uri)data.GetParcelableExtra(RingtoneManager.ExtraRingtonePickedUri);
					_settings.uploadSound = pickedUri != null ? pickedUri.ToString() : null;
					_settings.commit();
					Console.WriteLine("Set uploadSound = " + _settings.uploadSound);
					break;
				default:
					// Don't know how to handle other kinds of requests
					return;
			}
		}
	}

	void pickRingtone(int requestCode)
	{
		string title;
		Uri uri;
		switch(requestCode) {
			case RequestCode.DownloadSound:
				title = "Download notification sound";
				uri = _settings.downloadSound != null ? Uri.Parse(_settings.downloadSound) : null;
				break;
			case RequestCode.UploadSound:
				title = "Upload notification sound";
				uri = _settings.uploadSound != null ? Uri.Parse(_settings.uploadSound) : null;
				break;
			default:
				// Don't know how to handle other kinds of requests
				return;
		}

		Intent intent = new Intent(RingtoneManager.ActionRingtonePicker);
		intent.PutExtra(RingtoneManager.ExtraRingtoneTitle, title);
		intent.PutExtra(RingtoneManager.ExtraRingtoneShowSilent, true);
		intent.PutExtra(RingtoneManager.ExtraRingtoneShowDefault, true);
		intent.PutExtra(RingtoneManager.ExtraRingtoneType, (int)RingtoneType.Notification);
		intent.PutExtra(RingtoneManager.ExtraRingtoneDefaultUri, RingtoneManager.GetDefaultUri(RingtoneType.Notification));
		Console.WriteLine("ExtraRingtoneExistingUri: " + uri);
		intent.PutExtra(RingtoneManager.ExtraRingtoneExistingUri, uri);
		StartActivityForResult(intent, requestCode);
	}
}

}

