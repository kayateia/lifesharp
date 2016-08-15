/*
	LifeStream - Instant Photo Sharing
	Copyright (C) 2016 Deciare

	This code is licensed under the GPL v3 or later.
	Please see the file LICENSE for more info.
 */

using System;
using System.Collections.Generic;

using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Util;
using Android.Widget;
using Android.Views;

using Android.Support.V4.App;
using Android.Support.V4.View;

using Fragment = Android.Support.V4.App.Fragment;
using FragmentManager = Android.Support.V4.App.FragmentManager;
using Uri = Android.Net.Uri;

namespace LifeSharp
{

public class ImageFragment : Fragment
{
	public static readonly string KeyFileName = "filename";
	public static readonly string KeySourcePath = "sourcePath";
	public static readonly string KeyUploadTime = "uploadTime";
	public static readonly string KeyUserName = "userName";

	public const string LogTag = "ImageFragment";

	ImageView _imageView;
	WeakReference<Bitmap> _bitmapReference;

	public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
	{
		// Instantiate layout based on layout XML.
		View view = inflater.Inflate(Resource.Layout.SingleImageView, container, false);

		// Find the image view within the inflated layout.
		_imageView = view.FindViewById<ImageView>(Resource.Id.singleImageView);

		// Display image at the given URI.
		Log.Info(LogTag, "Allocating memory for bitmap of {0}", Arguments.GetString(KeyFileName));
		var bitmap = BitmapFactory.DecodeFile(Arguments.GetString(KeySourcePath));
		_imageView.SetImageBitmap(bitmap);
		_bitmapReference = new WeakReference<Bitmap>(bitmap);

		return view;
	}

	public override void OnDestroyView ()
	{
		base.OnDestroyView();

		Log.Info(LogTag, "Freeing memory used by bitmap of {0}", Arguments.GetString(KeyFileName));
		Bitmap bitmap;
		_imageView.SetImageBitmap(null);
		if (_bitmapReference != null && _bitmapReference.TryGetTarget(out bitmap) && !bitmap.IsRecycled)
		{
			bitmap.Recycle();
		}
	}
}

public class ImagePagerAdapter : FragmentStatePagerAdapter
{
	Context _context;
	List<Image> _images;
	ImageDatabaseAndroid _db;

	public ImagePagerAdapter(FragmentManager fm, Context context, string user) : base(fm)
	{
		_context = context;
		_db = ImageDatabaseAndroid.GetSingleton(context);
		_images = _db.getImagesByUser(user);
	}

	public override Fragment GetItem(int position)
	{
		Fragment fragment = new ImageFragment();

		// When creating a new ImageFragment, we need to provide it with
		// information to be displayed. Since we can't simply pass an Image
		// object, we have to pass the individual properties to be displayed.
		Bundle args = new Bundle();
		args.PutString(ImageFragment.KeyFileName, _images[position].filename);
		args.PutString(ImageFragment.KeySourcePath, _images[position].sourcePath);
		args.PutLong(ImageFragment.KeyUploadTime, Utils.DateTimeToUnix(_images[position].queueStamp));
		args.PutString(ImageFragment.KeyUserName, _images[position].userName);
		fragment.Arguments = args;
		return fragment;
	}

	/// <summary>
	/// Find the position (index) of an image with the given image ID within the
	/// data set associated with this adapter.
	/// </summary>
	/// <returns>Position within the data set.</returns>
	/// <param name="imageId">Image ID.</param>
	public int GetPositionByImageId(int imageId)
	{
		int retval = 0;

		if (imageId != 0)
		{
			for (var i = 0; i < _images.Count; i++)
			{
				if (_images[i].id == imageId)
				{
					retval = i;
					break;
				}
			}
		}

		return retval;
	}

	public override int Count
	{
		get
		{
			return _images.Count;
		}
	}
}

public class ImagePager : ViewPager
{
	public ImagePager(Context context, IAttributeSet attrs) : base(context, attrs)
	{
	}

	/// <summary>
	/// Sets the current image to be displayed based on image ID.
	/// </summary>
	/// <param name="imageId">Image ID.</param>
	public void SetCurrentImage(int imageId)
	{
		CurrentItem = ((ImagePagerAdapter)Adapter).GetPositionByImageId(imageId);
	}
}

}