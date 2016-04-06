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
using Java.Util;
using Java.IO;
using Android.Content.PM;
using Android.Provider;

namespace Cheesesquare
{
    [Activity(Label = "Select Image")]
    public class SelectImageActivity : AppCompatActivity
    {
        private GridView gridview;
        private string ItemID;
        public const int PICKIMAGE = 105;
        private const int GALLERYORCAMERA = 106;

        private Android.Net.Uri outputFileUri;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.activity_select_image);
            ItemID = Intent.GetStringExtra(DetailActivity.ITEM_ID); 

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
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    return true;
                case Resource.Id.camera:
                    // Determine Uri of camera image to save.
                    File root = new File(Android.OS.Environment.ExternalStorageDirectory + File.Separator + "MindSet" + File.Separator);
                    root.Mkdirs();
                    string fname = GetUniqueImageFilename();
                    File sdImageMainDirectory = new File(root, fname);
                    outputFileUri = Android.Net.Uri.FromFile(sdImageMainDirectory);

                    // Camera
                    List<Intent> cameraIntents = new List<Intent>();
                    Intent captureIntent = new Intent(Android.Provider.MediaStore.ActionImageCapture);
                    captureIntent.PutExtra(MediaStore.ExtraOutput, outputFileUri);

                    if (captureIntent.ResolveActivity(PackageManager) != null)
                    {
                        StartActivityForResult(captureIntent, GALLERYORCAMERA);
                    }
                    break;
                case Resource.Id.gallery:
                    Intent galleryIntent = new Intent();
                    galleryIntent.SetType("image/*");
                    galleryIntent.SetAction(Intent.ActionGetContent);

                    StartActivityForResult(galleryIntent, GALLERYORCAMERA);
                    break;
            }
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
            myIntent.PutExtra(DetailActivity.ITEM_ID, ItemID);

            SetResult(Result.Ok, myIntent);
            Finish();
        }

        private string GetUniqueImageFilename()
        {
            UUID uuid = UUID.RandomUUID();
            string randomUUIDString = uuid.ToString() + ".jpg";
            return randomUUIDString;
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (resultCode == Result.Canceled)
            {
                SetResult(Result.Canceled, new Intent());
                Finish();
            }

            if (resultCode == Result.Ok)
            {
                switch (requestCode)
                {
                    case GALLERYORCAMERA:
                        bool isCamera;
                        if (data == null || (data.Data == null && data.ClipData == null))
                        {
                            isCamera = true;
                        }
                        else
                        {
                            string action = data.Action;
                            if (action == null)
                            {
                                isCamera = false;
                            }
                            else
                            {
                                isCamera = action.Equals(Android.Provider.MediaStore.ActionImageCapture);
                            }
                        }

                        Android.Net.Uri selectedImageUri;
                        if (isCamera)
                        {
                            selectedImageUri = outputFileUri;
                        }
                        else
                        {
                            selectedImageUri = data == null ? null : data.Data;
                        }

                        string path = MainActivity.GetPath(this, selectedImageUri);
                        Toast.MakeText(this, path, ToastLength.Long).Show();

                        var item = PublicFields.ItemTree.Descendants().FirstOrDefault(node => node.Value.id == ItemID);
                        item.Value.ImagePath = path;

                        Intent myIntent = new Intent();


                        myIntent.SetType("image/*");
                        myIntent.SetData(selectedImageUri);

                        myIntent.PutExtra("path", path);
                        myIntent.PutExtra(DetailActivity.ITEM_ID, ItemID);

                        SetResult(Result.Ok, myIntent);
                        Finish();

                        break;

                    default:
                        break;
                }
            }

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