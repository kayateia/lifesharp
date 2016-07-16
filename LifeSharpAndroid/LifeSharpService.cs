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
public interface ILifeSharpService
{
	void start(Context context, Settings settings);
	void stop(Context context, Settings settings);
	void kick(Context context, Settings settings);
}

[Service]
public class LifeSharpService : Service
{
	const string LogTag = "LifeSharp/LifeSharpService";

	static LifeSharpService s_instance;
	bool _initted = false;
	static ILifeSharpService[] s_servicesCache;
	Settings _settings;

	static ILifeSharpService[] s_services
	{
		get
		{
			if (s_servicesCache == null)
				s_servicesCache = GetServices().ToArray();
			return s_servicesCache;
		}
	}

	static public void Start(Context context)
	{
		Log.Info(LogTag, "LifeSharp Service Start request");

		var lsServiceIntent = new Intent(context, typeof(LifeSharpService));
		context.StartService(lsServiceIntent);
	}

	static public LifeSharpService Instance
	{
		get
		{
			return s_instance;
		}
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
		if (!_initted)
		{
			// This can be a program entry point.
			Log.SetLogger(new LogAndroid());

			Log.Info(LogTag, "Started LifeSharp service");
			s_instance = this;
			_settings = new Settings(this.ApplicationContext);

			foreach (var service in s_services)
				service.start(this.ApplicationContext, _settings);

			_initted = true;
		}
		else
		{
			foreach (var service in s_services)
				service.kick(this.ApplicationContext, _settings);
		}

		return StartCommandResult.Sticky;
	}

	/// <summary>
	/// Find all the types in this assembly with the LifeSharp service attribute.
	/// Create them and return the instances.
	/// </summary>
	static IEnumerable<ILifeSharpService> GetServices()
	{
		return typeof(LifeSharpService).Assembly.GetTypes()
			.Where(t => t.GetCustomAttribute<LifeSharpServiceAttribute>() != null)
			.Select(t => (ILifeSharpService)Activator.CreateInstance(t));
	}

	/// <summary>
	/// Gives a service a swift kick in the okole. (i.e. tells them to do processing if they need to)
	/// </summary>
	public void kickService<T>() where T : ILifeSharpService
	{
		foreach (var service in s_services)
			if (service is T)
				service.kick(this.ApplicationContext, _settings);
	}
}

}

