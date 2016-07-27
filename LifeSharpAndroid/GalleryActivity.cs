/*
	LifeStream - Instant Photo Sharing
	Copyright (C) 2014-2016 Kayateia and Deciare

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

public class GalleryAdapter : RecyclerGalleryView.Adapter
{
	IImageDatabase _db;
	List<UserSummary> _users;

	public class ViewHolder : RecyclerGalleryView.ViewHolder
	{
		public UserSummary userSummary { get; private set; }
		public TextView userLoginView { get; private set; }
		public TextView numImagesView { get; private set; }
		public TextView latestTimeView { get; private set; }

		public ViewHolder(Activity activity, CardView itemView, int targetWidth, EventHandler<UserSummary> onClick) : base(activity, itemView, targetWidth)
		{
			// Clicking on the item view triggers the adapter's Click event.
			itemView.Click += delegate {
				onClick(this, userSummary);
			};

			// Attach member variables to child views of this item view.
			this.imageView = itemView.FindViewById<ImageView>(Resource.Id.galleryCardImageView);
			this.userLoginView = itemView.FindViewById<TextView>(Resource.Id.galleryCardUserLogin);
			this.numImagesView = itemView.FindViewById<TextView>(Resource.Id.galleryCardNumImages);
			this.latestTimeView = itemView.FindViewById<TextView>(Resource.Id.galleryCardLatest);
		}

		public void SetUser(UserSummary userSummary)
		{
			// Base class handles decoding and assigning image to view.
			SetImageViewTarget(userSummary.lastImage);

			// Additional things to be done for this class.
			this.userSummary = userSummary;
			this.numImagesView.Text = this.activity.Resources.GetQuantityString(
				Resource.Plurals.gallery_card_numImages, userSummary.numImages,
				new Java.Lang.Object[]{ userSummary.numImages });
			this.userLoginView.Text = userSummary.userLogin;
			this.latestTimeView.Text = this.activity.GetString(Resource.String.gallery_card_latestImage)
				+ " " + userSummary.lastImage.queueStamp.ToLocalTime().ToString("G");
		}
	}

	public GalleryAdapter(Activity activity) : base(activity)
	{
		_db = ImageDatabaseAndroid.GetSingleton(activity);
		_users = _db.getUserSummaries();
	}

	public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
	{
		var view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.GalleryCardView, parent, false);
		var viewHolder = new ViewHolder(this.activity, (CardView)view, this.screenWidth, Click);
		this.viewHolders.Add(viewHolder);
		return viewHolder;
	}

	public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
	{
		var viewHolder = (ViewHolder)holder;
		viewHolder.SetUser(_users[position]);
	}

	public override int ItemCount
	{
		get
		{
			return _users.Count;
		}
	}

	public event EventHandler<UserSummary> Click;
}

[Activity(Label = "@string/gallery_activity_title",	// Action bar title text
	ParentActivity = typeof(MainActivity))]			// Up button target activity
public class GalleryActivity : AppCompatActivity
{
	GalleryAdapter _adapter;
	LinearLayoutManager _layout;
	RecyclerView _recycler;

	protected override void OnCreate(Bundle savedInstanceState)
	{
		base.OnCreate(savedInstanceState);

		// Set content view from layout XML
		SetContentView(Resource.Layout.Gallery);

		// Set support action bar based on Toolbar from layout XML
		var galleryToolbar = FindViewById<Toolbar>(Resource.Id.galleryToolbar);
		SetSupportActionBar(galleryToolbar);

		// Enable Up button in action bar
		SupportActionBar.SetDisplayHomeAsUpEnabled(true);

		// Set flag to allow status bar colour to be managed by this activity.
		Window.SetFlags(WindowManagerFlags.DrawsSystemBarBackgrounds, WindowManagerFlags.DrawsSystemBarBackgrounds);

		// Get recycler view from layout.
		_recycler = FindViewById<RecyclerView>(Resource.Id.galleryRecyclerView);

		// Use a vertical linear layout for the recycler view.
		_layout = new LinearLayoutManager(this);
		_recycler.SetLayoutManager(_layout);

		// Attach our custom adapter to the recycler view.
		_adapter = new GalleryAdapter(this);
		_recycler.SetAdapter(_adapter);

		// Start user gallery activity
		_adapter.Click += delegate(Object sender, UserSummary user) {
			var userGalleryActivity = new Intent(this, typeof(UserGalleryActivity));
			userGalleryActivity.PutExtra("user", user.userLogin);
			StartActivity(userGalleryActivity);
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
