namespace LifeSharp
{
using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Util;

public class CaptureService : Service
{
	const string LogTag = "LifeSharp/CaptureService";

	public CaptureService()
	{
	}

	public override IBinder OnBind(Intent intent)
	{
		return null;
	}

	public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
	{
		Log.Info(LogTag, "test");
		checkForNewImages();
		return StartCommandResult.NotSticky;
	}

	void checkForNewImages()
	{
		
	}
}

}

