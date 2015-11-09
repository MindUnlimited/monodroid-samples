using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;

namespace Todo.Models
{
    public class ItemList : ObservableCollection<Item>
    {
    }

    public class ViewModel
    {
        ItemList list = new ItemList();
        public ItemList ViewModelList
        {
            get { return list; }
        }
    }
}