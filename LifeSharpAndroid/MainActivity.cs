/*
	LifeStream - Instant Photo Sharing
	Copyright (C) 2014-2016 Kayateia, Dove, and Deciare

	This code is licensed under the GPL v3 or later.
	Please see the file LICENSE for more info.
 */

using System;
using System.Linq;

using Android.App;
using Android.Support.V7.App;
using Android.Content;
using Android.Media;
using Android.Preferences;
using Android.Widget;
using Android.OS;
using Android.Views;
using System.Threading.Tasks;

using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace LifeSharp
{

[Activity(Label = "@string/app_name",	// Action bar title text
	MainLauncher = true,				// Launcher activates this activity
	Icon = "@drawable/icon")]			// Application icon
public class MainActivity : AppCompatActivity
{
	protected override async void OnCreate(Bundle bundle)
	{
		base.OnCreate(bundle);

		Log.SetLogger(new LogAndroid());

		// Set our view from the "main" layout resource
		SetContentView(Resource.Layout.Main);

		// Set support action bar based on toolbar from layout XML
		var mainToolbar = FindViewById<Toolbar>(Resource.Id.mainToolbar);
		SetSupportActionBar(mainToolbar);

		// Set flag to allow status bar colour to be managed by this activity.
		Window.SetFlags(WindowManagerFlags.DrawsSystemBarBackgrounds, WindowManagerFlags.DrawsSystemBarBackgrounds);

		Settings settings = new Settings(ApplicationContext);
		var statusLabel = FindViewById<TextView>(Resource.Id.loginStatus);
		var enabled = FindViewById<CheckBox>(Resource.Id.checkEnable);
		var buttonNotifications = FindViewById<Button>(Resource.Id.buttonNotifications);
		var login = FindViewById<TextView>(Resource.Id.editLogin);
		var password = FindViewById<TextView>(Resource.Id.editPassword);

		// First run initialisation.
		if (!settings.afterFirstRun) {
			// Set default values from NotificationPreferences.xml
			PreferenceManager.SetDefaultValues(ApplicationContext, Resource.Xml.NotificationPreferences, true);

			// If notification sound is null, set it to the system-default sound
			if (settings.downloadSound == null) {
				settings.downloadSound = RingtoneManager.GetDefaultUri(RingtoneType.Notification).ToString();
			}
			if (settings.uploadSound == null) {
				settings.uploadSound = RingtoneManager.GetDefaultUri(RingtoneType.Notification).ToString();
			}

			// Indicate that first run init is complete.
			settings.afterFirstRun = true;
			settings.commit();
		}

		// Load up defaults from Settings.
		enabled.Checked = settings.enabled;
		login.Text = settings.userName;
		password.Text = settings.password;

		if (settings.authToken.IsNullOrEmpty())
			statusLabel.Text = "Not logged in";
		else
			statusLabel.Text = "Logged in as " + settings.userName;

		// Wire up UI events.
		enabled.CheckedChange += delegate {
			bool oldSetting = settings.enabled;

			settings.enabled = enabled.Checked;
			settings.commit();

			if (!oldSetting)
				LifeSharpService.Start(this);
		};

		buttonNotifications.Click += delegate {
			Intent intent = new Intent(this, typeof(ConfigureNotificationsActivity));
			StartActivity(intent);
		};

		var defaultStreamSpinner = FindViewById<Spinner>(Resource.Id.defaultStream);
		await fillStreams(settings);
		defaultStreamSpinner.ItemSelected += delegate {
			if (_streamIds == null)
				return;

			settings.defaultStream = _streamIds[defaultStreamSpinner.SelectedItemPosition];
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
				if (result == null)
				{
					statusLabel.Text = "Log in failed";
					settings.authToken = null;
					enabled.Checked = false;
				}
				else
				{
					statusLabel.Text = "Logged in successfully";
					settings.authToken = result;
					enabled.Checked = true;
				}

				// Get our user ID.
				Protocol.LoginInfo loginInfo = await Network.GetLoginInfo(result, settings.userName);
				if (loginInfo.succeeded()) {
					settings.userId = loginInfo.id;
					// Commit so that user ID is available to upcoming fillStreams() call.
					settings.commit();
				}

				// Obtain list of streams owned by user.
				await fillStreams(settings);
				settings.commit();

				// This won't have already happened if we didn't have login info.
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

		ReceiveBoot.CompleteStartup(this, settings);
	}

	async Task fillStreams(Settings settings)
	{
		if (settings.userId == 0)
			return;

		Protocol.StreamList streams = await Network.GetStreamList(settings.authToken, (int)settings.userId);
		if (streams.succeeded())
		{
			// Fill the spinner itself.
			_streamIds = streams.streams.Select(x => x.id).ToArray();
			var adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem,
				streams.streams.Select(x => x.name).ToArray());
			adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
			var defaultStreamSpinner = FindViewById<Spinner>(Resource.Id.defaultStream);
			defaultStreamSpinner.Adapter = adapter;

			// If we already have a default stream, select it in the list.
			if (settings.defaultStream > 0)
			{
				// Figure out which index it was.
				int index = -1;
				for (int i=0; i<_streamIds.Length; ++i)
					if (_streamIds[i] == settings.defaultStream)
					{
						index = i;
						break;
					}

				if (index >= 0)
					defaultStreamSpinner.SetSelection(index);
			}
			else
			{
				// There was no default stream - set the first available one if we have one.
				if (streams.streams.Length > 0)
				{
					settings.defaultStream = streams.streams[0].id;
					settings.commit();
				}
			}
		}
	}

	int[] _streamIds;
}

}
