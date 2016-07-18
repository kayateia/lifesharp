/*
	LifeStream - Instant Photo Sharing
	Copyright (C) 2014-2016 Deciare

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
using Uri = Android.Net.Uri;

namespace LifeSharp
{

public class UserGalleryAdapter : RecyclerView.Adapter
{
	Context _context;
	List<Image> _images;
	ImageDatabaseAndroid _db;

	public class ViewHolder : RecyclerView.ViewHolder
	{
		public Image image { get; private set; }
		public ImageView imageView { get; private set; }
		public TextView badgesView { get; private set; }
		public TextView timeView { get; private set; }

		public ViewHolder(Context context, RelativeLayout itemView, EventHandler<int> onClick) : base(itemView)
		{
			// Clicking on the item view triggers the adapter's Click event.
			itemView.Click += delegate {
				onClick(this, image.id);
			};

			// Attach member variables to child views of this item view.
			imageView = itemView.FindViewById<ImageView>(Resource.Id.galleryTileImage);
			badgesView = itemView.FindViewById<TextView>(Resource.Id.galleryTileBadges);
			timeView = itemView.FindViewById<TextView>(Resource.Id.galleryTileTime);
		}

		public void SetImage(Image image)
		{
			this.image = image;

			// Update views based on image
			imageView.SetImageURI(Uri.Parse(image.sourcePath));
			badgesView.Text = image.comment.IsNullOrEmpty() ? "" : "\u1f4ac";
			timeView.Text = image.queueStamp.ToLocalTime().ToString("G");
		}
	}

	public UserGalleryAdapter(Context context, string user)
	{
		_context = context;
		_db = ImageDatabaseAndroid.GetSingleton(context);
		_images = _db.getImagesByUser(user);
	}

	public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
	{
		var view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.GalleryImageTile, parent, false);
		return new ViewHolder(_context, (RelativeLayout)view, Click);
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
}

}

