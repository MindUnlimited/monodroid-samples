using System.Collections.Generic;
using System.Text;
using Android.App;
using Android.Content;
using Android.Util;
using Gcm.Client;
using WindowsAzure.Messaging;

//VERY VERY VERY IMPORTANT NOTE!!!!
// Your package name MUST NOT start with an uppercase letter.
// Android does not allow permissions to start with an upper case letter
// If it does you will get a very cryptic error in logcat and it will not be obvious why you are crying!
// So please, for the love of all that is kind on this earth, use a LOWERCASE first letter in your Package Name!!!!
using System.Diagnostics;
using System;
using Android.Support.V7.App;
using System.Linq;

[assembly: Permission(Name = "@PACKAGE_NAME@.permission.C2D_MESSAGE")]
[assembly: UsesPermission(Name = "@PACKAGE_NAME@.permission.C2D_MESSAGE")]
[assembly: UsesPermission(Name = "com.google.android.c2dm.permission.RECEIVE")]

//GET_ACCOUNTS is needed only for Android versions 4.0.3 and below
[assembly: UsesPermission(Name = "android.permission.GET_ACCOUNTS")]
[assembly: UsesPermission(Name = "android.permission.INTERNET")]
[assembly: UsesPermission(Name = "android.permission.WAKE_LOCK")]

namespace MindSet
{
    //You must subclass this!
    [BroadcastReceiver(Permission = Gcm.Client.Constants.PERMISSION_GCM_INTENTS)]
    [IntentFilter(new string[] { Gcm.Client.Constants.INTENT_FROM_GCM_MESSAGE },
        Categories = new string[] { "@PACKAGE_NAME@" })]
    [IntentFilter(new string[] { Gcm.Client.Constants.INTENT_FROM_GCM_REGISTRATION_CALLBACK },
        Categories = new string[] { "@PACKAGE_NAME@" })]
    [IntentFilter(new string[] { Gcm.Client.Constants.INTENT_FROM_GCM_LIBRARY_RETRY },
        Categories = new string[] { "@PACKAGE_NAME@" })]
    public class MyBroadcastReceiver : GcmBroadcastReceiverBase<PushHandlerService>
    {
        public static string[] SENDER_IDS = new string[] { Constants.SenderID };

        public const string TAG = "MyBroadcastReceiver-GCM";
    }

    [Service] // Must use the service tag
    public class PushHandlerService : GcmServiceBase
    {
        public static string RegistrationID { get; private set; }
        private NotificationHub Hub { get; set; }
        public const int ITEMDETAIL = 103;
        public const int NOTIFY = 110;

        public PushHandlerService() : base(Constants.SenderID)
        {
            Log.Info(MyBroadcastReceiver.TAG, "PushHandlerService() constructor");
        }

        protected override async void OnRegistered(Context context, string registrationId)
        {
            Log.Verbose(MyBroadcastReceiver.TAG, "GCM Registered: " + registrationId);
            RegistrationID = registrationId;

            //createNotification("PushHandlerService-GCM Registered...", "The device has been Registered!");

            var device = new Models.Device { MachineId = RegistrationID, OS = 0 };
            await PublicFields.Database.SaveDevice(device);

            Hub = new NotificationHub(Constants.NotificationHubName, Constants.ListenConnectionString,
                                        context);
            try
            {
                Hub.UnregisterAll(registrationId);
            }
            catch (Exception ex)
            {
                Log.Error(MyBroadcastReceiver.TAG, ex.Message);
            }

            //var tags = new List<string>() { "falcons" }; // create tags if you want
            var tags = new List<string>() { };

            try
            {
                var hubRegistration = Hub.Register(registrationId, tags.ToArray());
            }
            catch (Exception ex)
            {
                Log.Error(MyBroadcastReceiver.TAG, ex.Message);
            }
        }

        protected override void OnMessage(Context context, Intent intent)
        {
            Log.Info(MyBroadcastReceiver.TAG, "GCM Message Received!");

            var msg = new StringBuilder();
            Dictionary<string, string> descDictionary = new Dictionary<string, string>();

            if (intent != null && intent.Extras != null)
            {
                foreach (var key in intent.Extras.KeySet())
                {
                    msg.AppendLine(key + "=" + intent.Extras.Get(key).ToString());
                    descDictionary[key] = intent.Extras.Get(key).ToString();
                }
            }

            //Log.Info(MyBroadcastReceiver.TAG, msg.ToString());

            string messageText = intent.Extras.GetString("message");
            if (!string.IsNullOrEmpty(messageText))
            {
                createNotification("New hub message!", messageText, descDictionary);
            }
            else
            {
                createNotification("Unknown message details", msg.ToString(), descDictionary);
            }
        }



