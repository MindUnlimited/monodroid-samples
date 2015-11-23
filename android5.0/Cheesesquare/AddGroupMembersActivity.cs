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
using V7Toolbar = Android.Support.V7.Widget.Toolbar;
using Android.Util;
using Newtonsoft.Json;
using Android.Graphics;
using Java.Lang;
using Android.Provider;
using Android.Support.V4.Graphics.Drawable;

namespace Cheesesquare
{



    public class AutocompleteCustomArrayAdapter : ArrayAdapter<Contact> {

        const string TAG = "AutocompleteCustomArrayAdapter.java";

        Context mContext;
        int layoutResourceId;
        public List<Contact> data;

        public AutocompleteCustomArrayAdapter(Context mContext, int layoutResourceId, List<Contact> data) : base(mContext, layoutResourceId, data)
        {
            this.layoutResourceId = layoutResourceId;
            this.mContext = mContext;
            this.data = data;
        }

        public override View GetView(int position, View convertView, ViewGroup parent) {

            try
            {

                /*
                 * The convertView argument is essentially a "ScrapView" as described is Lucas post 
                 * http://lucasr.org/2012/04/05/performance-tips-for-androids-listview/
                 * It will have a non-null value when ListView is asking you recycle the row layout. 
                 * So, when convertView is not null, you should simply update its contents instead of inflating a new row layout.
                 */
                if (convertView == null)
                {
                    // inflate the layout
                    LayoutInflater inflater = ((Activity)mContext).LayoutInflater;
                    convertView = inflater.Inflate(layoutResourceId, parent, false);
                }

                // object item based on the position

                Contact objectItem = GetItem(position);// data[position];


                // get the TextView and then set the text (item name) and tag (item ID) values
                TextView textViewItem = (TextView)convertView.FindViewById(Resource.Id.ContactName);
                textViewItem.Text = objectItem.DisplayName;

                ImageView imageViewitem = (ImageView)convertView.FindViewById(Resource.Id.ContactImage);
                var roundedThumbBitmap = RoundedBitmapDrawableFactory.Create(Application.Context.Resources, objectItem.PhotoThumb);
                roundedThumbBitmap.Circular = true;
                imageViewitem.SetImageDrawable(roundedThumbBitmap);               
            }
            catch (NullPointerException e)
            {
                e.PrintStackTrace();
            }
            catch (Java.Lang.Exception e)
            {
                e.PrintStackTrace();
            }

            return convertView;

        }
    }



        [Activity(Label = "Group")]
    class AddGroupMembersActivity : AppCompatActivity
    {
        private AutoCompleteTextView MembersTxtField;
        private ListView membersLV;
        private string GroupName;
        private List<Contact> contacts;
        private List<Contact> selectedContacts;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            GroupName = Intent.GetStringExtra("GroupName") ?? "Group Name not available";
            var contactsJson = Intent.GetStringExtra("contacts");
            contacts = JsonConvert.DeserializeObject<List<Contact>>(contactsJson);
            contacts.RemoveAt(0); // remove Group
            contacts = CircleBitmap.addPhotoThumbs(contacts); // add all the photo thumbnails to the contacts


            selectedContacts = new List<Contact>();

            this.Title = GroupName;

            // Create your application here
            SetContentView(Resource.Layout.activity_add_group_members);

            var toolbar = FindViewById<V7Toolbar>(Resource.Id.add_group_members_toolbar);
            SetSupportActionBar(toolbar);

            MembersTxtField = FindViewById<AutoCompleteTextView>(Resource.Id.group_members_txt);
            //MembersTxtField.Adapter = new AutocompleteCustomArrayAdapter(this, Resource.Layout.contact_list_item, contacts);
            ArrayAdapter<Contact> adapter = new AutocompleteCustomArrayAdapter(this, Resource.Layout.contact_list_item, contacts);
            MembersTxtField.Adapter = adapter;

            MembersTxtField.ItemClick += MembersTxtField_ItemClick;

            membersLV = FindViewById<ListView>(Resource.Id.group_members_lv);
            membersLV.Adapter = new AutocompleteCustomArrayAdapter(this, Resource.Layout.contact_list_item, selectedContacts);

            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);
        }

        public static Contact Cast(Java.Lang.Object obj)
        {
            var propertyInfo = obj.GetType().GetProperty("Instance");
            return propertyInfo == null ? null : propertyInfo.GetValue(obj, null) as Contact;
        }

        private void MembersTxtField_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            var autoCompleteTextView = sender as AutoCompleteTextView;
            var contact = Cast(autoCompleteTextView.Adapter.GetItem(e.Position));
            selectedContacts.Add(contact);
            ((AutocompleteCustomArrayAdapter)membersLV.Adapter).Add(contact);
            Log.Debug("AddGroupMembershipActivity", "added contact " + contact.ToString());
            autoCompleteTextView.Text = "";
            autoCompleteTextView.ClearListSelection();

            //var output = new Contact { DisplayName = contact.Displayname };
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    return true;
                case Resource.Id.def_group_done:

                    if (selectedContacts != null && selectedContacts.Count > 0)
                    {
                        Intent myIntent = new Intent();
                        var members = JsonConvert.SerializeObject(selectedContacts);
                        myIntent.PutExtra("members", members);
                        SetResult(Result.Ok, myIntent);
                        Finish();
                        return true;
                    }
                    else
                    {
                        Log.Debug("AddGroupMembersActivity", "No members set yet!");
                    }
                    break;
                    
            }
            return base.OnOptionsItemSelected(item);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {

            MenuInflater.Inflate(Resource.Menu.def_group_actions, menu);
            return true;
        }

    }
}