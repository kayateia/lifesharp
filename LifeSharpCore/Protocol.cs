/*
	LifeStream - Instant Photo Sharing
	Copyright (C) 2014-2016 Kayateia

	This code is licensed under the GPL v3 or later.
	Please see the file LICENSE for more info.
 */

using System;
using System.Json;

namespace LifeSharp.Protocol
{

public class Basic
{
	public Basic(JsonValue source)
	{
		if (!Succeeded(source))
			this.error = GetError(source);
	}

	static public bool Succeeded(JsonValue source)
	{
		return (bool)source["success"] == true;
	}

	static public string GetError(JsonValue source)
	{
		return (string)source["error"];
	}

	public bool succeeded()
	{
		return this.error.IsNullOrEmpty();
	}

	/// <summary>
	/// The error message, if any; if no error was detected, this is null.
	/// </summary>
	public string error;
}

/// <summary>
/// Represents the return from a Stream Contents REST call to the LifeStream server
/// </summary>
public class StreamContents : Basic
{
	public StreamContents(JsonValue source)
		: base(source)
	{
		if (succeeded())
		{
			var imgsrc = source["images"];
			this.images = new Image[imgsrc.Count];
			for (int i=0; i<imgsrc.Count; ++i)
			{
				this.images[i] = new Image()
				{
					id = (int)imgsrc[i]["id"],
					filename = (string)imgsrc[i]["filename"],
					userLogin = (string)imgsrc[i]["userLogin"],
					uploadTime = Utils.UnixToDateTime((int)imgsrc[i]["uploadTime"]),
					comment = (string)imgsrc[i]["comment"]
				};
			}
		}
	}

	public Image[] images;

	public class Image
	{
		public int id;
		public string filename;
		public string userLogin;
		public DateTimeOffset uploadTime;
		public string comment;
	}
}

/// <summary>
/// The return from a StreamList REST call to the LifeStream server.
/// </summary>
public class StreamList : Basic
{
	public StreamList(JsonValue source)
		: base(source)
	{
		if (succeeded())
		{
			var streamSrc = source["streams"];
			this.streams = new Stream[streamSrc.Count];
			for (int i=0; i<streamSrc.Count; ++i)
			{
				this.streams[i] = new Stream()
				{
					id = (int)streamSrc[i]["id"],
					name = (string)streamSrc[i]["name"],
					permission = (int)streamSrc[i]["permission"],
					userLogin = (string)streamSrc[i]["userLogin"],
					userName = (string)streamSrc[i]["userName"]
				};
			}
		}
	}

	public Stream[] streams;

	public class Stream
	{
		public int id;
		public string name;
		public int permission;
		public string userLogin;
		public string userName;
	}
}

/// <summary>
/// The return from a "get user info by username" REST call to the LifeStream server.
/// </summary>
public class LoginInfo : Basic
{
	public LoginInfo(JsonValue source)
		: base(source)
	{
		if (succeeded())
		{
			id = (int)source["id"];
			login = (string)source["login"];
			name = (string)source["name"];
			email = (string)source["email"];
			isadmin = (bool)source["isAdmin"];
		}
	}

	public int id;
	public string login;
	public string name;
	public string email;
	public bool isadmin;
}

public class SubscriptionInfo : Basic
{
	public SubscriptionInfo(JsonValue source)
		: base(source)
	{
		if (succeeded())
		{
			var subSrc = source["subscriptions"];
			this.subscriptions = new Subscription[subSrc.Count];
			for (int i=0; i<subSrc.Count; ++i)
			{
				this.subscriptions[i] = new Subscription()
				{
					id = (int)subSrc[i]["streamid"],
					name = (string)subSrc[i]["streamName"],
					userId = (int)subSrc[i]["userid"],
					userLogin = (string)subSrc[i]["userLogin"],
					userName = (string)subSrc[i]["userName"]
				};
			}
		}
	}

	public Subscription[] subscriptions;

	public class Subscription
	{
		public int id;
		public string name;
		public int userId;
		public string userLogin;
		public string userName;
	}
}

/// <summary>
/// Available types of push services that we might register for. Note that these are
/// Google, Apple, and Microsoft, not Android, iOS, and Windows, because it's actually
/// possible to e.g. use GCM on iOS.
/// </summary>
public enum PushServiceType
{
	Google,
	Apple,
	Microsoft
}

}

