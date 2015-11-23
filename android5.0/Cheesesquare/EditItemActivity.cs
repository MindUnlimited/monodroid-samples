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
using Newtonsoft.Json;

namespace Cheesesquare
{
    [Activity(Label = "New Item")]
    public class EditItemActivity : AppCompatActivity
    {
        private Android.Support.V7.Widget.Toolbar toolbar;

        private Todo.Item item;
        private Todo.Item parentItem;

        private bool newItem;


        //public const string EXTRA_NAME = "cheese_name";
        private const int ASSIGN_CONTACT = 100;
        private const int SHARE_CONTACT = 101;

        private EditText itemName;
        private RatingBar itemImportance;
        private EditText subtaskText;
        private EditText assignEditText;
        private EditText shareEditText;
        private EditText txtDate;
        private ListView subTaskListView;
        private ListView shareListView;
        private ArrayAdapter<String> subTaskArrayAdapter;
        private AutocompleteCustomArrayAdapter shareArrayAdapter;
        private List<String> subTaskList;
        private List<String> shareList;
        private List<Contact> selectedContacts;
        private Contact selectedContact;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            var itemID = Intent.GetStringExtra("itemID");
            newItem = Intent.GetBooleanExtra("newItem", false);

            if (itemID != null && itemID != "")
                item = PublicFields.allItems.Find(it => it.ID == itemID);

            string parentItemID = Intent.GetStringExtra("parentItemID");
            parentItem = PublicFields.allItems.Find(it => it.ID == parentItemID);

            // Create your application here
            SetContentView(Resource.Layout.activity_edit_item);
            Window.SetSoftInputMode(SoftInput.AdjustResize);
            Window.SetSoftInputMode(SoftInput.StateHidden);

            itemName = FindViewById<EditText>(Resource.Id.item_name);
            itemName.AfterTextChanged += ItemName_AfterTextChanged;
            itemImportance = FindViewById<RatingBar>(Resource.Id.item_ratingbar);

            subTaskList = new List<string>();
            shareList = new List<string>();
            selectedContacts = new List<Contact>();

            toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar_edit);
                
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
            shareEditText.Click += ShareEditText_Click;

            shareListView = FindViewById<ListView>(Resource.Id.user_to_share_listview);
            //shareArrayAdapter = new ArrayAdapter<String>(this, Android.Resource.Layout.SimpleListItem1, Android.Resource.Id.Text1, shareList);
            //shareArrayAdapter.SetNotifyOnChange(true);
            //shareListView.Adapter = shareArrayAdapter;
            shareArrayAdapter = new AutocompleteCustomArrayAdapter(this, Resource.Layout.contact_list_item_small, selectedContacts);
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

