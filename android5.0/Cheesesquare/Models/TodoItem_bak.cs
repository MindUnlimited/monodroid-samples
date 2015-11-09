using System;
using SQLite;

namespace Todo2
{
	public class TodoItem2
	{
		public TodoItem2 ()
		{
		}

		[PrimaryKey, AutoIncrement]
		public int ID { get; set; }
		public string Name { get; set; }
		public string Notes { get; set; }
		public bool Done { get; set; }
	}
}

