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

namespace Cheesesquare
{
    [Activity(Label = "SelectContactsActivity")]
    public class SelectContactsActivity : AppCompatActivity
    {
        private RecyclerView contactsRecyclerView;
        private RecyclerView.Adapter recyclerAdapter;
        private RecyclerView.LayoutManager recyclerLayoutManager;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Create your application here

            SetContentView(Resource.Layout.activity_select_contacts);
            //ContactsAdapter contactsAdapter = new ContactsAdapter(this);
            //ListView contactsListView = FindViewById<ListView>(Resource.Id.ContactsListView);
            //contactsListView.Adapter = contactsAdapter;


            contactsRecyclerView = FindViewById<RecyclerView>(Resource.Id.contacts_recycler_view);

            // use this setting to improve performance if you know that changes
            // in content do not change the layout size of the RecyclerView
            contactsRecyclerView.HasFixedSize = true;

            // set the dividers between the items (contacts)
            contactsRecyclerView.AddItemDecoration(new DividerItemDecoration(Application.Context, Resource.Drawable.line_divider));

            // use a linear layout manager
            recyclerLayoutManager = new LinearLayoutManager(this);
            contactsRecyclerView.SetLayoutManager(recyclerLayoutManager);

            // specify an adapter (see also next example)
            recyclerAdapter = new ContactsRecyclerAdapter(this);
            contactsRecyclerView.SetAdapter(recyclerAdapter);


            FastScroller fastScroller = FindViewById<FastScroller>(Resource.Id.fastscroller);
            fastScroller.SetRecyclerView(contactsRecyclerView);
        }



    }

    public class ContactsRecyclerAdapter : BaseRecyclerAdapter
    {
        Activity _activity;
        List<Contact> _contactList;

        public ContactsRecyclerAdapter(Activity activity)
        {
            _activity = activity;
            FillContacts();
        }

        void FillContacts()
        {
            var uri = ContactsContract.Contacts.ContentUri;

            string[] projection = {
                ContactsContract.Contacts.InterfaceConsts.Id,
                ContactsContract.Contacts.InterfaceConsts.DisplayName,
                ContactsContract.Contacts.InterfaceConsts.PhotoId
                };

            var cursor = Application.Context.ContentResolver.Query(uri, projection, null,
                null, null);


            _contactList = new List<Contact>();

            if (cursor.MoveToFirst())
            {
                do
                {
                    _contactList.Add(new Contact
                    {
                        Id = cursor.GetLong(
                    cursor.GetColumnIndex(projection[0])),
                        DisplayName = cursor.GetString(
                    cursor.GetColumnIndex(projection[1])),
                        PhotoId = cursor.GetString(
                    cursor.GetColumnIndex(projection[2]))
                    });
                } while (cursor.MoveToNext());
            }
        }

        private Contact GetValueAt(int position)
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
            var contact = GetValueAt(position);

            h.TextView.Text = GetValueAt(position).DisplayName;

            if (contact.PhotoId == null)
            {
                h.ImageView.SetImageResource(Resource.Drawable.ic_account_circle_black_24dp);
                h.ImageView.SetColorFilter(Color.ParseColor("#A9A9A9"));
            }
            else
            {
                var contactUri = ContentUris.WithAppendedId(
                    ContactsContract.Contacts.ContentUri, contact.Id);
                var contactPhotoUri = Android.Net.Uri.WithAppendedPath(contactUri,
                    Contacts.Photos.ContentDirectory);

                Bitmap thumbBitmap = MediaStore.Images.Media.GetBitmap(Application.Context.ContentResolver, contactPhotoUri); //.getBitmap(this.getContentResolver(), imageUri);
                var roundedThumbBitmap = RoundedBitmapDrawableFactory.Create(Application.Context.Resources, thumbBitmap);
                roundedThumbBitmap.Circular = true;

                h.ImageView.SetImageDrawable(roundedThumbBitmap);
            }

        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var view = LayoutInflater.From(parent.Context)
                .Inflate(Resource.Layout.contact_list_item, parent, false);

            return new ViewHolder(view);
        }

        public override string GetTextToShowInBubble(int pos)
        {
            return _contactList[pos].DisplayName[0].ToString();
        }

        class Contact
        {
            public long Id { get; set; }
            public string DisplayName { get; set; }
            public string PhotoId { get; set; }
        }



    }

    public class ViewHolder : RecyclerView.ViewHolder
    {
        public View View { get; set; }
        public TextView TextView { get; set; }
        public ImageView ImageView { get; set; }

        public ViewHolder(View view) : base(view)
        {
            View = view;
            TextView = view.FindViewById<TextView>(Resource.Id.ContactName);
            ImageView = view.FindViewById<ImageView>(Resource.Id.ContactImage);
        }

        public override string ToString()
        {
            return base.ToString() + " '" + TextView.Text;
        }
    }
}