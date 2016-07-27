/*
	LifeStream - Instant Photo Sharing
	Copyright (C) 2014-2016 Deciare

	This code is licensed under the GPL v3 or later.
	Please see the file LICENSE for more info.
 */

using System;

using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Widget;

namespace LifeSharp
{

// Adapted from: https://stackoverflow.com/a/17424313
public class DynamicImageView : ImageView
{
	bool _squareImage; // mutually exclusive with maxHeight; takes precedence
	int _maxHeight; // mutually exclusive with squareImage

	public DynamicImageView(Context context, IAttributeSet attrs) : base(context, attrs)
	{
		var attrsResourceIds = new int[]{
			Android.Resource.Attribute.MaxHeight,
			Resource.Attribute.squareImage
		};
		var attrsArr = context.ObtainStyledAttributes(attrs, attrsResourceIds);
		_maxHeight = (int)attrsArr.GetDimension(0, 0);
		_squareImage = attrsArr.GetBoolean(1, false);
		attrsArr.Recycle();
	}

	protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec) {
		if (Drawable != null)
		{
			// ceil not round - avoid thin vertical gaps along the left/right edges
			int width = MeasureSpec.GetSize(widthMeasureSpec);
			int height = (int)Math.Ceiling(width * (float)Drawable.IntrinsicHeight / Drawable.IntrinsicWidth);
			if (_maxHeight != 0 && height > _maxHeight)
				height = _maxHeight;
			else if (_squareImage)
				height = width;
			SetMeasuredDimension(width, height);
		}
		else
		{
			base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
		}
	}
}

}

