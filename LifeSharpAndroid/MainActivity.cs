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

using System;

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

		LifeSharpService.Start(this);

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
			settings.enabled = enabled.Checked;
			settings.commit();
		};
		/*uploadNotifications.CheckedChange += delegate {
			settings.uploadNotifications = uploadNotifications.Checked;
			settings.commit();
		};*/
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
			}
			catch (Exception ex)
			{
				Log.Info("LifeSharp", "Can't contact server: {0}", ex);
				statusLabel.Text = "Exception during login";
			}
		};
	}
}

}
