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
using Java.Lang;
using V7Toolbar = Android.Support.V7.Widget.Toolbar;
using Android.Support.V7.App;

namespace Cheesesquare
{
    [Activity(Label = "SelectImageActivity")]
    public class SelectImageActivity : AppCompatActivity
    {
        private GridView gridview;
        private string ItemID;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.activity_select_image);
            ItemID = Intent.GetStringExtra(CheeseDetailActivity.ITEM_ID); 

            var toolbar = FindViewById<V7Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            //SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.ic_menu);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);

            gridview = (GridView)FindViewById(Resource.Id.gridview);
            gridview.Adapter = new ImageAdapter(this);

            gridview.ItemClick += Gridview_ItemClick;
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {

            MenuInflater.Inflate(Resource.Menu.select_image_actions, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            return base.OnOptionsItemSelected(item);
        }

        private void Gridview_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            var resourceID = (int) gridview.Adapter.GetItem(e.Position);

            var adapter = (ImageAdapter)gridview.Adapter;


            Intent myIntent = new Intent();
            myIntent.PutExtra("resourceID", resourceID);

            int resourceBackdropID;
            switch (resourceID)
            {
                case Resource.Drawable.Goal256:
                    resourceBackdropID = Resource.Drawable.Goal1024;
                    myIntent.PutExtra("resourceBackdropID", resourceBackdropID);
                    break;
                case Resource.Drawable.Project256:
                    resourceBackdropID = Resource.Drawable.Project1024;
                    myIntent.PutExtra("resourceBackdropID", resourceBackdropID);
                    break;
                case Resource.Drawable.Task256:
                    resourceBackdropID = Resource.Drawable.Task1024;
                    myIntent.PutExtra("resourceBackdropID", resourceBackdropID);
                    break;
                default:
                    break;
            }
            myIntent.PutExtra(CheeseDetailActivity.ITEM_ID, ItemID);

            SetResult(Result.Ok, myIntent);
            Finish();
        }
    }

    public class ImageAdapter  : BaseAdapter
    {
        private Context mContext;

        public ImageAdapter(Context c) {
            mContext = c;
        }       

        public override Java.Lang.Object GetItem(int position)
        {
            return mThumbIds[position];
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        // create a new ImageView for each item referenced by the Adapter
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            ImageView imageView;
            if (convertView == null)
            {
                // if it's not recycled, initialize some attributes
                imageView = new ImageView(mContext);
                imageView.LayoutParameters = new GridView.LayoutParams(250, 250);
                imageView.SetScaleType(ImageView.ScaleType.CenterCrop);
                imageView.SetPadding(8, 8, 8, 8);
            }
            else
            {
                imageView = (ImageView)convertView;
            }

            imageView.SetImageResource(mThumbIds[position]);
            return imageView;
        }

        // references to our images
        private int[] mThumbIds = {
            Resource.Drawable.Goal256, 
            Resource.Drawable.Project256,
            Resource.Drawable.Task256
        };

        public override int Count
        {
            get
            {
                return mThumbIds.Length;
            }
        }
    }



}