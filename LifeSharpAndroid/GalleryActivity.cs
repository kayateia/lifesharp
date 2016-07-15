/*
	LifeStream - Instant Photo Sharing
	Copyright (C) 2014-2016 Kayateia

	This code is licensed under the GPL v3 or later.
	Please see the file LICENSE for more info.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Support.V7.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Support.V7.Widget;

using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace LifeSharp
{

[Activity(Label = "GalleryActivity",		// Action bar title text
	ParentActivity = typeof(MainActivity))]	// Up button target activity
public class GalleryActivity : AppCompatActivity
{
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
		Window.SetFlags(Android.Views.WindowManagerFlags.DrawsSystemBarBackgrounds, Android.Views.WindowManagerFlags.DrawsSystemBarBackgrounds);

		_recycler = FindViewById<RecyclerView>(Resource.Id.recyclerView);
		_layout = new GridLayoutManager(this, 4);
		_recycler.SetLayoutManager(_layout);
	}

	RecyclerView _recycler;
	GridLayoutManager _layout;
}

}
