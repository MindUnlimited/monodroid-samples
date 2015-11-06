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
    class AddGroupMembersActivity : AppCompatActivity
    {
        private EditText MembersTxtField;
        private string GroupName;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            GroupName = Intent.GetStringExtra("GroupName") ?? "Group Name not available";
            this.Title = GroupName;

            // Create your application here
            SetContentView(Resource.Layout.activity_add_group_members);

            var toolbar = FindViewById<V7Toolbar>(Resource.Id.add_group_members_toolbar);
            SetSupportActionBar(toolbar);

            MembersTxtField = FindViewById<EditText>(Resource.Id.group_members_txt);

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
                    //if (GroupName.Text != null)
                    //{
                    //    var activity2 = new Intent(this, typeof(Activity2));
                    //    activity2.PutExtra("GroupName", GroupName.Text);
                    //    StartActivity(activity2);

                    //    Finish();
                    //    return true;
                    //}
                    //else
                    //{
                    //    Log.Debug("DefineGroupActivity", "No Group name set yet!");
                    //}
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