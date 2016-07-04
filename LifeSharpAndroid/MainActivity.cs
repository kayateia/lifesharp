/*
	LifeStream - Instant Photo Sharing
	Copyright (C) 2014-2016 Kayateia

	This code is licensed under the GPL v3 or later.
	Please see the file LICENSE for more info.
 */

using System;
using System.Linq;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Util;

namespace LifeSharp
{

[Activity(Label = "LifeSharp", MainLauncher = true, Icon = "@drawable/icon")]
public class MainActivity : Activity
{
	protected override void OnCreate(Bundle bundle)
	{
		base.OnCreate(bundle);

		Log.SetLogger(new LogAndroid());

		// Set our view from the "main" layout resource
		SetContentView(Resource.Layout.Main);

		Settings settings = new Settings(this.ApplicationContext);
		var statusLabel = FindViewById<TextView>(Resource.Id.loginStatus);
		var enabled = FindViewById<CheckBox>(Resource.Id.checkEnable);
		// var uploadNotifications = FindViewById<CheckBox>(Resource.Id.checkUploadNotifications);
		var login = FindViewById<TextView>(Resource.Id.editLogin);
		var password = FindViewById<TextView>(Resource.Id.editPassword);

		// Load up defaults from Settings.
		enabled.Checked = settings.enabled;
		// uploadNotifications.Checked = 
		login.Text = settings.userName;
		password.Text = settings.password;

		// Wire up UI events.
		enabled.CheckedChange += delegate {
			bool oldSetting = settings.enabled;

			settings.enabled = enabled.Checked;
			settings.commit();

			if (!oldSetting)
				LifeSharpService.Start(this);
		};
		/*uploadNotifications.CheckedChange += delegate {
			settings.uploadNotifications = uploadNotifications.Checked;
			settings.commit();
		};*/

		var defaultStreamSpinner = FindViewById<Spinner>(Resource.Id.defaultStream);
		int[] streamIds = null;
		defaultStreamSpinner.ItemSelected += delegate {
			if (streamIds == null)
				return;

			settings.defaultStream = streamIds[defaultStreamSpinner.SelectedItemPosition];
			settings.commit();
			Log.Info("LifeSharp", "Setting new default stream to {0}", settings.defaultStream);
		};

		Button button = FindViewById<Button>(Resource.Id.buttonLogin);
		button.Click += async delegate {
			settings.userName = login.Text;
			settings.password = password.Text;
			settings.commit();
			Log.Info("LifeSharp", "Logging in with user {0} and password {1}", login.Text, password.Text);

			try
			{
				string result = await Network.Login(settings);
				statusLabel.Text = result;
				settings.authToken = result;
				settings.enabled = true;
				enabled.Checked = true;

				// Get our user ID.
				Protocol.LoginInfo loginInfo = await Network.GetLoginInfo(result, settings.userName);
				int? loginId = null;
				if (loginInfo.succeeded())
					loginId = loginInfo.id;

				Protocol.StreamList streams = await Network.GetStreamList(result, loginId);
				if (streams.error.IsNullOrEmpty())
				{
					streamIds = streams.streams.Select(x => x.id).ToArray();
					var adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem,
						streams.streams.Select(x => x.name).ToArray());
					defaultStreamSpinner.Adapter = adapter;

					if (streams.streams.Length > 0)
						settings.defaultStream = streams.streams[0].id;
				}

				settings.commit();

				LifeSharpService.Start(this);
			}
			catch (Exception ex)
			{
				Log.Info("LifeSharp", "Can't contact server: {0}", ex);
				statusLabel.Text = "Exception during login";
			}
		};
		button = FindViewById<Button>(Resource.Id.buttonGallery);
		button.Click += delegate(object sender, EventArgs e) {
			StartActivity(typeof(GalleryActivity));
		};

		if (Config.GcmNotificationKey != null)
		{
			if (!GCMRegistrationService.IsAvailable(this))
			{
				settings.enabled = false;
				enabled.Checked = false;
				return;
			}
			GCMRegistrationService.Start(this);
		}

		if (settings.enabled)
			LifeSharpService.Start(this);
	}
}

}
