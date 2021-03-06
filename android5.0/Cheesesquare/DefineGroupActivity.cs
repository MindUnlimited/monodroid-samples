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

namespace MindSet
{
    [Activity(Label = "Group")]
    class DefineGroupActivity : AppCompatActivity
    {
        private EditText GroupName;
        private const int SHARE_CONTACT = 101;
        private string contacts;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Create your application here
            SetContentView(Resource.Layout.activity_define_group);

            contacts = Intent.GetStringExtra("contacts");

            var toolbar = FindViewById<V7Toolbar>(Resource.Id.def_group_toolbar);
            SetSupportActionBar(toolbar);

            GroupName = FindViewById<EditText>(Resource.Id.def_group_name);

            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);
        }


        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    return true;
                case Resource.Id.def_group_done:
                    if (GroupName.Text != null && GroupName.Text != "") // There has to be a Group name
                    {
                        var addGroupMembers = new Intent(this, typeof(AddGroupMembersActivity));
                        addGroupMembers.PutExtra("GroupName", GroupName.Text);
                        addGroupMembers.PutExtra("contacts", contacts);
                        StartActivityForResult(addGroupMembers, SHARE_CONTACT);
                        return true;
                    }
                    else
                    {
                        Log.Debug("DefineGroupActivity", "No Group name set yet!");
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

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent intent)
        {
            base.OnActivityResult(requestCode, resultCode, intent);

            if (resultCode == Result.Ok)
            {
                switch (requestCode)
                {
                    case SHARE_CONTACT:
                        var members = intent.GetStringExtra("members");
                        Intent myIntent = new Intent();
                        myIntent.PutExtra("members", members);
                        myIntent.PutExtra("groupname", GroupName.Text);
                        SetResult(Result.Ok, myIntent);
                        Log.Debug(this.LocalClassName, "Define group");
                        Finish();

                        

                        break;
                    default:
                        break;
                }
            }
        }

    }
}