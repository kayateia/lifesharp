
using System;
using System.Collections.Generic;
using System.Json;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Util;
using System.Threading.Tasks;

namespace LifeSharp
{

[LifeSharpService]
public class StreamService : ILifeSharpService
{
	const string LogTag = "LifeSharp/StreamService";

	public StreamService()
	{
	}

	async Task doCheck(Context context, Settings settings)
	{
		Log.Info(LogTag, "Doing new files check on server");

		var json = await Network.HttpGetToJsonAsync(Settings.BaseUrl + "api/stream/1/contents", settings.authToken);
		Log.Info(LogTag, "Got back json: {0}", json);
	}

	public void start(Context context, Settings settings)
	{
		Log.Info(LogTag, "Started stream service");
	}

	public void stop(Context context, Settings settings)
	{
		Log.Info(LogTag, "Stopped stream service");
	}

	public void kick(Context context, Settings settings)
	{
		Task.Run(() => doCheck(context, settings).Wait());
	}
}

}
