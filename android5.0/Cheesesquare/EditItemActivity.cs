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



        public const string EXTRA_NAME = "cheese_name";
        private readonly int PICK_CONTACT = 99;
        private EditText subtaskText;
        private EditText assignEditText;
        private EditText shareEditText;
        private EditText txtDate;
        private ListView subTaskListView;
        private ArrayAdapter<String> subTaskArrayAdapter;
        private List<String> subTaskList;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            var cheeseName = Intent.GetStringExtra(EXTRA_NAME);

            // Create your application here
            SetContentView(Resource.Layout.activity_edit_item);
            Window.SetSoftInputMode(SoftInput.AdjustResize);
            Window.SetSoftInputMode(SoftInput.StateHidden);

            subTaskList = new List<string>();

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

            shareEditText = FindViewById<EditText>(Resource.Id.add_user_text);
            shareEditText.FocusChange += shareAssign_FocusChange;

            assignEditText = FindViewById<EditText>(Resource.Id.assigned_to_text);
            assignEditText.FocusChange += shareAssign_FocusChange; ;

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
                
                StartActivityForResult(intent, PICK_CONTACT);//PICK_CONTACT is private static final int, so declare in activity class
                editText.ClearFocus();
            }
        }

        protected override void OnActivityResult(int requestCode, Result resultCode,
        Intent intent)
        {
            base.OnActivityResult(requestCode, resultCode, intent);
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
                    String[] projection = { ContactsContract.ContactNameColumns.DisplayNamePrimary, ContactsContract.}; // PROBLEM OVER HERE?

                    var cursor = ContentResolver.Query(uri, null,
                            null, null, null);
                    cursor.MoveToFirst();
                    String[] names = cursor.GetColumnNames();

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