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

/// <summary>
/// Represents the return from a Stream Contents REST call to the LifeStream server
/// </summary>
public class StreamContents
{
	public StreamContents(JsonValue source)
	{
		if ((bool)source["success"])
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
		else
		{
			this.error = (string)source["error"];
		}
	}

	/// <summary>
	/// The error message, if any; if no error was detected, this is null.
	/// </summary>
	public string error;

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

}

