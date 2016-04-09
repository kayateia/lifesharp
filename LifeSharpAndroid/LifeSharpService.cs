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
		if (_initted)
			return StartCommandResult.Sticky;

		Log.Info(LogTag, "Started LifeSharp service");
		_settings = new Settings(this.ApplicationContext);

		_services = getServices().ToArray();
		foreach (var service in _services)
			service.start(this.ApplicationContext, _settings);

		_initted = true;
		return StartCommandResult.Sticky;
	}

	IEnumerable<ILifeSharpService> getServices()
	{
		// Find all the types in this assembly with the LifeSharp service attribute.
		// Create them and return the instances.
		return typeof(LifeSharpService).Assembly.GetTypes()
			.Where(t => t.GetCustomAttribute<LifeSharpServiceAttribute>() != null)
			.Select(t => (ILifeSharpService)Activator.CreateInstance(t));
	}
}
}