            if (itemID != null)
            {
                item = PublicFields.allItems.Find(it => it.ID == itemID);

                itemName.Text = item.Name;
                txtDate.Text = item.EndDate;
                itemImportance.Rating = item.Importance;
                toolbar.Title = item.Name;

                subTaskArrayAdapter.AddAll(item.SubItems);
            }
        }

        private void ItemName_AfterTextChanged(object sender, Android.Text.AfterTextChangedEventArgs e)
        {
            toolbar.Title = itemName.Text;
        }

        private void ShareEditText_Click(object sender, EventArgs e)
        {
            var editText = (EditText)sender;

            var intent = new Intent(this, typeof(SelectContactsActivity));
            StartActivityForResult(intent, SHARE_CONTACT);
        }

        private void ThumbAndName_Click(object sender, EventArgs e)
        {
            Intent intent = new Intent(this, typeof(SelectContactsActivity));//(Intent.ActionPick, ContactsContract.Contacts.ContentUri);
            intent.PutExtra("members", JsonConvert.SerializeObject(selectedContacts));
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

        protected override void OnActivityResult(int requestCode, Result resultCode,
        Intent intent)
        {
            base.OnActivityResult(requestCode, resultCode, intent);

            if (resultCode == Result.Ok)
            {
                ImageView assignedThumb = this.FindViewById<ImageView>(Resource.Id.assigned_to_thumb);
                EditText assignedTo = FindViewById<EditText>(Resource.Id.assigned_to_name);

                switch (requestCode)
                {
                    case ASSIGN_CONTACT:
                        if (intent.GetStringExtra("member") != null)
                        {
                            var member = intent.GetStringExtra("member");
                            selectedContact = JsonConvert.DeserializeObject<Contact>(member);
                            selectedContact = CircleBitmap.addPhotoThumbs(selectedContact); // add the photo thumbnails to the contact

                            Log.Debug("EditItemActivity", "back to edit item activity! " + selectedContact.DisplayName);

                            var roundedThumbBitmap = RoundedBitmapDrawableFactory.Create(Application.Context.Resources, selectedContact.PhotoThumb);
                            roundedThumbBitmap.Circular = true;
                            assignedThumb.SetImageDrawable(roundedThumbBitmap);

                            assignedTo.Text = selectedContact.DisplayName;
                        }
                        break;
                    case SHARE_CONTACT:
                        // clear assigned to field first
                        //assignedThumb = this.FindViewById<ImageView>(Resource.Id.assigned_to_thumb);
                        assignedThumb.SetImageDrawable(null);

                        //EditText assignedTo = FindViewById<EditText>(Resource.Id.assigned_to_name);
                        assignedTo.Text = "";

                        if (intent.GetStringExtra("members") != null)
                        {
                            var members = intent.GetStringExtra("members");
                            selectedContacts = JsonConvert.DeserializeObject<List<Contact>>(members);
                            selectedContacts = CircleBitmap.addPhotoThumbs(selectedContacts); // add all the photo thumbnails to the contacts

                            var groupName = intent.GetStringExtra("groupname");
                            Log.Debug("EditItemActivity", "back to edit item activity! " + members + groupName);

                            shareArrayAdapter.Clear();
                            shareArrayAdapter.AddAll(selectedContacts);
                            shareArrayAdapter.NotifyDataSetChanged();
                            shareEditText.Text = groupName;
                        }
                        else if (intent.GetStringExtra("member") != null)
                        {
                            var member = intent.GetStringExtra("member");
                            selectedContact = JsonConvert.DeserializeObject<Contact>(member);
                            selectedContact = CircleBitmap.addPhotoThumbs(selectedContact); // add the photo thumbnails to the contact

                            Log.Debug("EditItemActivity", "back to edit item activity! " + selectedContact.DisplayName);

                            shareArrayAdapter.Clear();
                            selectedContacts.Clear();
                            selectedContacts.Add(selectedContact);
                            //shareArrayAdapter.Add(selectedContact); // don't add anything to the listview
                            shareArrayAdapter.NotifyDataSetChanged();
                            shareEditText.Text = selectedContact.DisplayName;
                        }
                        else
                        {
                            Log.Warn("EditItemActivity", "returned to edit item activity but no contacts selected?");
                        }
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
            item.EndDate = editText.Text;
        }

        public override bool OnOptionsItemSelected(IMenuItem menuItem)
        {
            switch (menuItem.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    return true;
                case Resource.Id.edit_done:
                    Intent returnIntent = new Intent();

                    if (newItem)
                    {
                        item = new Todo.Item();
                        item.Name = itemName.Text;
                        item.Importance = (int)itemImportance.Rating;
                        item.Parent = parentItem.ID;
                        item.OwnedBy = PublicFields.Database.defGroup.ID;

                        if (parentItem.Type < 4)
                            item.Type = parentItem.Type + 1;
                        else // type does not go lower than 4 (task)
                            item.Type = 4;

                        // add to db
                        PublicFields.Database.SaveItem(item);

                        // add to local memory
                        PublicFields.allItems.Add(item);

                        // add to subtasks field of parent item
                        var parentIndex = PublicFields.allItems.FindIndex(it => it.ID == item.Parent);
                        var parentUpdated = PublicFields.allItems.Find(it => it.ID == item.Parent);
                        parentUpdated.SubItems.Add(item);
                        PublicFields.allItems[parentIndex] = parentUpdated;
                    }
                    else
                    {
                        item.Name = itemName.Text;
                        item.Importance = (int)itemImportance.Rating;

                        returnIntent.PutExtra("edited", true);
                        PublicFields.Database.SaveItem(item);

                        int index = PublicFields.allItems.FindIndex(it => it.ID == item.ID);
                        PublicFields.allItems[index] = item;
                    }
                    
                    returnIntent.PutExtra("itemID", item.ID);

                    SetResult(Result.Ok, returnIntent);

                    Finish();
                    return true;
            }
            return base.OnOptionsItemSelected(menuItem);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {

            MenuInflater.Inflate(Resource.Menu.edit_toolbar_actions, menu);
            return true;
        }

    }
}