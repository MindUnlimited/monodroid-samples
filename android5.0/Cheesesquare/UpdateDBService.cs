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

namespace Cheesesquare
{
    [Service]
    [IntentFilter(new String[] { "com.sample.cheesesqaure.UpdateDBService" })]
    public class UpdateDBService : IntentService
    {
        public const string DBUpdatedAction = "DBUpdated";

        protected async override void OnHandleIntent(Intent intent)
        {
                try
                {
                    // update db
                    await PublicFields.Database.SyncAsync();

                    var dbIntent = new Intent(DBUpdatedAction);

                    SendOrderedBroadcast(dbIntent, null);
                }
                catch (Exception e)
                {
                    
                }
        }
    }
}