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

namespace Cheesesquare
{
    public class DateDialog : DialogFragment, DatePickerDialog.IOnDateSetListener
    {
        EditText txtDate;

        public DateDialog(View view)
        {
            txtDate = (EditText)view;
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
            String date = dayOfMonth + "-" + (monthOfYear+1) + "-" + year;
            txtDate.Text = date;
            txtDate.ClearFocus();
        }
    }
}