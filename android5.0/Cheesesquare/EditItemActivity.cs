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
using Android.Text.Format;
using Android.Net;

namespace Cheesesquare
{
    //public class SubTaskArrayAdapter : ArrayAdapter<>

    [Activity]
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
        private EditText commentText;
        private ListView subTaskListView;
        private ListView shareListView;
        private ArrayAdapter<Todo.Item> subTaskArrayAdapter;
        private AutocompleteCustomArrayAdapter shareArrayAdapter;
        private List<Todo.Item> subTaskList;
        //private List<String> shareList;
        private List<Todo.User> selectedContacts;
        private Todo.User selectedContact;
        private bool groupChanged;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            groupChanged = false;

            var itemID = Intent.GetStringExtra("itemID");
            newItem = Intent.GetBooleanExtra("newItem", false);

            if (itemID != null && itemID != "")
                item = PublicFields.allItems.Find(it => it.id == itemID);

            string parentItemID = Intent.GetStringExtra("parentItemID");
            parentItem = PublicFields.allItems.Find(it => it.id == parentItemID);

            // Create your application here
            SetContentView(Resource.Layout.activity_edit_item);
            Window.SetSoftInputMode(SoftInput.AdjustResize);
            Window.SetSoftInputMode(SoftInput.StateHidden);

            itemName = FindViewById<EditText>(Resource.Id.item_name);
            itemName.AfterTextChanged += ItemName_AfterTextChanged;
            itemImportance = FindViewById<RatingBar>(Resource.Id.item_ratingbar);
            itemImportance.RatingBarChange += ItemImportance_RatingBarChange;

            subTaskList = new List<Todo.Item>();
            //shareList = new List<string>();
            selectedContacts = new List<Todo.User>();

            toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar_edit);
                
            toolbar.SetNavigationIcon(Resource.Drawable.ic_clear_white_24dp);

            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);

            subTaskListView = FindViewById<ListView>(Resource.Id.subtask_lists);
            subTaskArrayAdapter = new ArrayAdapter<Todo.Item>(this, Android.Resource.Layout.SimpleListItem1, Android.Resource.Id.Text1, subTaskList);
            subTaskArrayAdapter.SetNotifyOnChange(true);
            subTaskListView.Adapter = subTaskArrayAdapter;

            

            txtDate = (EditText)FindViewById(Resource.Id.item_date);
            txtDate.FocusChange += TxtDate_FocusChange;

            shareEditText = FindViewById<EditText>(Resource.Id.user_to_share_name);
            shareEditText.Click += ShareEditText_Click;

            commentText = FindViewById<EditText>(Resource.Id.insert_comment_text);
            if (item != null && item.Notes != null)
                commentText.Text = item.Notes;
            commentText.AfterTextChanged += CommentText_AfterTextChanged;

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

            subtaskText.KeyPress += (object sender, View.KeyEventArgs e) =>
            {
                e.Handled = false;

                // If the event is a key-down event on the "enter" button
                if (e.Event.Action == KeyEventActions.Down && e.KeyCode == Keycode.Enter)
                {
                    if (subtaskText.Text != "")
                    {
                        subtaskText.Text.Trim('\n');

                        var subItem = new Todo.Item();
                        subItem.Name = subtaskText.Text;
                        subItem.Parent = item.id;
                        subItem.OwnedBy = item.OwnedBy;

                        if (item.Type < 4)
                            subItem.Type = item.Type + 1;
                        else
                            subItem.Type = 4;

                        subTaskArrayAdapter.Add(subItem);

                        if (item.SubItems == null)
                            item.SubItems = new List<Todo.Item>();

                        item.SubItems.Add(subItem);
                        subTaskList.Add(subItem);

                        subtaskText.Text.Trim('\n');
                        subtaskText.Text = "";
                        e.Handled = true;
                    }
                    else // an immediate enter does nothing
                    {
                        subtaskText.Text = "";
                        e.Handled = true;
                    }
                }
            };

            if (itemID != null)
            {
                //item = PublicFields.allItems.Find(it => it.id == itemID);

                itemName.Text = item.Name;
                itemImportance.Rating = item.Importance;
                toolbar.Title = item.Name;
                commentText.Text = item.Notes;

                if (item.EndDate != null && item.EndDate != "")
                {
                    //String givenDateString = "Tue Apr 23 16:08:28 GMT+05:30 2013";
                    //SimpleDateFormat sdf = new SimpleDateFormat("dd-MM-yyyy");//new SimpleDateFormat("EEE MMM dd HH:mm:ss z yyyy");
                    try
                    {
                        //DateTime dt = new DateTime();
                        //Date mDate = sdf.Parse(item.EndDate);
                        long timeInMilliseconds;
                        long.TryParse(item.EndDate, out timeInMilliseconds);
                        if (timeInMilliseconds > 0)
                            txtDate.Text = DateUtils.GetRelativeTimeSpanString(Application.Context, timeInMilliseconds);
                    }
                    catch (ParseException e)
                    {
                        e.PrintStackTrace();
                    }
                }

                subTaskList.AddRange(item.SubItems);
                subTaskArrayAdapter.AddAll(item.SubItems);
            }

            item = item ?? new Todo.Item(); // if no item yet, make a new one

            if(newItem)
            {
                item.Parent = parentItem.id;
                item.OwnedBy = PublicFields.Database.defGroup.ID;

                if (parentItem.Type < 4)
                    item.Type = parentItem.Type + 1;
                else // type does not go lower than 4 (task)
                    item.Type = 4;

                toolbar.Title = "New item";
            }

            
        }

        private void CommentText_AfterTextChanged(object sender, Android.Text.AfterTextChangedEventArgs e)
        {
            item.Notes = commentText.Text;
        }

        private void ItemImportance_RatingBarChange(object sender, RatingBar.RatingBarChangeEventArgs e)
        {
            item.Importance = (int) itemImportance.Rating;
        }

        private void ItemName_AfterTextChanged(object sender, Android.Text.AfterTextChangedEventArgs e)
        {
            toolbar.Title = itemName.Text;
            item.Name = itemName.Text;
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
                            selectedContact = JsonConvert.DeserializeObject<Todo.User>(member);
                            selectedContact = CircleBitmap.addPhotoThumbs(selectedContact); // add the photo thumbnails to the contact

                            Log.Debug("EditItemActivity", "back to edit item activity! " + selectedContact.Name);

                            var roundedThumbBitmap = RoundedBitmapDrawableFactory.Create(Application.Context.Resources, selectedContact.Thumbnail);
                            roundedThumbBitmap.Circular = true;
                            assignedThumb.SetImageDrawable(roundedThumbBitmap);

                            assignedTo.Text = selectedContact.Name;
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
                            selectedContacts = JsonConvert.DeserializeObject<List<Todo.User>>(members);
                            selectedContacts = CircleBitmap.addPhotoThumbs(selectedContacts); // add all the photo thumbnails to the contacts

                            var groupName = intent.GetStringExtra("groupname");
                            Log.Debug("EditItemActivity", "back to edit item activity! " + members + groupName);

                            shareArrayAdapter.Clear();
                            shareArrayAdapter.AddAll(selectedContacts);
                            shareArrayAdapter.NotifyDataSetChanged();
                            shareEditText.Text = groupName;

                            groupChanged = true;
                        }
                        else if (intent.GetStringExtra("member") != null)
                        {
                            var member = intent.GetStringExtra("member");
                            selectedContact = JsonConvert.DeserializeObject<Todo.User>(member);
                            selectedContact = CircleBitmap.addPhotoThumbs(selectedContact); // add the photo thumbnails to the contact

                            Log.Debug("EditItemActivity", "back to edit item activity! " + selectedContact.Name);

                            shareArrayAdapter.Clear();
                            selectedContacts.Clear();
                            selectedContacts.Add(selectedContact);
                            //shareArrayAdapter.Add(selectedContact); // don't add anything to the listview
                            shareArrayAdapter.NotifyDataSetChanged();
                            shareEditText.Text = selectedContact.Name;
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
                DateDialog dialog = new DateDialog((View)sender, item);
                FragmentTransaction ft = FragmentManager.BeginTransaction();
                dialog.Show(ft, "DatePicker");
                editText.ClearFocus();
            }
            
            //item.EndDate = editText.Text;
        }

        public override bool OnOptionsItemSelected(IMenuItem menuItem)
        {
            switch (menuItem.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    return true;
                case Resource.Id.edit_done:

                    if (string.IsNullOrEmpty(item.Name))
                    {
                        new Android.Support.V7.App.AlertDialog.Builder(this)
                        .SetMessage("Name is empty!")
                        .SetCancelable(true)
                        .Show();
                        return false;
                    }
                    else
                    {
                        Intent returnIntent = new Intent();

                        if (selectedContacts != null && selectedContacts.Count > 0)
                        {
                            returnIntent.PutExtra("groupChanged", groupChanged);
                            returnIntent.PutExtra("selectedContacts", JsonConvert.SerializeObject(selectedContacts));
                            returnIntent.PutExtra("groupName", shareEditText.Text);
                        }

                        if (newItem)
                        {
                            //item = new Todo.Item();
                            //item.Name = itemName.Text;
                            //item.Importance = (int)itemImportance.Rating;



                            //item.SubItems = subTaskList;
                            //item.Notes = commentText.Text;

                            // add to db and update item locally
                            //PublicFields.Database.SaveItem(item);

                            //var getItemTask = PublicFields.Database.GetItem(item.id);
                            //item = getItemTask.Result;
                            //item.SubItems = subTaskList;

                            // add to local memory
                            PublicFields.allItems.Add(item);

                            // add to subtasks field of parent item
                            var parentIndex = PublicFields.allItems.FindIndex(it => it.id == item.Parent);
                            var parentUpdated = PublicFields.allItems.Find(it => it.id == item.Parent);
                            parentUpdated.SubItems.Add(item);
                            PublicFields.allItems[parentIndex] = parentUpdated;
                        }
                        else
                        {
                            //item.Name = itemName.Text;
                            //item.Importance = (int)itemImportance.Rating;

                            returnIntent.PutExtra("edited", true);

                            //item.SubItems = subTaskList;
                            //item.Notes = commentText.Text;

                            // add to db and update item locally
                            //PublicFields.Database.SaveItem(item);
                            //var getItemTask = PublicFields.Database.GetItem(item.id);
                            //item = getItemTask.Result;
                            //item.SubItems = subTaskList;

                            int index = PublicFields.allItems.FindIndex(it => it.id == item.id);
                            PublicFields.allItems[index] = item;

                            // update in subitems of parent
                            var parentIndex = PublicFields.allItems.FindIndex(it => it.id == item.Parent);
                            var parent = PublicFields.allItems.Find(it => it.id == item.Parent);
                            var itemIndexInSubItems = parent.SubItems.FindIndex(it => it.id == item.id);
                            parent.SubItems[itemIndexInSubItems] = item;
                            PublicFields.allItems[parentIndex] = parent;
                        }

                        if (string.IsNullOrEmpty(item.id))
                            item.id = "temporaryID";

                        returnIntent.PutExtra("itemID", item.id);


                        SetResult(Result.Ok, returnIntent);

                        Finish();
                        return true;
                    }

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