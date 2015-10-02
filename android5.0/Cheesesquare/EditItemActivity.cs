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

namespace Cheesesquare
{
    [Activity(Label = "EditItemActivity")]
    public class EditItemActivity : AppCompatActivity
    {
        private Android.Support.V7.Widget.Toolbar toolbar;
        public const string EXTRA_NAME = "cheese_name";

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            var cheeseName = Intent.GetStringExtra(EXTRA_NAME);

            // Create your application here
            SetContentView(Resource.Layout.activity_edit_item);

            toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.appbar_edit);
            toolbar.Title = cheeseName;
            SetSupportActionBar(toolbar);
        }

        protected override void OnStart()
        {
            base.OnStart();
            EditText txtDate = (EditText)FindViewById(Resource.Id.item_date);
            txtDate.FocusChange += TxtDate_FocusChange;
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