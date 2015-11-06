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

namespace Cheesesquare
{
    [Activity(Label = "Group")]
    class DefineGroupActivity : AppCompatActivity
    {
        private EditText GroupName;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Create your application here
            SetContentView(Resource.Layout.activity_define_group);

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
                        StartActivity(addGroupMembers);

                        Finish();
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

    }
}