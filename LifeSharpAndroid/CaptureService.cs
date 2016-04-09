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

