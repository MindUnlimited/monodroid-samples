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
using Android.Provider;
using Android.Util;
using Android.Graphics;
using Android.Content.Res;
using Android.Support.V4.Graphics.Drawable;
using Android.Support.V7.App;
using Android.Database;
using Android.Support.V7.Widget;
using V7Toolbar = Android.Support.V7.Widget.Toolbar;
using Java.Util;
using System.Xml.Serialization;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Cheesesquare
{

    public static class CircleBitmap
    {
        public static List<Todo.User> addPhotoThumbs(List<Todo.User> contacts)
        {
            var noThumb = CircleBitmap.getCircleBitmap(BitmapFactory.DecodeResource(Application.Context.Resources, Resource.Drawable.ic_person_white_24dp));

            foreach (Todo.User contact in contacts)
            {
                if(contact.Thumbnail == null)
                {
                    if (contact.PhotoId == null)
                    {
                        //h.ImageView.SetImageResource(Resource.Drawable.ic_account_circle_black_24dp);
                        //h.ImageView.SetColorFilter(Color.ParseColor("#A9A9A9"));

                        contact.Thumbnail = noThumb;
                    }
                    else
                    {
                        //var contactUri = ContentUris.WithAppendedId(
                        //    ContactsContract.Contacts.ContentUri, contact.Id);
                        //var contactPhotoUri = Android.Net.Uri.WithAppendedPath(contactUri,
                        //    Contacts.Photos.ContentDirectory);

                        var contactPhotoUri = Android.Net.Uri.Parse(contact.PhotoId);
                        Bitmap thumbBitmap = MediaStore.Images.Media.GetBitmap(Application.Context.ContentResolver, contactPhotoUri); //.getBitmap(this.getContentResolver(), imageUri);
                                                                                                                                      //var roundedThumbBitmap = RoundedBitmapDrawableFactory.Create(Application.Context.Resources, thumbBitmap);
                                                                                                                                      //roundedThumbBitmap.Circular = true;

                        contact.Thumbnail = thumbBitmap;
                    }
                }
            }
            return contacts;
        }

        public static Todo.User addPhotoThumbs(Todo.User contact)
        {
            var noThumb = CircleBitmap.getCircleBitmap(BitmapFactory.DecodeResource(Application.Context.Resources, Resource.Drawable.ic_person_white_24dp));

                if (contact.PhotoId == null)
                {
                    //h.ImageView.SetImageResource(Resource.Drawable.ic_account_circle_black_24dp);
                    //h.ImageView.SetColorFilter(Color.ParseColor("#A9A9A9"));

                    contact.Thumbnail = noThumb;
                }
                else
                {
                    //var contactUri = ContentUris.WithAppendedId(
                    //    ContactsContract.Contacts.ContentUri, contact.Id);
                    //var contactPhotoUri = Android.Net.Uri.WithAppendedPath(contactUri,
                    //    Contacts.Photos.ContentDirectory);

                    var contactPhotoUri = Android.Net.Uri.Parse(contact.PhotoId);
                    Bitmap thumbBitmap = MediaStore.Images.Media.GetBitmap(Application.Context.ContentResolver, contactPhotoUri); //.getBitmap(this.getContentResolver(), imageUri);
                                                                                                                                  //var roundedThumbBitmap = RoundedBitmapDrawableFactory.Create(Application.Context.Resources, thumbBitmap);
                                                                                                                                  //roundedThumbBitmap.Circular = true;

                    contact.Thumbnail = thumbBitmap;
                }
            return contact;
        }

        public static Bitmap getCircleBitmap(Bitmap bitmap)
        {
            return getCircleBitmap(bitmap, Color.ParseColor("#A9A9A9")); // default is gray
        }

        public static Bitmap getCircleBitmap(Bitmap bitmap, Color color)
        {
            //var imageView = _activity.FindViewById<ImageView>(Resource.Id.ContactImage);
            //int width = imageView.Width;
            //int height = imageView.Height;

            var test = BitmapFactory.DecodeResource(Application.Context.Resources, Resource.Drawable.ic_group_add_white_48dp);
            int width = test.Width;
            int height = test.Height;

            Bitmap output = Bitmap.CreateBitmap(width,
             height, Bitmap.Config.Argb8888);
            Canvas canvas = new Canvas(output);

            Paint paint = new Paint();
            Rect rect = new Rect(0, 0, width, height);
            RectF rectF = new RectF(rect);

            paint.AntiAlias = true;
            canvas.DrawARGB(0, 0, 0, 0);
            paint.Color = color;
            canvas.DrawOval(rectF, paint);

            int cx = (width - bitmap.Width) >> 1; // same as (...) / 2
            int cy = (height - bitmap.Height) >> 1;

            paint.SetXfermode(new PorterDuffXfermode(PorterDuff.Mode.SrcOver));
            //canvas.DrawBitmap(bitmap, matrix, paint);
            //canvas.DrawBitmap(bitmap, rect, rect, paint);
            canvas.DrawBitmap(bitmap, cx, cy, paint);

            bitmap.Recycle();

            return output;
        }
    }

    //public class Contact : IComparable<Contact>
    //{

    //    public long Id { get; set; }
    //    public string DisplayName { get; set; }
    //    public string PhotoId { get; set; }
    //    [JsonIgnore]
    //    public Bitmap PhotoThumb { get; set; }
    //    public string Email { get; set; }

    //    public int CompareTo(Contact other)
    //    {
    //        if (this.DisplayName != null && other.DisplayName != null)
    //        {
    //            if (DisplayName.ToUpper()[0] < other.DisplayName.ToUpper()[0])
    //                return -1;
    //            if (DisplayName.ToUpper()[0] == other.DisplayName.ToUpper()[0])
    //                return 0;
    //            if (DisplayName.ToUpper()[0] > other.DisplayName.ToUpper()[0])
    //                return 1;
    //        }
    //        return -1;

    //    }

    //    public override string ToString()
    //    {
    //        return DisplayName;//System.String.Format("Id: {0}\tName: {1}\tEmail: {2}\tPhotoId: {3}", Id, DisplayName, Email, PhotoId);
    //    }
    //}

    [Activity(Label = "SelectContactsActivity")]
    public class SelectContactsActivity : AppCompatActivity
    {
        private RecyclerView contactsRecyclerView;
        private ContactsRecyclerAdapter recyclerAdapter;
        private RecyclerView.LayoutManager recyclerLayoutManager;
        private const int SHARE_CONTACT = 101;
        private List<Todo.User> members;

        private Todo.User currentUser;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Create your application here
            SetContentView(Resource.Layout.activity_select_contacts);

            

            var toolbar = FindViewById<V7Toolbar>(Resource.Id.contacts_toolbar);
            SetSupportActionBar(toolbar);

            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);


            //ContactsAdapter contactsAdapter = new ContactsAdapter(this);
            //ListView contactsListView = FindViewById<ListView>(Resource.Id.ContactsListView);
            //contactsListView.Adapter = contactsAdapter;

            //LinearLayout groupItem = FindViewById<LinearLayout>(Resource.Id.group_item);
            //groupItem.SetBackgroundColor(Color.ParseColor("#D1C4E9"));
            //groupItem.Click += GroupItem_Click;

            //var groupImage = groupItem.FindViewById<ImageView>(Resource.Id.ContactImage);
            //var groupName = groupItem.FindViewById<TextView>(Resource.Id.ContactName);

            //var groupThumb = CircleBitmap.getCircleBitmap(BitmapFactory.DecodeResource(Application.Context.Resources, Resource.Drawable.ic_group_add_white_24dp), Application.Context.Resources.GetColor(Resource.Color.colorAccent));
            //groupImage.SetImageBitmap(groupThumb);
            //groupName.Text = "Group";

            contactsRecyclerView = FindViewById<RecyclerView>(Resource.Id.contacts_recycler_view);

            // use this setting to improve performance if you know that changes
            // in content do not change the layout size of the RecyclerView
            contactsRecyclerView.HasFixedSize = true;

            // set the dividers between the items (contacts)
            contactsRecyclerView.AddItemDecoration(new DividerItemDecoration(Application.Context, Resource.Drawable.line_divider));

            var contactList = Intent.GetStringExtra("members");
            if (contactList != null) // assign operation
            {
                members = JsonConvert.DeserializeObject<List<Todo.User>>(contactList);
                members.Insert(0, PublicFields.Database.defUser);
                members = CircleBitmap.addPhotoThumbs(members);
                recyclerAdapter = new ContactsRecyclerAdapter(this, contactsRecyclerView, members);
            }
            else // share operation
            {
                recyclerAdapter = new ContactsRecyclerAdapter(this, contactsRecyclerView);
            }

            // specify an adapter (see also next example)
            
            recyclerAdapter.ItemClick += OnItemClick;
            contactsRecyclerView.SetAdapter(recyclerAdapter);

            // use a linear layout manager
            recyclerLayoutManager = new LinearLayoutManager(this);
            contactsRecyclerView.SetLayoutManager(recyclerLayoutManager);

            // use a fast scroller
            FastScroller fastScroller = FindViewById<FastScroller>(Resource.Id.fastscroller);
            fastScroller.SetRecyclerView(contactsRecyclerView);
            fastScroller.BringToFront();
        }

        private void OnItemClick(object sender, int e)
        {
            var adapter = sender as ContactsRecyclerAdapter;
            var contact = adapter.GetValueAt(e);
            if (contact.Name == "Group" && contact.Email == null)
            {
                var intent = new Intent(this, typeof(DefineGroupActivity));

                var contacts = recyclerAdapter.GetContactList();
                var jsonContacts = JsonConvert.SerializeObject(contacts);
                //    , Formatting.None,
                //new JsonSerializerSettings()
                //{
                //    PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                //    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                //});

                intent.PutExtra("contacts", jsonContacts);

                StartActivityForResult(intent, SHARE_CONTACT);

            }
            else
            {
                Intent myIntent = new Intent();
                myIntent.PutExtra("member", JsonConvert.SerializeObject(contact));
                SetResult(Result.Ok, myIntent);
                Finish();
            }

            //Log.Debug("SelectContactActivity", contact.ToString());
            //var view = contactsRecyclerView.GetChildAt(e);
            //view.SetBackgroundColor(Color.AliceBlue);
            //var contactView = contactsRecyclerView.GetChildAt(e);
            //contactView.SetBackgroundColor(Color.AliceBlue);
        }

        protected override void OnActivityResult(int requestCode, Result resultCode,
Intent intent)
        {
            base.OnActivityResult(requestCode, resultCode, intent);

            if (resultCode == Result.Ok)
            {
                switch (requestCode)
                {
                    case SHARE_CONTACT: // made a new group
                        var members = intent.GetStringExtra("members");
                        var groupName = intent.GetStringExtra("groupname");

                        Intent myIntent = new Intent();
                        myIntent.PutExtra("members", members);
                        myIntent.PutExtra("groupname", groupName);
                        SetResult(Result.Ok, myIntent);
                        Finish();

                        Log.Debug(this.LocalClassName, "Select contacts");

                        break;
                    default:
                        break;
                }
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
    }



    public class ContactsRecyclerAdapter : BaseRecyclerAdapter
    {
        Activity _activity;
        List<Todo.User> _contactList;
        RecyclerView _recyclerview;
        Todo.User _currentUser;
        public Todo.User currentUser
        {
	        get
	        {
                if (_currentUser == null)
                {
                    var currentUserContact = _contactList.Find(contact => contact.Email == PublicFields.Database.defUser.Email);
                    _currentUser = currentUserContact;
                    return currentUserContact;
                }
	            else
                    return this._currentUser;
	        }
	        set
	        {
	            this._currentUser = value;
	        }
        }

        //Create an Event so that our our clients can act when a user clicks
        //on each individual item.
        public event EventHandler<int> ItemClick;

        public ContactsRecyclerAdapter(Activity activity, RecyclerView recyclerView) // share constructor
        {
            _activity = (SelectContactsActivity)activity;
            _recyclerview = recyclerView;
            FillContacts();
            _contactList.Sort();

            //_contactList.Remove(currentUser);

            var groupThumb = CircleBitmap.getCircleBitmap(BitmapFactory.DecodeResource(Application.Context.Resources, Resource.Drawable.ic_group_add_white_24dp), Application.Context.Resources.GetColor(Resource.Color.colorAccent));
            _contactList.Insert(0, new Todo.User { Name = "Group", Thumbnail = groupThumb });
        }

        public ContactsRecyclerAdapter(Activity activity, RecyclerView recyclerView, List<Todo.User> contactList) // assign constructor
        {
            _activity = (SelectContactsActivity)activity;
            _recyclerview = recyclerView;
            _contactList = contactList;
        }

        //This will fire any event handlers that are registered with our ItemClick
        //event.
        private void OnClick(int position)
        {
            if (ItemClick != null)
            {
                ItemClick(this, position);
            }
        }

        public List<Todo.User> GetContactList()
        {
            return _contactList;
        }

        void FillContacts()
        {
            var db = new Database();
            db.getContacts();

            var uri = ContactsContract.Contacts.ContentUri;

            string[] projection = {
                ContactsContract.Contacts.InterfaceConsts.Id,
                ContactsContract.Contacts.InterfaceConsts.DisplayName,
                ContactsContract.ContactsColumns.PhotoThumbnailUri
                };

            var cursor = Application.Context.ContentResolver.Query(uri, projection, null,
                null, null);


            _contactList = new List<Todo.User>();
            var noThumb = CircleBitmap.getCircleBitmap(BitmapFactory.DecodeResource(Application.Context.Resources, Resource.Drawable.ic_person_white_24dp));

            try
            {
                if (cursor.MoveToFirst())
                {
                    do
                    {
                        var Id = cursor.GetLong(
                        cursor.GetColumnIndex(projection[0]));

                        var PhotoId = cursor.GetString(
                            cursor.GetColumnIndex(projection[2]));


                        ICursor cursor_email = null;
                        string email = "";
                        try
                        {
                            cursor_email = Application.Context.ContentResolver.Query(ContactsContract.CommonDataKinds.Email.ContentUri, null, ContactsContract.CommonDataKinds.Email.InterfaceConsts.ContactId + "=?", new string[] { Id.ToString() }, null);
                            int emailColumnIndex = cursor_email.GetColumnIndex(ContactsContract.CommonDataKinds.Email.Address);

                            if (cursor_email.MoveToFirst())
                            {
                                email = cursor_email.GetString(emailColumnIndex);
                                //Log.Verbose("EditItemActivity", "Got email: " + email);
                            }
                            //else
                            //{
                            //    Log.Warn("EditItemActivity", "No email found for this account");
                            //}
                        }
                        catch (Java.Lang.Exception e)
                        {
                            Log.Error("EditItemActivity", "Failed to get email data", e);
                        }
                        finally
                        {
                            if (cursor_email != null)
                            {
                                cursor_email.Close();
                            }
                            if (email.Length == 0)
                            {
                                //Snackbar.Make(FindViewById(Resource.Id.edit_item_coordinator), "No email for Selected Contact", Snackbar.LengthLong).Show();
                                //Toast.MakeText(this, "No email for Selected Contact", ToastLength.Long).Show();
                            }
                        }

                        Bitmap PhotoThumb = null;
                        if (PhotoId == null)
                        {
                            //h.ImageView.SetImageResource(Resource.Drawable.ic_account_circle_black_24dp);
                            //h.ImageView.SetColorFilter(Color.ParseColor("#A9A9A9"));

                            PhotoThumb = noThumb;
                        }
                        else
                        {
                            //var contactUri = ContentUris.WithAppendedId(
                            //    ContactsContract.Contacts.ContentUri, contact.Id);
                            //var contactPhotoUri = Android.Net.Uri.WithAppendedPath(contactUri,
                            //    Contacts.Photos.ContentDirectory);

                            var contactPhotoUri = Android.Net.Uri.Parse(PhotoId);
                            Bitmap thumbBitmap = MediaStore.Images.Media.GetBitmap(Application.Context.ContentResolver, contactPhotoUri); //.getBitmap(this.getContentResolver(), imageUri);
                            //var roundedThumbBitmap = RoundedBitmapDrawableFactory.Create(Application.Context.Resources, thumbBitmap);
                            //roundedThumbBitmap.Circular = true;

                            PhotoThumb = thumbBitmap;
                        }


                        if (email != null && email.Length > 0)
                        {
                            var contact = new Todo.User
                            {
                                //Id = Id,
                                Email = email,
                                Name = cursor.GetString(cursor.GetColumnIndex(projection[1])),
                                PhotoId = PhotoId,
                                Thumbnail = PhotoThumb
                            };

                            if (email == PublicFields.Database.defUser.Email) // found the current user
                                currentUser = contact;

                            if (!_contactList.Contains(contact))
                                _contactList.Insert(0, contact); // insert current user at top
                        }
                    } while (cursor.MoveToNext());
                }
            }
            catch (Java.Lang.Exception e)
            {
                Log.Error("EditItemActivity", "Failed to get email data", e);
            }
            finally
            {
                if (cursor != null)
                {
                    cursor.Close();
                }
            }

        }

        public Todo.User GetValueAt(int position)
        {
            return _contactList[position];
        }

        public override int ItemCount
        {
            get
            {
                return _contactList.Count;
            }
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            // - get element from your dataset at this position
            // - replace the contents of the view with that element
            var h = holder as ViewHolder;
            var contact = GetValueAt(h.AdapterPosition);

            h.TextView.Text = contact.Name;

            var roundedThumbBitmap = RoundedBitmapDrawableFactory.Create(Application.Context.Resources, contact.Thumbnail);
            roundedThumbBitmap.Circular = true;
            h.ImageView.SetImageDrawable(roundedThumbBitmap);

            //h.View.Click += View_Click;

            //h.View.Click += (sender, e) =>
            //{
            //    Log.Debug("SelectContactsActivity", contact.ToString());
            //    h.View.SetBackgroundColor(Color.Aqua);
            //};
        }

        //private void View_Click(object sender, EventArgs e)
        //{
        //    var view = sender as View;

        //    int position = _recyclerview.GetChildLayoutPosition(view);
        //    var contact = GetValueAt(position);
        //    Log.Debug("SelectContactsActivity", contact.ToString());

        //    //view.SetBackgroundColor(Color.AliceBlue);
        //}

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var view = LayoutInflater.From(parent.Context)
                .Inflate(Resource.Layout.contact_list_item, parent, false);

            return new ViewHolder(view, OnClick);
        }

        public override string GetTextToShowInBubble(int pos)
        {
            if (_contactList[pos].Name != null)
                return _contactList[pos].Name.ToUpper()[0].ToString();
            return "";
        }

    }


    public class ViewHolder : RecyclerView.ViewHolder
    {
        public View View { get; set; }
        public TextView TextView { get; set; }
        public ImageView ImageView { get; set; }

        public ViewHolder(View view, Action<int> listener) : base(view)
        {
            View = view;
            TextView = view.FindViewById<TextView>(Resource.Id.ContactName);
            ImageView = view.FindViewById<ImageView>(Resource.Id.ContactImage);

            view.Click += (sender, e) => listener(base.LayoutPosition);
        }

        //public override void OnClick(View v)
        //{
        //    Contact contact = GetValueAt(AdapterPosition);
        //    Log.Debug("SelectContactsActivity", contact.ToString());
        //    //    h.View.SetBackgroundColor(Color.Aqua);
        //}

        public override string ToString()
        {
            return base.ToString() + " '" + TextView.Text;
        }
    }
}