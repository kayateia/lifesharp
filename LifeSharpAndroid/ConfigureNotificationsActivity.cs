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

namespace LifeSharp
{

[Activity(Label = "ConfigureNotificationsActivity")]
public class ConfigureNotificationsActivity : Activity
{
	protected override void OnCreate(Bundle savedInstanceState)
	{
		base.OnCreate(savedInstanceState);

		SetContentView(Resource.Layout.ConfigureNotifications);

		Settings settings = new Settings(this);

		var uploadNotifications = FindViewById<CheckBox>(Resource.Id.cbUploadNotifications);
		uploadNotifications.Checked = settings.uploadNotifications;
		uploadNotifications.CheckedChange += delegate {
			settings.uploadNotifications = uploadNotifications.Checked;
			settings.commit();
		};

		var uploadVibrate = FindViewById<CheckBox>(Resource.Id.cbUVibration);
		uploadVibrate.CheckedChange += delegate {
		};

		var uploadSound = FindViewById<Button>(Resource.Id.btnUploadSound);
		uploadSound.Click += delegate {
			pickRingtone();
		};

		var downloadNotifications = FindViewById<CheckBox>(Resource.Id.cbDownloadNotifications);
		downloadNotifications.Checked = true;
		downloadNotifications.CheckedChange += delegate {
		};

		var downloadVibrate = FindViewById<CheckBox>(Resource.Id.cbDVibration);
		downloadVibrate.CheckedChange += delegate {
		};

		var downloadSound = FindViewById<Button>(Resource.Id.btnDownloadSound);
		downloadSound.Click += delegate {
			pickRingtone();
		};

		var done = FindViewById<Button>(Resource.Id.btnOkay);
		done.Click += delegate {
			Finish();
		};
	}

	void pickRingtone()
	{
		Intent intent = new Intent(RingtoneManager.ActionRingtonePicker);
		intent.PutExtra(RingtoneManager.ExtraRingtoneTitle, "Select notification sound:");
		intent.PutExtra(RingtoneManager.ExtraRingtoneShowSilent, true);
		intent.PutExtra(RingtoneManager.ExtraRingtoneShowDefault, true);
		intent.PutExtra(RingtoneManager.ExtraRingtoneType, (int)RingtoneType.Notification);
		intent.PutExtra(RingtoneManager.ExtraRingtoneExistingUri, RingtoneManager.GetDefaultUri(RingtoneType.Notification));
		StartActivityForResult(intent, 0);
	}
}

}

