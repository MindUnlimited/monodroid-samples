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
using Android.Support.V7.App;
using Android.Provider;
using Android.Util;
using Android.Graphics;
using Android.Support.Design.Widget;
using Android.Support.V4.Graphics.Drawable;

namespace Cheesesquare
{
    [Activity(Label = "EditItemActivity")]
    public class EditItemActivity : AppCompatActivity
    {
        private Android.Support.V7.Widget.Toolbar toolbar;



        public const string EXTRA_NAME = "cheese_name";
        private const int ASSIGN_CONTACT = 100;
        private const int SHARE_CONTACT = 101;

        private EditText subtaskText;
        private EditText assignEditText;
        private EditText shareEditText;
        private EditText txtDate;
        private ListView subTaskListView;
        private ListView shareListView;
        private ArrayAdapter<String> subTaskArrayAdapter;
        private ArrayAdapter<String> shareArrayAdapter;
        private List<String> subTaskList;
        private List<String> shareList;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            var cheeseName = Intent.GetStringExtra(EXTRA_NAME);

            // Create your application here
            SetContentView(Resource.Layout.activity_edit_item);
            Window.SetSoftInputMode(SoftInput.AdjustResize);
            Window.SetSoftInputMode(SoftInput.StateHidden);

            subTaskList = new List<string>();
            shareList = new List<string>();

            toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar_edit);
            toolbar.Title = cheeseName;
            toolbar.SetNavigationIcon(Resource.Drawable.ic_clear_white_24dp);

            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);

            subTaskListView = FindViewById<ListView>(Resource.Id.subtask_lists);
            subTaskArrayAdapter = new ArrayAdapter<String>(this, Android.Resource.Layout.SimpleListItem1, Android.Resource.Id.Text1, subTaskList);
            subTaskArrayAdapter.SetNotifyOnChange(true);
            subTaskListView.Adapter = subTaskArrayAdapter;

            txtDate = (EditText)FindViewById(Resource.Id.item_date);
            txtDate.FocusChange += TxtDate_FocusChange;

            shareEditText = FindViewById<EditText>(Resource.Id.user_to_share_name);
            shareEditText.FocusChange += shareAssign_FocusChange;

            shareListView = FindViewById<ListView>(Resource.Id.user_to_share_listview);
            shareArrayAdapter = new ArrayAdapter<String>(this, Android.Resource.Layout.SimpleListItem1, Android.Resource.Id.Text1, shareList);
            shareArrayAdapter.SetNotifyOnChange(true);
            shareListView.Adapter = shareArrayAdapter;

            assignEditText = FindViewById<EditText>(Resource.Id.assigned_to_name);
            assignEditText.Click += ThumbAndName_Click;

            LinearLayout thumbAndName = FindViewById<LinearLayout>(Resource.Id.assigned_to_thumb_name);
            thumbAndName.Click += ThumbAndName_Click;

            subtaskText = FindViewById<EditText>(Resource.Id.add_subtask_text);
            //subtaskText.SetOnKeyListener(new subTaskKeyListener(subTaskArrayAdapter, subtaskText));
            //subtaskText.KeyPress += SubtaskText_KeyPress;

            subtaskText.KeyPress += (object sender, View.KeyEventArgs e) =>
            {
                e.Handled = false;
                if (e.Event.Action == KeyEventActions.Down && e.KeyCode == Keycode.Enter)
                {
                    if (subtaskText.Text != "")
                    {
                        subTaskArrayAdapter.Add(subtaskText.Text);
                        subTaskArrayAdapter.NotifyDataSetChanged();
                        Log.Debug("EditItemActivity", subTaskArrayAdapter.Count.ToString());
                        subtaskText.Text = "";
                        e.Handled = true;
                    }
                    else // an immediate enter does nothing
                    {
                        e.Handled = true;
                    }
                }
            };
        }

        private void ThumbAndName_Click(object sender, EventArgs e)
        {
            Intent intent = new Intent(Intent.ActionPick, ContactsContract.Contacts.ContentUri);
            StartActivityForResult(intent, ASSIGN_CONTACT);
        }

        protected override void OnStart()
        {
            base.OnStart();


        }

        private void SubtaskText_KeyPress(object sender, View.KeyEventArgs e)
        {
            // If the event is a key-down event on the "enter" button
            if ((e.Event.Action == KeyEventActions.Down) && (e.KeyCode == Keycode.Enter))
            {
                subTaskArrayAdapter.Add(subtaskText.Text);
            }
        }

        private void shareAssign_FocusChange(object sender, View.FocusChangeEventArgs e)
        {
            var editText = (EditText)sender;
            Intent intent = new Intent(Intent.ActionPick, ContactsContract.Contacts.ContentUri);
            if (e.HasFocus)
            {
                
                StartActivityForResult(intent, SHARE_CONTACT);//PICK_CONTACT is private static final int, so declare in activity class
                editText.ClearFocus();
            }
        }

        protected override void OnActivityResult(int requestCode, Result resultCode,
        Intent intent)
        {
            base.OnActivityResult(requestCode, resultCode, intent);

            if (resultCode == Result.Ok)
            {
                switch (requestCode)
                {
                    case ASSIGN_CONTACT:
                        Android.Net.Uri uri = intent.Data;
                        String id = intent.Data.LastPathSegment;

                        Android.Database.ICursor cursor = null;
                        String photoUri = "", photoThumbnailUri = "", name = "";
                        try
                        {
                            String[] projection = { ContactsContract.ContactNameColumns.DisplayNamePrimary, ContactsContract.ContactsColumns.PhotoUri, ContactsContract.ContactsColumns.PhotoThumbnailUri }; // PROBLEM OVER HERE?
                            cursor = ContentResolver.Query(uri, projection,
                                null, null, null);
                            cursor.MoveToFirst();

                            if (cursor.MoveToFirst())
                            {
                                int nameColumnIndex = cursor.GetColumnIndex(ContactsContract.ContactNameColumns.DisplayNamePrimary);
                                name = cursor.GetString(nameColumnIndex);

                                int photoUriColumnIndex = cursor.GetColumnIndex(ContactsContract.ContactsColumns.PhotoUri);
                                photoUri = cursor.GetString(photoUriColumnIndex);

                                int photoThumbnailUriColumnIndex = cursor.GetColumnIndex(ContactsContract.ContactsColumns.PhotoThumbnailUri);
                                photoThumbnailUri = cursor.GetString(photoThumbnailUriColumnIndex);
                            }
                            else
                            {
                                Log.Warn("EditItemActivity", "No results");
                            }
                        }
                        catch (Exception e)
                        {
                            Log.Error("EditItemActivity", "Failed to personal data", e);
                        }
                        finally
                        {
                            if (cursor != null)
                            {
                                cursor.Close();
                            }

                            if (photoThumbnailUri != null)
                            {
                                var photo = Android.Net.Uri.Parse(photoThumbnailUri);
                                ImageView assignedThumb = this.FindViewById<ImageView>(Resource.Id.assigned_to_thumb);
                                Bitmap thumbBitmap = MediaStore.Images.Media.GetBitmap(ContentResolver, photo); //.getBitmap(this.getContentResolver(), imageUri);
                                var roundedThumbBitmap = RoundedBitmapDrawableFactory.Create(Resources, thumbBitmap);
                                roundedThumbBitmap.Circular = true;

                                assignedThumb.SetImageDrawable(roundedThumbBitmap);
                                assignedThumb.SetColorFilter(null);
                                assignedThumb.Visibility = ViewStates.Visible;
                            }
                            else
                            {
                                ImageView assignedThumb = this.FindViewById<ImageView>(Resource.Id.assigned_to_thumb);
                                assignedThumb.SetImageResource(Resource.Drawable.ic_account_circle_black_24dp);
                                assignedThumb.SetColorFilter(Color.ParseColor("#A9A9A9"));//. ImageTintMode = PorterDuff.Mode.ValueOf();
                                assignedThumb.Visibility = ViewStates.Visible;
                            }
                            if (name == null)
                            {
                                //Snackbar.Make(FindViewById(Resource.Id.edit_item_coordinator), "No name for Selected Contact", Snackbar.LengthLong).Show();
                                Toast.MakeText(this, "No name for Selected Contact", ToastLength.Long).Show();
                                var assignedTo = FindViewById<EditText>(Resource.Id.assigned_to_name);
                                assignedTo.Text = "(No name)";
                            }
                            else
                            {
                                var assignedTo = FindViewById<EditText>(Resource.Id.assigned_to_name);
                                assignedTo.Text = name;
                            }
                        }

                        cursor = null;
                        String email = "";
                        try
                        {
                            cursor = ContentResolver.Query(ContactsContract.CommonDataKinds.Email.ContentUri, null, ContactsContract.CommonDataKinds.Email.InterfaceConsts.ContactId + "=?", new String[] { id }, null);
                            int emailColumnIndex = cursor.GetColumnIndex(ContactsContract.CommonDataKinds.Email.InterfaceConsts.Data);

                            cursor.MoveToFirst();

                            if (cursor.MoveToFirst())
                            {
                                email = cursor.GetString(emailColumnIndex);
                                Log.Verbose("EditItemActivity", "Got email: " + email);
                            }
                            else
                            {
                                Log.Warn("EditItemActivity", "No results");
                            }
                        }
                        catch (Exception e)
                        {
                            Log.Error("EditItemActivity", "Failed to get email data", e);
                        }
                        finally
                        {
                            if (cursor != null)
                            {
                                cursor.Close();
                            }
                            if (email.Length == 0)
                            {
                                //Snackbar.Make(FindViewById(Resource.Id.edit_item_coordinator), "No email for Selected Contact", Snackbar.LengthLong).Show();
                                Toast.MakeText(this, "No email for Selected Contact", ToastLength.Long).Show();
                            }
                        }

                        Log.Debug(this.LocalClassName, " id :  " + id + " , name : " + name + " , email : " + email + " photo uri : " + photoUri + " photo thumb uri: " + photoThumbnailUri);

                        break;
                    case SHARE_CONTACT:
                        uri = intent.Data;
                        id = intent.Data.LastPathSegment;

                        cursor = null;
                        photoUri = "";
                        photoThumbnailUri = "";
                        name = "";

                        try
                        {
                            String[] projection = { ContactsContract.ContactNameColumns.DisplayNamePrimary, ContactsContract.ContactsColumns.PhotoUri, ContactsContract.ContactsColumns.PhotoThumbnailUri }; // PROBLEM OVER HERE?
                            cursor = ContentResolver.Query(uri, projection,
                                null, null, null);
                            cursor.MoveToFirst();

                            if (cursor.MoveToFirst())
                            {
                                int nameColumnIndex = cursor.GetColumnIndex(ContactsContract.ContactNameColumns.DisplayNamePrimary);
                                name = cursor.GetString(nameColumnIndex);

                                int photoUriColumnIndex = cursor.GetColumnIndex(ContactsContract.ContactsColumns.PhotoUri);
                                photoUri = cursor.GetString(photoUriColumnIndex);

                                int photoThumbnailUriColumnIndex = cursor.GetColumnIndex(ContactsContract.ContactsColumns.PhotoThumbnailUri);
                                photoThumbnailUri = cursor.GetString(photoThumbnailUriColumnIndex);
                            }
                            else
                            {
                                Log.Warn("EditItemActivity", "No results");
                            }
                        }
                        catch (Exception e)
                        {
                            Log.Error("EditItemActivity", "Failed to personal data", e);
                        }
                        finally
                        {
                            if (cursor != null)
                            {
                                cursor.Close();
                            }

                            if (photoThumbnailUri != null)
                            {
                                //var photo = Android.Net.Uri.Parse(photoThumbnailUri);
                                //ImageView assignedThumb = this.FindViewById<ImageView>(Resource.Id.assigned_to_thumb);
                                //Bitmap thumbBitmap = MediaStore.Images.Media.GetBitmap(ContentResolver, photo); //.getBitmap(this.getContentResolver(), imageUri);
                                //var roundedThumbBitmap = RoundedBitmapDrawableFactory.Create(Resources, thumbBitmap);
                                //roundedThumbBitmap.Circular = true;

                                //assignedThumb.SetImageDrawable(roundedThumbBitmap);
                                //assignedThumb.SetColorFilter(null);
                                //assignedThumb.Visibility = ViewStates.Visible;
                            }
                            else
                            {
                                //ImageView assignedThumb = this.FindViewById<ImageView>(Resource.Id.assigned_to_thumb);
                                //assignedThumb.SetImageResource(Resource.Drawable.ic_account_circle_black_24dp);
                                //assignedThumb.SetColorFilter(Color.ParseColor("#A9A9A9"));//. ImageTintMode = PorterDuff.Mode.ValueOf();
                                //assignedThumb.Visibility = ViewStates.Visible;
                            }
                            if (name == null)
                            {
                                //Snackbar.Make(FindViewById(Resource.Id.edit_item_coordinator), "No name for Selected Contact", Snackbar.LengthLong).Show();
                                Toast.MakeText(this, "No name for Selected Contact", ToastLength.Long).Show();

                            }
                            else
                            {
                                var shareUser = FindViewById<EditText>(Resource.Id.user_to_share_name);
                                shareUser.Text = "";
                                shareArrayAdapter.Add(name);
                                shareArrayAdapter.NotifyDataSetChanged();
                                Log.Debug("EditItemActivity", shareArrayAdapter.Count.ToString());
                            }
                        }

                        cursor = null;
                        email = "";
                        try
                        {
                            cursor = ContentResolver.Query(ContactsContract.CommonDataKinds.Email.ContentUri, null, ContactsContract.CommonDataKinds.Email.InterfaceConsts.ContactId + "=?", new String[] { id }, null);
                            int emailColumnIndex = cursor.GetColumnIndex(ContactsContract.CommonDataKinds.Email.InterfaceConsts.Data);

                            cursor.MoveToFirst();

                            if (cursor.MoveToFirst())
                            {
                                email = cursor.GetString(emailColumnIndex);
                                Log.Verbose("EditItemActivity", "Got email: " + email);
                            }
                            else
                            {
                                Log.Warn("EditItemActivity", "No results");
                            }
                        }
                        catch (Exception e)
                        {
                            Log.Error("EditItemActivity", "Failed to get email data", e);
                        }
                        finally
                        {
                            if (cursor != null)
                            {
                                cursor.Close();
                            }
                            if (email.Length == 0)
                            {
                                //Snackbar.Make(FindViewById(Resource.Id.edit_item_coordinator), "No email for Selected Contact", Snackbar.LengthLong).Show();
                                Toast.MakeText(this, "No email for Selected Contact", ToastLength.Long).Show();
                            }
                        }

                        Log.Debug(this.LocalClassName, " id :  " + id + " , name : " + name + " , email : " + email + " photo uri : " + photoUri + " photo thumb uri: " + photoThumbnailUri);



                        break;
                    default:
                        break;
                }
            }

        }

        private void TxtDate_FocusChange(object sender, View.FocusChangeEventArgs e)
        {
            var editText = (EditText)sender;
            if (e.HasFocus)
            {
                DateDialog dialog = new DateDialog((View)sender);
                FragmentTransaction ft = FragmentManager.BeginTransaction();
                dialog.Show(ft, "DatePicker");
                editText.ClearFocus();
            }
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {

            MenuInflater.Inflate(Resource.Menu.edit_toolbar_actions, menu);
            return true;
        }

    }
}