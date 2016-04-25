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
using Newtonsoft.Json;
using Java.IO;
using Android.Content.PM;
using Android.Provider;
using Java.Util;
using Android.Support.V7.App;

namespace MindSet
{
    [Activity(Label = "PickImageActivity")]
    public class PickImageActivity : AppCompatActivity
    {
        private Android.Net.Uri outputFileUri;

        private void openImageIntent()
        {

            // Determine Uri of camera image to save.
            File root = new File(Android.OS.Environment.ExternalStorageDirectory + File.Separator + "MindSet" + File.Separator);
            root.Mkdirs();
            String fname = GetUniqueImageFilename();
            File sdImageMainDirectory = new File(root, fname);
            outputFileUri = Android.Net.Uri.FromFile(sdImageMainDirectory);

            // Camera.
            List< Intent > cameraIntents = new List<Intent>();
            Intent captureIntent = new Intent(Android.Provider.MediaStore.ActionImageCapture);
            PackageManager packageManager = PackageManager;
            IList<ResolveInfo> listCam = packageManager.QueryIntentActivities(captureIntent, 0);
            foreach (ResolveInfo res in listCam)
            {
                String packageName = res.ActivityInfo.PackageName;
                Intent intent = new Intent(captureIntent);
                intent.SetComponent(new ComponentName(res.ActivityInfo.PackageName, res.ActivityInfo.Name));
                intent.SetPackage(packageName);
                intent.PutExtra(MediaStore.ExtraOutput, outputFileUri);
                cameraIntents.Add(intent);
            }

            // Filesystem.
             Intent galleryIntent = new Intent();
            galleryIntent.SetType("image/*");
            galleryIntent.SetAction(Intent.ActionGetContent);

            var galleryChooser = Intent.CreateChooser(galleryIntent, "Select Picture");

            // Chooser of filesystem options.
            Intent chooserIntent = Intent.CreateChooser(galleryIntent, "Select Source");

            //foreach(var cameraInt in cameraIntents)
            //{
            //    cameraInt.par
            //}

            // Add the camera options.
            chooserIntent.PutExtra(Intent.ExtraInitialIntents, cameraIntents.ToArray());//cameraIntents.ToArray(new Parcelable[cameraIntents.size()]));

            StartActivityForResult(chooserIntent, GALLERYORCAMERA);
        }

        private string GetUniqueImageFilename()
        {
            UUID uuid = UUID.RandomUUID();
            String randomUUIDString = uuid.ToString() + ".jpg";
            return randomUUIDString;
        }

        //        @Override
        //protected void onActivityResult(int requestCode, int resultCode, Intent data)
        //        {
        //            if (resultCode == RESULT_OK)
        //            {
        //                if (requestCode == YOUR_SELECT_PICTURE_REQUEST_CODE)
        //                {
        //                     boolean isCamera;
        //                    if (data == null)
        //                    {
        //                        isCamera = true;
        //                    }
        //                    else
        //                    {
        //                         String action = data.getAction();
        //                        if (action == null)
        //                        {
        //                            isCamera = false;
        //                        }
        //                        else
        //                        {
        //                            isCamera = action.equals(android.provider.MediaStore.ACTION_IMAGE_CAPTURE);
        //                        }
        //                    }

        //                    Uri selectedImageUri;
        //                    if (isCamera)
        //                    {
        //                        selectedImageUri = outputFileUri;
        //                    }
        //                    else
        //                    {
        //                        selectedImageUri = data == null ? null : data.getData();
        //                    }
        //                }
        //            }
        //        }




        private string ItemID;
        public const int PICKIMAGE = 105;
        private const int GALLERYORCAMERA = 106;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            ItemID = Intent.GetStringExtra(DetailActivity.ITEM_ID);

            openImageIntent();

            //var intent = new Intent();
            //intent.SetType("image/*");
            //intent.SetAction(Intent.ActionGetContent);

            //StartActivityForResult(Intent.CreateChooser(intent, "Select Picture"), PICKIMAGE);

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
                    //case PICKIMAGE:
                    //    Android.Net.Uri uri = data.Data;

                    //    string path = MainActivity.GetPath(this, uri);
                    //    //Log.Debug("Main", path);
                    //    Toast.MakeText(this, path, ToastLength.Long).Show();

                    //    var item = PublicFields.ItemTree.Descendants().FirstOrDefault(node => node.Value.id == ItemID);
                    //    item.Value.ImagePath = path;

                    //    Intent myIntent = new Intent();


                    //    myIntent.SetType("image/*");
                    //    myIntent.SetData(uri);

                    //    myIntent.PutExtra("path", path);
                    //    myIntent.PutExtra(CheeseDetailActivity.ITEM_ID, ItemID);

                    //    SetResult(Result.Ok, myIntent);
                    //    Finish();
                    //    break;
                    case GALLERYORCAMERA:
                        bool isCamera;
                        if (data == null || (data.Data == null && data.ClipData == null))
                        {
                            isCamera = true;
                        }
                        else
                        {
                            String action = data.Action;
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


                        //Android.Net.Uri uri = data.Data;

                        string path = MainActivity.GetPath(this, selectedImageUri);
                        //Log.Debug("Main", path);
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
}