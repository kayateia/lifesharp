using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using Android.Support.V7.Widget;

namespace LifeSharp
{

public class RecyclerGalleryView
{
	public abstract class Adapter : RecyclerView.Adapter
	{
		const string LogTag = "Gallery.Adapter";

		protected Activity activity;
		protected int screenWidth;
		protected List<ViewHolder> viewHolders;

		public Adapter(Activity activity)
		{
			this.activity = activity;
			viewHolders = new List<ViewHolder>();

			// The target width of each displayed image shoud be proportional to
			// a given percentage of the screen size.
			IWindowManager wm = activity.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();
			Display display = wm.DefaultDisplay;
			Point size = new Point();
			display.GetSize(size);
			screenWidth = size.X;
		}

		public void FreeAllBitmaps()
		{
			if (viewHolders.Count == 0)
			{
				Log.Warn(LogTag, "viewHolders list is empty. Is OnCreateViewHolder() appending to the list?");
			}

			// Iterate through all ViewHolders created by this adapter, and free the
			// bitmap that was allocated by each.
			foreach (ViewHolder holder in viewHolders)
			{
				holder.FreePreviousBitmap();
			}
		}
	}


	public abstract class ViewHolder : RecyclerView.ViewHolder
	{
		const string LogTag = "Gallery.ViewHolder";

		protected Activity activity;
		protected int targetWidth;
		protected WeakReference<Bitmap> bitmapReference;
		protected ImageView imageView;

		protected ViewHolder(Activity activity, View itemView, int targetWidth) : base(itemView)
		{
			this.activity = activity;
			this.targetWidth = targetWidth;
		}

		protected int CalculateInSampleSize(BitmapFactory.Options options)
		{
			int width = options.OutWidth / 2;
			int inSampleSize = 1;

			// Iterate until we find a power of 2 that, when dividing the bitmap's
			// width by that value, would cause the resultant width to be smaller
			// than the target width. Stop just before that happens.
			while (width / inSampleSize > targetWidth)
			{
				inSampleSize *= 2;
			}

			return inSampleSize;
		}

		protected Bitmap GetBitmapForImage(Image image)
		{
			// Obtain dimensions of image.
			var options = new BitmapFactory.Options();
			options.InJustDecodeBounds = true;
			BitmapFactory.DecodeFile(image.sourcePath, options);

			// Generate a scaled-down bitmap of the image to put in the view.
			options.InJustDecodeBounds = false;
			options.InSampleSize = CalculateInSampleSize(options);
			Bitmap bitmap = BitmapFactory.DecodeFile(image.sourcePath, options);

			// Save a reference to the bitmap so it can be manually recycled later.
			bitmapReference = new WeakReference<Bitmap>(bitmap);

			return bitmap;
		}

		public void FreePreviousBitmap()
		{
			Debug.Assert(imageView != null, "imageView must be set by subclass constructor");

			// If a bitmap was previously allocated for this view holder, free
			// the memory that was used by that bitmap.
			Bitmap bitmap;
			if (bitmapReference != null && bitmapReference.TryGetTarget(out bitmap))
			{
				imageView.SetImageBitmap(null);
				try
				{
					bitmap.Recycle();
				}
				catch (ObjectDisposedException e)
				{
					// In some cases, the bitmap was already garbage collected
					// by the time we tried to manually free it.
					Log.Warn(LogTag, "Bitmap was already freed: {0}", e);
				}
			}
		}

		public void SetImageViewTarget(Image image)
		{
			// When a different image is assigned to this view holder, the
			// previous image's bitmap may not be freed immediately, resulting
			// in the possibility of an out-of-memory error if SetImage() is
			// called rapidly. Free the memory used by the previous image before
			// attempting to allocate a bitmap for the next image.
			FreePreviousBitmap();

			// Decode image into bitmap in separate thread, to avoid locking up
			// the UI thread.
			ThreadPool.QueueUserWorkItem(delegate {
				Bitmap bitmap = GetBitmapForImage(image);

				// Once the bitmap decode is complete, assign it to the ImageView.
				// This must be done on the UI thread.
				activity.RunOnUiThread(delegate {
					// When scrolling rapidly, it's possible that the bitmap has
					// already been recycled by a subsequent call to
					// SetImageViewTarget() before we get around to displaying
					// it. Ensure that it hasn't been.
					if (bitmap != null && !bitmap.IsRecycled)
					{
						// Update views based on image properties.
						imageView.SetImageBitmap(bitmap);
					}
				});
			});
		}
	}
}

}