/*
	LifeStream - Instant Photo Sharing
	Copyright (C) 2014-2016 Kayateia

	This code is licensed under the GPL v3 or later.
	Please see the file LICENSE for more info.
 */

using System;
using System.Json;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;

namespace LifeSharp
{

/// <summary>
/// Network protocol utilities, for talking to the LifeStream server
/// </summary>
static public class Network
{
	const string LogTag = "LifeSharp/Network";

	/// <summary>
	/// Downloads the specified URL into the specified file.
	/// </summary>
	/// <param name="url">The URL to download</param>
	/// <param name="token">The authentication token to use</param>
	/// <param name="outputFilename">Where to write the file</param>
	static public async Task HttpDownloadAsync(string url, string token, string outputFilename)
	{
		Log.Info(LogTag, "Performing HttpDownloadAsync, URL {0}", url);

		using (HttpClient client = new HttpClient())
		{
			var request = new HttpRequestMessage()
			{
				RequestUri = new System.Uri(url),
				Method = HttpMethod.Get
			};
			if (token != null)
				request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

			using (HttpResponseMessage response = await client.SendAsync(request))
			using (HttpContent responseContent = response.Content)
			{
				// Copy the results out to the specified file.
				using (FileStream output = File.OpenWrite(outputFilename))
				{
					await responseContent.CopyToAsync(output);
				}
			}
		}
	}

	/// <summary>
	/// Downloads the specified URL into a JSON output.
	/// </summary>
	/// <returns>The JSON data</returns>
	/// <param name="url">The URL to POST to</param>
	/// <param name="token">The authentication token to use</param>
	/// <param name="param">POST parameters</param>
	static public async Task<JsonValue> HttpPostToJsonAsync(string url, string token, IDictionary<string, string> param)
	{
		Log.Info(LogTag, "Performing HttpPostToJsonAsync, URL {0}", url);

		using (HttpContent content = new FormUrlEncodedContent(param.ToArray()))
		using (HttpClient client = new HttpClient())
		{
			var request = new HttpRequestMessage()
			{
				RequestUri = new System.Uri(url),
				Method = HttpMethod.Post,
				Content = content
			};
			if (token != null)
				request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

			using (HttpResponseMessage response = await client.SendAsync(request))
			using (HttpContent responseContent = response.Content)
			{
				// Use this stream to build a JSON document object.
				JsonValue jsonDoc = JsonObject.Load(await responseContent.ReadAsStreamAsync());
				Log.Info(LogTag, "Response: {0}", jsonDoc.ToString());

				return jsonDoc;
			}
		}
	}

	/// <summary>
	/// Downloads the specified URL into a JSON output.
	/// </summary>
	/// <returns>The JSON data</returns>
	/// <param name="url">The URL to download</param>
	/// <param name="token">The authentication token to use</param>
	static public async Task<JsonValue> HttpGetToJsonAsync(string url, string token)
	{
		Log.Info(LogTag, "Performing HttpGetToJsonAsync, URL {0}", url);

		using (HttpClient client = new HttpClient())
		{
			var request = new HttpRequestMessage()
			{
				RequestUri = new System.Uri(url),
				Method = HttpMethod.Get
			};
			if (token != null)
				request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

			using (HttpResponseMessage response = await client.SendAsync(request))
			using (HttpContent responseContent = response.Content)
			{
				// Use this stream to build a JSON document object.
				JsonValue jsonDoc = JsonObject.Load(await responseContent.ReadAsStreamAsync());
				Log.Info(LogTag, "Response: {0}", jsonDoc.ToString());

				return jsonDoc;
			}
		}
	}

	/// <summary>
	/// Logs into the LifeStream server and gets the JSON results of that request. Returns a token as a string
	/// or null if the login failed.
	/// </summary>
	static public async Task<string> Login(Settings settings)
	{
		var param = new Dictionary<string, string>()
		{
			{ "login", settings.userName },
			{ "password", settings.password }
		};
		JsonValue results = await HttpPostToJsonAsync(Settings.BaseUrl + "api/user/login", null, param);

		if (Protocol.Basic.Succeeded(results))
		{
			string token = results["token"];
			Log.Info(LogTag, "Login results: {0}", token);

			return token;
		}
		else
		{
			Log.Info(LogTag, "Login failed: {0}", Protocol.Basic.GetError(results));
			return null;
		}
	}

	/// <summary>
	/// Registers the device with the LifeStream server for push notifications.
	/// </summary>
	/// <returns>True if we succeeded</returns>
	/// <param name="authToken">Auth token from settings</param>
	/// <param name="serviceType">Service type</param>
	/// <param name="deviceId">Device unique identifier</param>
	/// <param name="pushToken">Push service token</param>
	static public async Task<bool> RegisterDevice(string authToken, Protocol.PushServiceType serviceType, string deviceId, string pushToken)
	{
		var param = new Dictionary<string, string>()
		{
			{ "id", deviceId },
			{ "type", serviceType.ToString().ToLowerInvariant() },
			{ "token", pushToken }
		};
		JsonValue results = await HttpPostToJsonAsync(Settings.BaseUrl + "api/user/register-device", authToken, param);

		if (Protocol.Basic.Succeeded(results))
		{
			Log.Info(LogTag, "Successfully registered device with server");
			return true;
		}
		else
		{
			Log.Info(LogTag, "Device registration failed: {0}", Protocol.Basic.GetError(results));
			return false;
		}
	}
}

}
