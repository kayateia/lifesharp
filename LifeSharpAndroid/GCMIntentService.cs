using System;

using Android.App;
using Android.Content;
using Android.Gms.Common;
using Android.Gms.Gcm;
using Android.Gms.Gcm.Iid;
using Android.Widget;

using Task = System.Threading.Tasks.Task;

namespace LifeSharp
{

// For some reason, according to the Xamarin docs, these have to be three separate services. In
// Java they are one. This seems backwards to me.

[Service(Exported = false)]
public class GCMRegistrationService : IntentService
{
	const string LogTag = "LifeSharp/GCMRegistrationService";
	const string SenderKey = "<insert your GCM key here>";

	object s_locker = new object();

	public GCMRegistrationService() : base("GCMRegistrationService") { }

	static public bool IsAvailable(Context context)
	{
		int resultCode = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(context);
		if (resultCode != ConnectionResult.Success)
		{
			if (GoogleApiAvailability.Instance.IsUserResolvableError(resultCode))
			{
				Log.Error(LogTag, "Error connecting to GCM: {0}", GoogleApiAvailability.Instance.GetErrorString(resultCode));
				Toast.MakeText(context, GoogleApiAvailability.Instance.GetErrorString(resultCode), ToastLength.Short).Show();
			}
			else
			{
				Log.Error(LogTag, "Error connection to GCM: Unknown error {0}", resultCode);
				Toast.MakeText(context, "Can't connect to GCM", ToastLength.Short).Show();
			}

			return false;
		}

		return true;
	}

	static public void Start(Context context)
	{
		var intent = new Intent(context, typeof(GCMRegistrationService));
		context.StartService(intent);
	}

	protected override void OnHandleIntent(Intent intent)
	{
		try
		{
			Log.Info(LogTag, "Calling InstanceID.GetToken");
			lock (s_locker)
			{
				var instanceId = InstanceID.GetInstance(this);
				var token = instanceId.GetToken(SenderKey, GoogleCloudMessaging.InstanceIdScope, null);

				Log.Info(LogTag, "GCM Reg token: " + token);
				var settings = new Settings(this);
				settings.gcmId = token;
				settings.commit();

				// If we've already logged in, go ahead and register the device with the server. Otherwise,
				// it'll happen when we log in later.
				if (!settings.authToken.IsNullOrEmpty())
					Task.Run(() => Network.RegisterDevice(settings.authToken, Protocol.PushServiceType.Google, Settings.GetAndroidID(this), token).Wait());
			}
		}
		catch (Exception e)
		{
			Log.Error(LogTag, "Can't register GCM: " + e);
		}
	}
}

[Service(Exported = false), IntentFilter(new [] { "com.google.android.gms.iid.InstanceID" })]
public class GCMRefreshService : InstanceIDListenerService
{
	const string LogTag = "LifeSharp/GCMRefreshService";

	public override void OnTokenRefresh()
	{
		Log.Info(LogTag, "GCM Requested a token refresh");
		GCMRegistrationService.Start(this);
	}
}

[Service(Exported = false), IntentFilter(new[] { "com.google.android.c2dm.intent.RECEIVE" })]
public class GCMIntentService : GcmListenerService
{
	const string LogTag = "LifeSharp/GCMIntentService";

	public override void OnMessageReceived(string from, Android.OS.Bundle data)
	{
		Log.Info(LogTag, "Received intent from " + from + ": " + data.GetString("message"));
	}
}

}
