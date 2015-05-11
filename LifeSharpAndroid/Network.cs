using System;
using System.Json;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Util;
using System.Net.Http;

namespace LifeSharp
{
static public class Network
{
	const string LogTag = "LifeSharp/Network";

	// Downloads the specified URL into the specified file.
	static public async Task HttpDownloadAsync(string url, string outputFilename)
	{
		Log.Info(LogTag, "Performing HttpDownloadAsync, URL {0}", url);

		using (HttpClient client = new HttpClient())
		using (HttpResponseMessage response = await client.GetAsync(url))
		using (HttpContent responseContent = response.Content)
		{
			// Copy the results out to the specified file.
			using (FileStream output = File.OpenWrite(outputFilename))
			{
				await responseContent.CopyToAsync(output);
			}
		}
	}

	// Downloads the specified URL into a JSON output.
	static public async Task<JsonValue> HttpPostGetJsonAsync(string url, IDictionary<string, string> param)
	{
		Log.Info(LogTag, "Performing HttpPostGetJsonAsync, URL {0}", url);

		using (HttpContent content = new FormUrlEncodedContent(param.ToArray()))
		using (HttpClient client = new HttpClient())
		using (HttpResponseMessage response = await client.PostAsync(url, content))
		using (HttpContent responseContent = response.Content)
		{
			// Use this stream to build a JSON document object.
			JsonValue jsonDoc = JsonObject.Load(await responseContent.ReadAsStreamAsync());
			Log.Info(LogTag, "Response: {0}", jsonDoc.ToString());

			return jsonDoc;
		}
	}

	// Logs into the LifeStream server and gets the JSON results of that request.
	static public async Task<string> Login(Settings settings)
	{
		var param = new Dictionary<string, string>()
		{
			{ "login", settings.userName },
			{ "pass", settings.password },
			{ "gcm", "" },
			{ "auth", "" }
		};
		JsonValue results = await HttpPostGetJsonAsync(Settings.BaseUrl + "login.php", param);

		string result = results["message"];
		Log.Info(LogTag, "Login results: {0}", result);

		return result;
	}
}
}

