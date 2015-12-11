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
            //String date = dayOfMonth + "-" + (monthOfYear+1) + "-" + year;
            //txtDate.Text = date;
            
            Calendar calendar = Calendar.Instance;
            calendar.Set(year, monthOfYear, dayOfMonth, 0, 0, 0);
            Item.Value.EndDate = calendar.TimeInMillis.ToString(); // time in milliseconds

            if (Item.Value.EndDate != null && Item.Value.EndDate != "")
            {
                //String givenDateString = "Tue Apr 23 16:08:28 GMT+05:30 2013";
                //SimpleDateFormat sdf = new SimpleDateFormat("dd-MM-yyyy");//new SimpleDateFormat("EEE MMM dd HH:mm:ss z yyyy");
                try
                {
                    //Date mDate = sdf.Parse(item.EndDate);
                    long timeInMilliseconds;
                    long.TryParse(Item.Value.EndDate, out timeInMilliseconds);
                    if (timeInMilliseconds > 0)
                        txtDate.Text = DateUtils.GetRelativeTimeSpanString(Application.Context, timeInMilliseconds);
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

            //(datePicker.getYear(), datePicker.getMonth(), datePicker.getDayOfMonth(),
            //             timePicker.getCurrentHour(), timePicker.getCurrentMinute(), 0);
            //long startTime = calendar.getTimeInMillis();
        }
    }
}