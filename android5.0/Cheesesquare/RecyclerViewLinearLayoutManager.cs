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

namespace Cheesesquare
{
    class RecyclerViewLinearLayoutManager : LinearLayoutManager
    {
        public RecyclerViewLinearLayoutManager(Context context) : base(context)
        {
        }

        private int[] mMeasuredDimension = new int[2];

        public override bool CanScrollVertically()
        {
            return false;
        }

        public override void OnMeasure(RecyclerView.Recycler recycler, RecyclerView.State state,
            int widthSpec, int heightSpec)
        {
            var widthMode = View.MeasureSpec.GetMode(widthSpec);
            var heightMode = View.MeasureSpec.GetMode(heightSpec);

            int widthSize = View.MeasureSpec.GetSize(widthSpec);
            int heightSize = View.MeasureSpec.GetSize(heightSpec);

            int width = 0;
            int height = 0;

            for (int i = 0; i < ItemCount; i++)
            {
                if (Orientation == 0) // Horizontal
                {
                    measureScrapChild(recycler, i,
                            View.MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified),
                            heightSpec,
                            mMeasuredDimension);

                    width = width + mMeasuredDimension[0];
                    if (i == 0)
                    {
                        height = mMeasuredDimension[1];
                    }
                }
                else {
                    measureScrapChild(recycler, i,
                            widthSpec,
                            View.MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified),
                            mMeasuredDimension);

                    height = height + mMeasuredDimension[1];
                    if (i == 0)
                    {
                        width = mMeasuredDimension[0];
                    }
                }
            }

            switch (widthMode)
            {
                case MeasureSpecMode.Exactly:
                    width = widthSize;
                    break;
                case MeasureSpecMode.AtMost:
                    break;
                case MeasureSpecMode.Unspecified:
                    break;
            }

            switch (heightMode)
            {
                case MeasureSpecMode.Exactly:
                    height = heightSize;
                    break;
                case MeasureSpecMode.AtMost:
                    break;
                case MeasureSpecMode.Unspecified:
                    break;
            }

            SetMeasuredDimension(width, height);
        }

        private void measureScrapChild(RecyclerView.Recycler recycler, int position, int widthSpec, int heightSpec, int[] measuredDimension)
        {

            View view = recycler.GetViewForPosition(position);
            if (view.Visibility == ViewStates.Gone)
            {
                measuredDimension[0] = 0;
                measuredDimension[1] = 0;
                return;
            }
            // For adding Item Decor Insets to view
            base.MeasureChildWithMargins(view, 0, 0);
            RecyclerView.LayoutParams p = (RecyclerView.LayoutParams)view.LayoutParameters;
            int childWidthSpec = ViewGroup.GetChildMeasureSpec(
                    widthSpec,
                    PaddingLeft + PaddingRight + GetDecoratedLeft(view) + GetDecoratedRight(view),
                    p.Width);
            int childHeightSpec = ViewGroup.GetChildMeasureSpec(
                    heightSpec,
                    PaddingTop + PaddingBottom + GetDecoratedTop(view) + GetDecoratedBottom(view),
                    p.Height);
            view.Measure(childWidthSpec, childHeightSpec);

            // Get decorated measurements
            measuredDimension[0] = GetDecoratedMeasuredWidth(view) + p.LeftMargin + p.RightMargin;
            measuredDimension[1] = GetDecoratedMeasuredHeight(view) + p.BottomMargin+ p.TopMargin;
            recycler.RecycleView(view);
        }

    }
}