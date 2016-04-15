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
using Android.Util;


namespace MindSet
{
    [Register("com.sample.mindset.FlowLayout")]
    public class FlowLayout : ViewGroup
    {

        private int line_height;

        public new class LayoutParams : ViewGroup.LayoutParams
        {

            public readonly int horizontal_spacing;
            public readonly int vertical_spacing;

            /**
             * @param horizontal_spacing Pixels between items, horizontally
             * @param vertical_spacing Pixels between items, vertically
             */
            public LayoutParams(int horizontal_spacing, int vertical_spacing) : base(0,0)
            {
                this.horizontal_spacing = horizontal_spacing;
                this.vertical_spacing = vertical_spacing;
            }
        }

    public FlowLayout(Context context) : base(context)
    {
    }

    public FlowLayout(Context context, IAttributeSet attrs) : base(context, attrs)
    {
    }

    protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
    {
        //assert(MeasureSpec.GetMode(widthMeasureSpec) != MeasureSpecMode.Unspecified);
        if (MeasureSpec.GetMode(widthMeasureSpec) != MeasureSpecMode.Unspecified)
        {
                int width = MeasureSpec.GetSize(widthMeasureSpec) - PaddingLeft - PaddingRight;
                int height = MeasureSpec.GetSize(heightMeasureSpec) - PaddingTop - PaddingBottom;
                int count = ChildCount;
                int line_height = 0;

                int xpos = PaddingLeft;
                int ypos = PaddingRight;

                int childHeightMeasureSpec;
                if (MeasureSpec.GetMode(heightMeasureSpec) == MeasureSpecMode.AtMost)
                {
                    childHeightMeasureSpec = MeasureSpec.MakeMeasureSpec(height, MeasureSpecMode.AtMost);
                }
                else
                {
                    childHeightMeasureSpec = MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified);
                }


                for (int i = 0; i < count; i++)
                {
                    View child = GetChildAt(i);
                    if (child.Visibility != ViewStates.Gone)
                    {
                        //LayoutParams lp = (LayoutParams)child.LayoutParameters;
                        LayoutParams lp = new LayoutParams(1, 1); 

                        child.Measure(MeasureSpec.MakeMeasureSpec(width, MeasureSpecMode.AtMost), childHeightMeasureSpec);
                        int childw = child.MeasuredWidth;
                        line_height = Math.Max(line_height, child.MeasuredHeight + lp.vertical_spacing);

                        if (xpos + childw > width)
                        {
                            xpos = PaddingLeft;
                            ypos += line_height;
                        }

                        xpos += childw + lp.horizontal_spacing;
                    }
                }
                this.line_height = line_height;

                if (MeasureSpec.GetMode(heightMeasureSpec) == MeasureSpecMode.Unspecified)
                {
                    height = ypos + line_height;

                }
                else if (MeasureSpec.GetMode(heightMeasureSpec) == MeasureSpecMode.AtMost)
                {
                    if (ypos + line_height < height)
                    {
                        height = ypos + line_height;
                    }
                }
                SetMeasuredDimension(width, height);
            }
    }

    protected override ViewGroup.LayoutParams GenerateDefaultLayoutParams()
    {
        return new LayoutParams(1, 1); // default of 1px spacing
    }

    protected override Boolean CheckLayoutParams(ViewGroup.LayoutParams p)
    {
        if (p is LayoutParams) {
            return true;
        }
        return false;
    }

    protected override void OnLayout(Boolean changed, int l, int t, int r, int b)
    {
        int count = ChildCount;
        int width = r - l;
        int xpos = PaddingLeft;
        int ypos = PaddingTop;

        for (int i = 0; i < count; i++)
        {
            View child = GetChildAt(i);
            if (child.Visibility != ViewStates.Gone)
            {
                int childw = child.MeasuredWidth;
                int childh = child.MeasuredHeight;
                LayoutParams lp = new LayoutParams(1, 1);
                //LayoutParams lp = (LayoutParams)child.LayoutParameters;
                if (xpos + childw > width)
                {
                    xpos = PaddingLeft;
                    ypos += line_height;
                }
                child.Layout(xpos, ypos, xpos + childw, ypos + childh);
                xpos += childw + lp.horizontal_spacing;
            }
        }
    }
}
}