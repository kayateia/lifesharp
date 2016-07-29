/*
	LifeStream - Instant Photo Sharing
	Copyright (C) 2016 Deciare

	This code is licensed under the GPL v3 or later.
	Please see the file LICENSE for more info.
 */

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;

using Android.Support.V7.App;

using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace LifeSharp
{

[Activity (Label = "@string/image_activity_title")]
public class ImagePagerActivity : AppCompatActivity
{
	ImagePagerAdapter _adapter;
	ImagePager _pager;

	protected override void OnCreate (Bundle savedInstanceState)
	{
		base.OnCreate (savedInstanceState);
		
		// Set content view from layout XML.
		SetContentView(Resource.Layout.ImageViewPager);

		// Set support action bar based on toolbar from layout XML.
		var toolbar = FindViewById<Toolbar>(Resource.Id.galleryToolbar);
		SetSupportActionBar(toolbar);

		// Enable Up button on action bar.
		SupportActionBar.SetDisplayHomeAsUpEnabled(true);

		// Set title based on name of user whose gallery is being viewed.
		SupportActionBar.Title = Intent.GetStringExtra("user");

		// Set flag to allow status bar colour to be managed by this activity.
		Window.SetFlags(WindowManagerFlags.DrawsSystemBarBackgrounds, WindowManagerFlags.DrawsSystemBarBackgrounds);

		// Get view pager from layout.
		_pager = FindViewById<ImagePager>(Resource.Id.imageViewPager);

		// Attach view pager to custom adapter.
		_adapter = new ImagePagerAdapter(SupportFragmentManager, this, Intent.GetStringExtra("user"));
		_pager.Adapter = _adapter;

		// Set initial image to display.
		_pager.SetCurrentImage(Intent.GetIntExtra("imageId", 0));
	}

	public override Intent SupportParentActivityIntent {
		get
		{
			return GetParentActivityIntentImpl();
		}
	}

	public override Intent ParentActivityIntent {
		get
		{
			return GetParentActivityIntentImpl();
		}
	}

	/// <summary>
	/// Programmatically set this activity's parent activity. We can't simply
	/// specify the parent activity as an attribute on this class because
	/// UserGalleryActivity requires the userLogin to be passed as string extra
	/// to the intent, and we can't pass extras from an attribute.
	/// </summary>
	Intent GetParentActivityIntentImpl() {
	    Intent intent;

		// The parent activityis a UserGalleryActivity.
		intent = new Intent(this, typeof(UserGalleryActivity));
		// Pass the userLogin that was passed into this activity, back to UserGalleryActivity.
		intent.PutExtra("user", Intent.GetStringExtra("user"));
		// Reuse an existing instance of UserGalleryActivity if present.
		intent.SetFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop);
	
	    return intent;
	}
}

}

