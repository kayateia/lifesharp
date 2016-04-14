using System;
using System.Json;

namespace LifeSharp.Protocol
{

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

