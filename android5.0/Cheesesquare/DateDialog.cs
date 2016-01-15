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
using Java.Util;
using Android.Text.Format;
using Android.Net;

namespace Cheesesquare
{
    public class DateDialog : DialogFragment, DatePickerDialog.IOnDateSetListener
    {
        EditText txtDate;
        Todo.TreeNode<Todo.Item> Item;

        public DateDialog(View view, Todo.TreeNode<Todo.Item> item)
        {
            txtDate = (EditText)view;
            Item = item;
            //txtDate.ClearFocus();
        }

        public override Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            Calendar c = Calendar.GetInstance(Locale.Default);
            var year = c.Get(CalendarField.Year);
            var month = c.Get(CalendarField.Month);
            var day = c.Get(CalendarField.DayOfMonth);
            return new DatePickerDialog(Activity, this, year, month, day);

        }

        public void OnDateSet(DatePicker view, int year, int monthOfYear, int dayOfMonth)
        {
            Calendar calendar = Calendar.Instance;
            calendar.Set(year, monthOfYear, dayOfMonth, 0, 0, 0);

            var dateInms = calendar.TimeInMillis; // time in milliseconds
            DateTime date = new DateTime(year, monthOfYear+1, dayOfMonth);
            Item.Value.EndDate = date;

            if (Item.Value.EndDate != null)
            {
                try
                {
                    if (dateInms >= 0)
                        txtDate.Text = DateUtils.GetRelativeTimeSpanString(Application.Context, dateInms);
                }
                catch (ParseException e)
                {
                    e.PrintStackTrace();
                }
            }
            else
            {
                txtDate.Text = "No due date";
            }

            txtDate.ClearFocus();
        }
    }
}