/*
	LifeStream - Instant Photo Sharing
	Copyright (C) 2014-2016 Kayateia

	This code is licensed under the GPL v3 or later.
	Please see the file LICENSE for more info.
 */

using System;
using Android.Content;
using Android.Media;
using Android.Net;
using Android.App;
using Android.Util;

using Uri = Android.Net.Uri;
using Android.Provider;

namespace LifeSharp
{

/// <summary>
/// Utility methods for handling notifications of uploads and downloads.
/// </summary>
static public class Notifications
{
	const string LogTag = "LifeStream/Notifications";

	static Intent GetBaseIntent(Context context, string imagePath)
	{
		Intent intent;
		if (imagePath == null)
		{
			intent = new Intent(context, typeof(MainActivity));
		}
		else
		{
			intent = new Intent();
			intent.SetAction(Intent.ActionView);
			intent.SetDataAndType(Uri.Parse("file://" + imagePath), "image/*");
		}
		return intent;
	}

	static public void NotifyError(Context context, int id, bool replace, string tickerText, string title, string text)
	{
		Intent intent = GetBaseIntent(context, null);
		NotifyCommon(context, id, replace, tickerText, title, text, Android.Resource.Drawable.StatNotifyError, intent, true, RingtoneManager.GetDefaultUri(RingtoneType.Notification), false, null);
	}

	static public void NotifyDownload(Context context, int id, bool replace, string tickerText, string title, string text, Uri imageUri)
	{
		Settings settings = new Settings(context);
		Uri soundUri = settings.downloadSound != null ? Uri.Parse(settings.downloadSound) : null;
		Intent intent = GetBaseIntent(context, "");
		intent.SetDataAndType(imageUri, "image/*");
		NotifyCommon(context, id, replace, tickerText, title, text, Resource.Drawable.TransferDown, intent, true, soundUri, settings.downloadVibration, imageUri);
	}

	static public void NotifyUpload(Context context, int id, bool replace, string tickerText, string title, string text, string imagePath, string thumbnailPath)
	{
		Settings settings = new Settings(context);
		Uri soundUri = settings.uploadSound != null ? Uri.Parse(settings.uploadSound) : null;
		Intent intent = GetBaseIntent(context, imagePath);
		NotifyCommon(context, id, replace, tickerText, title, text, Resource.Drawable.TransferDown, intent, false, soundUri, settings.uploadVibration, Uri.Parse("file://" + thumbnailPath));
	}

	static public void NotifyCommon(Context context, int id, bool replace,
		string tickerText, String title, String text, int icon,
		Intent notificationIntent, bool showLed, Uri soundUri, bool vibration, Uri contentUri)
	{
		PendingIntentFlags flags = 0;
		if (replace)
			flags = PendingIntentFlags.CancelCurrent;
		PendingIntent contentIntent = PendingIntent.GetActivity(context, id, notificationIntent, flags);

		NotificationDefaults defaults = 0;
		// If vibration was requested, use the system-default vibration pattern
		defaults = (vibration) ? NotificationDefaults.Vibrate : 0;
		// If notification light was requested, use system-default colour and blink pattern
		defaults |= (showLed) ? NotificationDefaults.Lights : 0;

		Notification.Builder notificationBuilder = new Notification.Builder(context)
			.SetDefaults(defaults)
			.SetSound(soundUri)
			.SetContentTitle(title)
			.SetTicker(tickerText)
			.SetContentText(text)
			.SetContentIntent(contentIntent)
			.SetSmallIcon(icon, 0)
			.SetAutoCancel(true);

		// No notification light if not requested
		if (!showLed)
			notificationBuilder.SetLights(0, 0, 0);

		// No vibration pattern if not requested
		if (!vibration)
			notificationBuilder.SetVibrate(new long[] { 0, 0 });

		Notification notification = null;
		if (contentUri != null)
		{
			try
			{
				Notification.BigPictureStyle picBuilder = new Notification.BigPictureStyle(notificationBuilder)
					.BigPicture(MediaStore.Images.Media.GetBitmap(context.ContentResolver, contentUri));
				notification = picBuilder.Build();
			}
			catch (Exception e)
			{
				Log.Error(LogTag, "Unable to decode incoming image for big notification: " + e);
			}
		}

		if (notification == null)
			notification = notificationBuilder.Build();

		NotificationManager notificationManager = (NotificationManager)context.GetSystemService(Context.NotificationService);
		notificationManager.Notify(id, notification);
		Log.Warn(LogTag, title + ":" + text);
	}
}

}

