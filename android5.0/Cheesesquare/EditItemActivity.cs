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

namespace Cheesesquare
{
    [Activity(Label = "EditItemActivity")]
    public class EditItemActivity : AppCompatActivity
    {
        private Android.Support.V7.Widget.Toolbar toolbar;

        EditText assignEditText;
        EditText shareEditText;
        EditText txtDate;

        public const string EXTRA_NAME = "cheese_name";
        private readonly int PICK_CONTACT = 1;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            var cheeseName = Intent.GetStringExtra(EXTRA_NAME);

            // Create your application here
            SetContentView(Resource.Layout.activity_edit_item);

            toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.appbar_edit);
            toolbar.Title = cheeseName;
            toolbar.SetNavigationIcon(Resource.Drawable.ic_clear_white_24dp);

            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);

        }

        protected override void OnStart()
        {
            base.OnStart();
            txtDate = (EditText)FindViewById(Resource.Id.item_date);
            txtDate.FocusChange += TxtDate_FocusChange;

            shareEditText = FindViewById<EditText>(Resource.Id.add_user_text);
            shareEditText.FocusChange += shareAssign_FocusChange;

            assignEditText = FindViewById<EditText>(Resource.Id.assigned_to_text);
            assignEditText.FocusChange += shareAssign_FocusChange; ;
        }

        private void shareAssign_FocusChange(object sender, View.FocusChangeEventArgs e)
        {
            var editText = (EditText)sender;
            Intent intent = new Intent(Intent.ActionPick, ContactsContract.Contacts.ContentUri);
            if (e.HasFocus)
            {
                StartActivityForResult(intent, PICK_CONTACT);//PICK_CONTACT is private static final int, so declare in activity class
            }
            else
            {
                SetResult(Result.Ok, intent);
                FinishActivity(PICK_CONTACT);
            }
            
        }

        protected override void OnActivityResult(int requestCode, Result resultCode,
        Intent intent)
        {
            if (requestCode == PICK_CONTACT)
            {
                if (resultCode == Result.Ok)
                {
                    //Android.Net.Uri contactData = intent.Data;
                    //Cursor c = ManagedQuery(contactData, null, null, null, null);
                    //if (c.moveToFirst())
                    //{


                    //    String id = c.getString(c.getColumnIndexOrThrow(ContactsContract.Contacts._ID));

                    //    String hasPhone = c.getString(c.getColumnIndex(ContactsContract.Contacts.HAS_PHONE_NUMBER));

                    //    if (hasPhone.equalsIgnoreCase("1"))
                    //    {
                    //        Cursor phones = getContentResolver().query(
                    //                     ContactsContract.CommonDataKinds.Phone.CONTENT_URI, null,
                    //                     ContactsContract.CommonDataKinds.Phone.CONTACT_ID + " = " + id,
                    //                     null, null);
                    //        phones.moveToFirst();
                    //        cNumber = phones.getString(phones.getColumnIndex("data1"));
                    //        System.out.println("number is:" + cNumber);
                    //    }
                    //    String name = c.getString(c.getColumnIndex(ContactsContract.Contacts.DISPLAY_NAME));

                    Android.Net.Uri uri = intent.Data;
                    String[] projection = { ContactsContract.CommonDataKinds.Phone.Number, ContactsContract.CommonDataKinds.StructuredName.DisplayName}; // PROBLEM OVER HERE?

                    var cursor = ContentResolver.Query(uri, projection,
                            null, null, null);
                    cursor.MoveToFirst();

                    int numberColumnIndex = cursor.GetColumnIndex(ContactsContract.CommonDataKinds.Phone.Number);
                    String number = cursor.GetString(numberColumnIndex);

                    int nameColumnIndex = cursor.GetColumnIndex(ContactsContract.CommonDataKinds.StructuredName.DisplayName);
                    String name = cursor.GetString(nameColumnIndex);

                    Log.Debug(this.LocalClassName, "ZZZ number : " + number + " , name : " + name);

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