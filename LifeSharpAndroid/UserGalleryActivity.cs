/*
	LifeStream - Instant Photo Sharing
	Copyright (C) 2014-2016 Deciare

	This code is licensed under the GPL v3 or later.
	Please see the file LICENSE for more info.
 */

using System;
using System.Collections.Generic;
using System.Threading;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;

using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace LifeSharp
{

public class UserGalleryAdapter : RecyclerView.Adapter
{
	Activity _activity;
	List<Image> _images;
	ImageDatabaseAndroid _db;
	List<ViewHolder> _viewHolders;

	public class ViewHolder : RecyclerView.ViewHolder
	{
		Activity _activity;
		int _targetWidth;
		WeakReference<Bitmap> _bitmapReference;
		public Image image { get; private set; }
		public DynamicImageView imageView { get; private set; }
		public TextView badgesView { get; private set; }
		public TextView timeView { get; private set; }

		public ViewHolder(Activity activity, RelativeLayout itemView, EventHandler<int> onClick) : base(itemView)
		{
			_activity = activity;

			// Clicking on the item view triggers the adapter's Click event.
			itemView.Click += delegate {
				onClick(this, image.id);
			};

			// Attach member variables to child views of this item view.
			imageView = itemView.FindViewById<DynamicImageView>(Resource.Id.galleryTileImage);
			badgesView = itemView.FindViewById<TextView>(Resource.Id.galleryTileBadges);
			timeView = itemView.FindViewById<TextView>(Resource.Id.galleryTileTime);

			// UserGalleryActivity shows 2 columns, so the target size should be half the width of the display.
			IWindowManager wm = activity.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();
			Display display = wm.DefaultDisplay;
			Point size = new Point();
			display.GetSize(size);
			_targetWidth = size.X / 2;
		}

		public int CalculateInSampleSize(BitmapFactory.Options options)
		{
			int width = options.OutWidth / 2;
			int inSampleSize = 1;

			// Iterate until we find a power of 2 that, when dividing the bitmap's
			// width by that value, would cause the resultant width to be smaller
			// than the target width. Stop just before that happens.
			while (width / inSampleSize > _targetWidth)
			{
				inSampleSize *= 2;
			}

			return inSampleSize;
		}

		public void FreePreviousBitmap()
		{
			// If a bitmap was previously allocated for this view holder, free
			// the memory that was used by that bitmap.
			Bitmap bitmap;
			if (_bitmapReference != null && _bitmapReference.TryGetTarget(out bitmap))
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
					Log.Warn("UserGalleryAdapter", "Bitmap was already freed: {0}", e);
				}
			}
		}

		public void SetImage(Image image)
		{
			// When a different image is assigned to this view holder, the
			// previous image's bitmap may not be freed immediately, resulting
			// in the possibility of an out-of-memory error if SetImage() is
			// called rapidly. Free the memory used by the previous image before
			// attempting to allocate a bitmap for the next image.
			FreePreviousBitmap();

			// Decode image into bitmap in separate thread, to avoid locking up
			// the UI thread.
			ThreadPool.QueueUserWorkItem(delegate(object state) {
				this.image = image;

				// Obtain dimensions of image.
				var options = new BitmapFactory.Options();
				options.InJustDecodeBounds = true;
				BitmapFactory.DecodeFile(image.sourcePath, options);

				// Generate a scaled-down bitmap of the image to put in the view.
				options.InJustDecodeBounds = false;
				options.InSampleSize = CalculateInSampleSize(options);
				// Use a weak reference to avoid preventing the ImageView from
				// being garbage collected if the view becomes no longer needed
				// while the bitmap is still becoming decoded.
				var viewReference = new WeakReference<DynamicImageView>(imageView);
				Bitmap bitmap = BitmapFactory.DecodeFile(image.sourcePath, options);

				// Once the bitmap decode is complete, assign it to the ImageView.
				// This must be done on the UI thread.
				_activity.RunOnUiThread(delegate() {
					// Update views based on image properties.
					if (viewReference != null && bitmap != null)
					{
						// If the reference to the image view is still valid, then
						// other views in this view holder are also still valid.
						// Assign appropriate values to all views in this vie wholder.
						DynamicImageView view;
						if (viewReference.TryGetTarget(out view))
						{
							_bitmapReference = new WeakReference<Bitmap>(bitmap);
							view.SetImageBitmap(bitmap);
							badgesView.Text = image.comment.IsNullOrEmpty() ? "" : "💬";
							timeView.Text = image.queueStamp.ToLocalTime().ToString("G");
						}
					}
				});
			});
		}
	}

	public UserGalleryAdapter(Activity activity, string user)
	{
		_activity = activity;
		_db = ImageDatabaseAndroid.GetSingleton(activity);
		_images = _db.getImagesByUser(user);
		_viewHolders = new List<ViewHolder>();
	}

	public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
	{
		// Inflate XML layout to on which ViewHolder will be based.
		var view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.GalleryImageTile, parent, false);

		// Create a ViewHolder based on the inflated XML layout.
		// Maintain a list of all ViewHolders that are created by this adapter,
		// so that bitmaps allocated in the ViewHolder can be freed.
		var holder = new ViewHolder(_activity, (RelativeLayout)view, Click);
		_viewHolders.Add(holder);
		return holder;
	}

	public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
	{
		var viewHolder = (ViewHolder)holder;
		viewHolder.SetImage(_images[position]);
	}

	public override int ItemCount
	{
		get
		{
			return _images.Count;
		}
	}

	public void FreeAllBitmaps()
	{
		// Iterate through all ViewHolders created by this adapter, and free the
		// bitmap that was allocated by each.
		foreach (ViewHolder holder in _viewHolders)
		{
			holder.FreePreviousBitmap();
		}
	}

	public event EventHandler<int> Click;
}

[Activity (Label = "@string/user_gallery_activity_title",
	ParentActivity = typeof(GalleryActivity))]
public class UserGalleryActivity : AppCompatActivity
{
	UserGalleryAdapter _adapter;
	GridLayoutManager _layout;
	RecyclerView _recycler;

	protected override void OnCreate (Bundle savedInstanceState)
	{
		base.OnCreate (savedInstanceState);

		// Set content view from layout XML.
		SetContentView(Resource.Layout.Gallery);

		// Set support action bar based on toolbar from layout XML.
		var toolbar = FindViewById<Toolbar>(Resource.Id.galleryToolbar);
		SetSupportActionBar(toolbar);

		// Enable Up button on action bar.
		SupportActionBar.SetDisplayHomeAsUpEnabled(true);

		// Set title based on name of user whose gallery is being viewed.
		SupportActionBar.Title = Intent.GetStringExtra("user");

		// Get recycler view from layout.
		_recycler = FindViewById<RecyclerView>(Resource.Id.galleryRecyclerView);

		// Grid layout for user galleries.
		_layout = new GridLayoutManager(this, 2);
		_recycler.SetLayoutManager(_layout);

		// Attach recycler view to custom adapter.
		_adapter = new UserGalleryAdapter(this, Intent.GetStringExtra("user"));
		_recycler.SetAdapter(_adapter);

		// Respond to clicks on image tiles.
		_adapter.Click += delegate(Object sender, int imageId) {
			Toast.MakeText(this, "Image clicked: " + imageId, ToastLength.Short).Show();
		};
	}

	protected override void OnDestroy ()
	{
		base.OnDestroy();

		// When this activity ends, free memory used by all bitmaps that were
		// allocated for the RecyclerView adapter's view holders.
		_adapter.FreeAllBitmaps();
	}
}

}

