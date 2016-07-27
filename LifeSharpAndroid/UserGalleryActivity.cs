/*
	LifeStream - Instant Photo Sharing
	Copyright (C) 2016 Deciare

	This code is licensed under the GPL v3 or later.
	Please see the file LICENSE for more info.
 */

using System;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;

using Android.Support.V7.App;
using Android.Support.V7.Widget;

using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace LifeSharp
{

public class UserGalleryAdapter : RecyclerGalleryView.Adapter
{
	List<Image> _images;
	ImageDatabaseAndroid _db;

	public class ViewHolder : RecyclerGalleryView.ViewHolder
	{
		public Image image { get; private set; }
		public TextView badgesView { get; private set; }
		public TextView timeView { get; private set; }

		public ViewHolder(Activity activity, RelativeLayout itemView, int targetWidth, EventHandler<int> onClick) : base(activity, itemView, targetWidth)
		{
			// Clicking on the item view triggers the adapter's Click event.
			itemView.Click += delegate {
				onClick(this, image.id);
			};

			// Attach member variables to child views of this item view.
			this.imageView = itemView.FindViewById<ImageView>(Resource.Id.galleryTileImage);
			this.badgesView = itemView.FindViewById<TextView>(Resource.Id.galleryTileBadges);
			this.timeView = itemView.FindViewById<TextView>(Resource.Id.galleryTileTime);
		}

		public void SetImage(Image image)
		{
			// Base class handles decoding and assigning image to view.
			SetImageViewTarget(image);

			// Additional things to be done for this class.
			this.image = image;
			this.badgesView.Text = image.comment.IsNullOrEmpty() ? "" : "💬";
			this.timeView.Text = image.queueStamp.ToLocalTime().ToString("G");
		}
	}

	public UserGalleryAdapter(Activity activity, string user) : base(activity)
	{
		_db = ImageDatabaseAndroid.GetSingleton(activity);
		_images = _db.getImagesByUser(user);
	}

	public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
	{
		// Inflate XML layout to on which ViewHolder will be based.
		var view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.GalleryImageTile, parent, false);

		// Create a ViewHolder based on the inflated XML layout.
		// Maintain a list of all ViewHolders that are created by this adapter,
		// so that bitmaps allocated in the ViewHolder can be freed.
		var holder = new ViewHolder(this.activity, (RelativeLayout)view, (int)(this.screenWidth * 0.5), Click);
		this.viewHolders.Add(holder);
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

