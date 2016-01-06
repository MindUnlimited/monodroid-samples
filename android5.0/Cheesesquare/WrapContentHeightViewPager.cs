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
using Android.Support.V4.View;
using Android.Util;

namespace Cheesesquare
{
    [Register("com.sample.cheesesquare.WrapContentHeightViewPager")]
    public class WrapContentHeightViewPager : ViewPager
    {
        public WrapContentHeightViewPager(Context context) : base(context)
        {
        }
        public WrapContentHeightViewPager(Context context, IAttributeSet attrs) : base(context, attrs)
        {
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {

            int height = 0;
            for (int i = 0; i < ChildCount; i++)
            {
                View child = GetChildAt(i);
                child.Measure(widthMeasureSpec, MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified));
                int h = child.MeasuredHeight;

                if (h > height) height = h;
            }

            heightMeasureSpec = MeasureSpec.MakeMeasureSpec(height, MeasureSpecMode.Exactly);

            base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
        }
    }
}