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
using Android.Graphics.Drawables;
using Android.Content.Res;
using Android.Support.V4.Content;
using Android.Graphics;

namespace MindSet
{
    public class DividerItemDecoration : RecyclerView.ItemDecoration
    {

        private Drawable mDivider;

        /**
         * Default divider will be used
         */
        public DividerItemDecoration(Context context) {
            var styledAttributes = context.ObtainStyledAttributes(new int[] { Android.Resource.Attribute.ListDivider });
            mDivider = styledAttributes.GetDrawable(0);
            styledAttributes.Recycle();

            //mDivider = ContextCompat.GetDrawable(context,Resource.Drawable.line_divider);
        }

        /**
         * Custom divider will be used
         */
        public DividerItemDecoration(Context context, int resId) {
            mDivider = ContextCompat.GetDrawable(context, resId);
        }

        public override void OnDraw(Canvas c, RecyclerView parent, RecyclerView.State state) {
            int left = parent.PaddingLeft;
            int right = parent.Width - parent.PaddingRight;

            // adapt for our own contact_list_item
            left += 65; // imageview + margins is 65dp
            right -= 16; // bit smaller on the right

            int childCount = parent.ChildCount;
            for (int i = 0; i < childCount; i++)
            {
                View child = parent.GetChildAt(i);
                ViewHolder vh = parent.GetChildViewHolder(child) as ViewHolder;

                left = (int) vh.TextView.GetX();

                right = left + vh.TextView.Width;

                ViewGroup.MarginLayoutParams paramss = child.LayoutParameters as ViewGroup.MarginLayoutParams;

                var parameters = child.LayoutParameters.JavaCast<RecyclerView.LayoutParams>();

                int top = child.Bottom + parameters.BottomMargin;
                int bottom = top + mDivider.IntrinsicHeight;

                mDivider.SetBounds(left, top, right, bottom);
                mDivider.Draw(c);
            }
        }
    }
}