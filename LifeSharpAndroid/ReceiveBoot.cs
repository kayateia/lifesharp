/*
	LifeStream - Instant Photo Sharing
	Copyright (C) 2014-2016 Dove and Kayateia

	This code is licensed under the GPL v3 or later.
	Please see the file LICENSE for more info.
 */

using System;
using Android.App;
using Android.Content;

namespace LifeSharp
{

[BroadcastReceiver]
[IntentFilter(new[] { Android.Content.Intent.ActionBootCompleted }, Categories = new[] { Android.Content.Intent.CategoryDefault })]
public class ReceiveBoot : BroadcastReceiver
{
	public override void OnReceive(Context context, Intent intent)
	{
		if (intent.Action == Android.Content.Intent.ActionBootCompleted)
		{
			// This may happen before our MainActivity is loaded, so we have to do it also.
			Log.SetLogger(new LogAndroid());

			Settings settings = new Settings(context);
			CompleteStartup(context, settings);
		}
	}

	static public void CompleteStartup(Context context, Settings settings)
	{
		if (Config.GcmNotificationKey != null)
		{
			if (!GCMRegistrationService.IsAvailable(context))
			{
				settings.enabled = false;
				settings.commit();
				return;
			}
			GCMRegistrationService.Start(context);
		}

		if (settings.enabled)
			LifeSharpService.Start(context);		
	}
}

}
