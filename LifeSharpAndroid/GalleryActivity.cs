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
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Support.V7.Widget;

namespace LifeSharp
{

[Activity(Label = "GalleryActivity")]
public class GalleryActivity : Activity
{
	protected override void OnCreate(Bundle savedInstanceState)
	{
		base.OnCreate(savedInstanceState);

		SetContentView(Resource.Layout.Gallery);

		_recycler = FindViewById<RecyclerView>(Resource.Id.recyclerView);
		_layout = new GridLayoutManager(this, 4);
		_recycler.SetLayoutManager(_layout);
	}

	RecyclerView _recycler;
	GridLayoutManager _layout;
}

}
