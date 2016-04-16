using System;
using Android.Content;
using Android.Net;
using Android.App;
using Android.Util;

using Uri = Android.Net.Uri;
using Android.Provider;

namespace LifeSharp
{

public class Notifications
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
		NotifyCommon(context, id, replace, tickerText, title, text, Android.Resource.Drawable.StatNotifyError, intent, true, null);
	}

	static public void NotifyDownload(Context context, int id, bool replace, string tickerText, string title, string text, Uri imageUri)
	{
		Intent intent = GetBaseIntent(context, "");
		intent.SetDataAndType(imageUri, "image/*");
		NotifyCommon(context, id, replace, tickerText, title, text, Resource.Drawable.TransferDown, intent, true, imageUri);
	}

	static public void NotifyUpload(Context context, int id, bool replace, string tickerText, string title, string text, string imagePath, string thumbnailPath)
	{
		Intent intent = GetBaseIntent(context, imagePath);
		NotifyCommon(context, id, replace, tickerText, title, text, Resource.Drawable.TransferDown, intent, false, Uri.Parse("file://" + thumbnailPath));
	}

	static public void NotifyCommon(Context context, int id, bool replace,
		string tickerText, String title, String text, int icon,
		Intent notificationIntent, bool showLed, Uri contentUri)
	{
		PendingIntentFlags flags = 0;
		if (replace)
			flags = PendingIntentFlags.CancelCurrent;
		PendingIntent contentIntent = PendingIntent.GetActivity(context, id, notificationIntent, flags);

		Settings settings = new Settings(context);
		bool vibration = settings.vibration;
		bool soundsEnabled = settings.sounds;
		NotificationDefaults defaults = 0;
		if (vibration && soundsEnabled && showLed)
			defaults = NotificationDefaults.All;
		else
		{
			defaults = (soundsEnabled) ? NotificationDefaults.Sound : 0;
			defaults |= (vibration) ? NotificationDefaults.Vibrate : 0;
			defaults |= (showLed) ? NotificationDefaults.Lights : 0;
		}

		Notification.Builder notificationBuilder = new Notification.Builder(context)
			.SetDefaults(defaults)
			.SetContentTitle(title)
			.SetTicker(tickerText)
			.SetContentText(text)
			.SetContentIntent(contentIntent)
			.SetSmallIcon(icon, 0)
			.SetAutoCancel(true);

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