        public async void createNotification(string title, string desc, Dictionary<string,string> descDictionary)
        {
            //item_ownedby
            var name = descDictionary[DetailActivity.EXTRA_NAME];
            var itemID = descDictionary[DetailActivity.ITEM_ID];

            //Create notification
            var notificationManager = GetSystemService(Context.NotificationService) as NotificationManager;

            ////update db and retrieve item link
            //await PublicFields.Database.SyncAsync();
            //PublicFields.MakeTree();

            var itemLinks = await PublicFields.Database.GetItemLinks();
            var myItemLinks = from itl in itemLinks where itl.OwnedBy == PublicFields.Database.defGroup.id select itl;

            //shared from other user
            Intent intent = null;

            //var itemLink = myItemLinks.FirstOrDefault(x => x.ItemID == itemID);
            //if (itemLink != null) // found the link
            //{
            //    Log.Debug("push", string.Format("found itemlink {0}", itemLink.id));

            //    if (itemLink.Parent != null) // has a place in the tree already
            //    {
            //        //Create an intent to show UI
            //        intent = new Intent(this, typeof(CheeseDetailActivity));
            //        intent.PutExtra(CheeseDetailActivity.EXTRA_NAME, name);
            //        intent.PutExtra(CheeseDetailActivity.ITEM_ID, itemID);
            //    }
            //    else // not assigned yet
            //    {
            //        //Create an intent to show the shared items
            //        Log.Debug("push", "found itemlink not regual item");
            //        //intent = new Intent(this, typeof(SharedItemsActivity));
            //        //intent.AddFlags(ActivityFlags.ClearTop);
            //    }
            //}
            //else
            //{
            //    Log.Debug("push", "found no itemlink");
            //    //Create an intent to show UI
            //    //intent = new Intent(this, typeof(CheeseDetailActivity));
            //    //intent.PutExtra(CheeseDetailActivity.EXTRA_NAME, name);
            //    //intent.PutExtra(CheeseDetailActivity.ITEM_ID, itemID);
            //}

            //Create an intent to show UI
            intent = new Intent(this, typeof(DetailActivity));
            intent.PutExtra(DetailActivity.EXTRA_NAME, name);
            intent.PutExtra(DetailActivity.ITEM_ID, itemID);

            //Create the notification
            var notification = new Notification(Android.Resource.Drawable.SymActionEmail, title);

            //Auto-cancel will remove the notification once the user touches it
            notification.Flags = NotificationFlags.AutoCancel;

            var notificationTitle = "A new item has been added";
            //Set the notification info
            //we use the pending intent, passing our ui intent over, which will get called
            //when the notification is tapped.
            //notification.SetLatestEventInfo(Application.Context, notificationTitle, name, PendingIntent.GetActivity(this, ITEMDETAIL, intent, 0));

            

            NotificationCompat.Builder builder = new NotificationCompat.Builder(this);

            if(intent != null)
            {
                var pendingIntent = PendingIntent.GetActivity(this, ITEMDETAIL, intent, PendingIntentFlags.UpdateCurrent); // update current is neccessary for passing the extras allong
                builder.SetContentIntent(pendingIntent);
            }
            

            notification = builder.SetSmallIcon(Resource.Drawable.LogoMindSet).SetTicker("new item added")
                                  .SetAutoCancel(true).SetContentTitle(notificationTitle)
                                  .SetContentText(name).Build();

            //Show the notification
            //dialogNotify(title, desc);
            notificationManager.Notify(NOTIFY, notification);
        }


        public void createNotification(string title, string desc)
        {
            //Create notification
            var notificationManager = GetSystemService(Context.NotificationService) as NotificationManager;

            //Create an intent to show UI
            var uiIntent = new Intent(this, typeof(MainActivity));

            //Create the notification
            var notification = new Notification(Android.Resource.Drawable.SymActionEmail, title);

            //Auto-cancel will remove the notification once the user touches it
            notification.Flags = NotificationFlags.AutoCancel;

            //Set the notification info
            //we use the pending intent, passing our ui intent over, which will get called
            //when the notification is tapped.
            notification.SetLatestEventInfo(this, title, desc, PendingIntent.GetActivity(this, 0, uiIntent, 0));

            //Show the notification
            notificationManager.Notify(1, notification);
            dialogNotify(title, desc);
        }

        protected void dialogNotify(String title, String message)
        {

            MainActivity.instance.RunOnUiThread(() =>
            {
                Android.App.AlertDialog.Builder dlg = new Android.App.AlertDialog.Builder(MainActivity.instance);
                Android.App.AlertDialog alert = dlg.Create();
                alert.SetTitle(title);
                alert.SetButton("Ok", delegate
                {
                    alert.Dismiss();
                });
                alert.SetMessage(message);
                alert.Show();
            });
        }

        protected override void OnUnRegistered(Context context, string registrationId)
        {
            Log.Verbose(MyBroadcastReceiver.TAG, "GCM Unregistered: " + registrationId);

            createNotification("GCM Unregistered...", "The device has been unregistered!");
        }

        protected override bool OnRecoverableError(Context context, string errorId)
        {
            Log.Warn(MyBroadcastReceiver.TAG, "Recoverable Error: " + errorId);

            return base.OnRecoverableError(context, errorId);
        }

        protected override void OnError(Context context, string errorId)
        {
            Log.Error(MyBroadcastReceiver.TAG, "GCM Error: " + errorId);
        }
    }
}

