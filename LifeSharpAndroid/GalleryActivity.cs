/*
	LifeStream - Instant Photo Sharing
	Copyright (C) 2014-2016 Kayateia

	This code is licensed under the GPL v3 or later.
	Please see the file LICENSE for more info.
 */

using System.Collections.Generic;

using Android.App;
using Android.Support.V7.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Support.V7.Widget;

using Uri = Android.Net.Uri;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace LifeSharp
{

public class GalleryAdapter : RecyclerView.Adapter
{
	Context _context;
	IImageDatabase _db;
	List<UserSummary> _users;

	public class ViewHolder : RecyclerView.ViewHolder
	{
		public CardView container { get; private set; }
		public Image image { get; private set; }
		public ImageView imageView { get; private set; }
		public TextView userLoginView { get; private set; }
		public TextView numImagesView { get; private set; }
		public TextView latestTimeView { get; private set; }

		public ViewHolder(CardView itemView) : base(itemView)
		{
			container = itemView;
			imageView = itemView.FindViewById<ImageView>(Resource.Id.galleryCardImageView);
			userLoginView = itemView.FindViewById<TextView>(Resource.Id.galleryCardUserLogin);
			numImagesView = itemView.FindViewById<TextView>(Resource.Id.galleryCardNumImages);
			latestTimeView = itemView.FindViewById<TextView>(Resource.Id.galleryCardLatest);
		}
	}

	public GalleryAdapter(Context context)
	{
		_context = context;
		_db = ImageDatabaseAndroid.GetSingleton(_context);
		_users = _db.getUserSummaries();
	}

	public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
	{
		var view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.GalleryCardView, parent, false);
		return new ViewHolder((CardView)view);
	}

	public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
	{
		var viewHolder = (ViewHolder)holder;
		viewHolder.imageView.SetImageURI(Uri.Parse(_users[position].lastImage.sourcePath));
		viewHolder.numImagesView.Text = _context.Resources.GetQuantityString(
			Resource.Plurals.gallery_card_numImages, _users[position].numImages,
			new Java.Lang.Object[]{ _users[position].numImages });
		viewHolder.userLoginView.Text = _users[position].userLogin;
		viewHolder.latestTimeView.Text = _context.GetString(Resource.String.gallery_card_latestImage)
			+ " " + _users[position].lastImage.queueStamp.ToLocalTime();
	}

	public override int ItemCount
	{
		get
		{
			return _users.Count;
		}
	}
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
	}
}

}
