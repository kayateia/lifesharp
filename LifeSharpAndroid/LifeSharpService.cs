/*
	LifeStream - Instant Photo Sharing
	Copyright (C) 2014-2016 Kayateia

	This code is licensed under the GPL v3 or later.
	Please see the file LICENSE for more info.
 */

using System;
using System.Linq;
using System.Reflection;
using Android.App;
using Android.Util;
using Android.Provider;
using Android.Content;
using System.Collections.Generic;

namespace LifeSharp
{

// Apply to a class to mark it as a LifeSharp service. These will be instantiated
// and the ILifeSharpService interface will be queried for their use.
sealed class LifeSharpServiceAttribute : Attribute
{
}

// Implemented by LifeSharp services.
interface ILifeSharpService
{
	void start(Context context, Settings settings);
	void stop(Context context, Settings settings);
	void kick(Context context, Settings settings);
}

[Service]
public class LifeSharpService : Service
{
	const string LogTag = "LifeSharp/LifeSharpService";
	bool _initted = false;
	ILifeSharpService[] _services;
	Settings _settings;

	static public void Start(Context context)
	{
		Log.Info(LogTag, "LifeSharp Service Start request");
		var lsServiceIntent = new Intent(context, typeof(LifeSharpService));
		context.StartService(lsServiceIntent);
	}

	public LifeSharpService()
	{
	}

	public override Android.OS.IBinder OnBind(Android.Content.Intent intent)
	{
		return null;
	}

	public override StartCommandResult OnStartCommand(Android.Content.Intent intent, StartCommandFlags flags, int startId)
	{
		// If we've already been initted, then just do a kick and bail.
		if (_initted)
		{
			Log.Info(LogTag, "Kicked LifeSharp service");
			kickServices(this.ApplicationContext, _settings);
			return StartCommandResult.Sticky;
		}

		Log.Info(LogTag, "Started LifeSharp service");
		_settings = new Settings(this.ApplicationContext);

		_services = getServices().ToArray();
		foreach (var service in _services)
			service.start(this.ApplicationContext, _settings);

		_initted = true;
		return StartCommandResult.Sticky;
	}

	/// <summary>
	/// Find all the types in this assembly with the LifeSharp service attribute.
	/// Create them and return the instances.
	/// </summary>
	IEnumerable<ILifeSharpService> getServices()
	{
		return typeof(LifeSharpService).Assembly.GetTypes()
			.Where(t => t.GetCustomAttribute<LifeSharpServiceAttribute>() != null)
			.Select(t => (ILifeSharpService)Activator.CreateInstance(t));
	}

	/// <summary>
	/// Gives all our services a swift kick in the okole. (i.e. tells them to do processing if they need to)
	/// </summary>
	void kickServices(Context context, Settings settings)
	{
		foreach (var service in _services)
			service.kick(context, settings);
	}
}
}

