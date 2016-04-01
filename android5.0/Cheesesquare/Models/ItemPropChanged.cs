using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.MobileServices;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Cheesesquare.Models;

namespace Todo
{
    public class Item : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string caller = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(caller));
            }
        }

        public string id
        {
            get; set;
        }

        [JsonIgnore]
        private ItemLink _sharedLink;

        [JsonIgnore]
        public ItemLink SharedLink
        {
            get { return _sharedLink; }
            set
            {
                // OnPropertyChanged should not be called if the property value
                // does not change.
                if (_sharedLink == value)
                    return;
                _sharedLink = value;
                OnPropertyChanged();
            }
        }


        [JsonIgnore]
        private string _imagePath;

        [JsonIgnore]
        public string ImagePath
        {
            get { return _imagePath; }
            set
            {
                // OnPropertyChanged should not be called if the property value
                // does not change.
                if (_imagePath == value)
                    return;
                _imagePath = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        private int _imageResource;

        [JsonIgnore]
        public int ImageResource
        {
            get { return _imageResource; }
            set
            {
                // OnPropertyChanged should not be called if the property value
                // does not change.
                if (_imageResource == value)
                    return;
                _imageResource = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        private int _imageResourceBackdrop;

        [JsonIgnore]
        public int ImageResourceBackdrop
        {
            get { return _imageResourceBackdrop; }
            set
            {
                // OnPropertyChanged should not be called if the property value
                // does not change.
                if (_imageResourceBackdrop == value)
                    return;
                _imageResourceBackdrop = value;
                OnPropertyChanged();
            }
        }


        [JsonIgnore]
        private String _ownedby;

        public String OwnedBy
        {
            get { return _ownedby; }
            set
            {
                // OnPropertyChanged should not be called if the property value
                // does not change.
                if (_ownedby == value)
                    return;
                _ownedby = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        private String _parent;

        public string Parent
        {
            get { return _parent; }
            set
            {
                // OnPropertyChanged should not be called if the property value
                // does not change.
                if (_parent == value)
                    return;
                _parent = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        private int _type;

        public int Type
        {
            get { return _type; }
            set
            {
                // OnPropertyChanged should not be called if the property value
                // does not change.
                if (_type == value)
                    return;
                _type = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        private string name;

        public string Name
        {
            get { return name; }
            set
            {
                // OnPropertyChanged should not be called if the property value
                // does not change.
                if (name == value)
                    return;
                name = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        private int status;

        public int Status
        {
            get { return status; }
            set
            {
                // OnPropertyChanged should not be called if the property value
                // does not change.
                if (status == value)
                    return;
                status = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        private int importance;

        public int Importance
        {
            get { return importance; }
            set
            {
                // OnPropertyChanged should not be called if the property value
                // does not change.
                if (importance == value)
                    return;
                importance = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        private int urgency;

        public int Urgency
        {
            get { return urgency; }
            set
            {
                // OnPropertyChanged should not be called if the property value
                // does not change.
                if (urgency == value)
                    return;
                urgency = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        private int order;

        public int Order
        {
            get { return order; }
            set
            {
                // OnPropertyChanged should not be called if the property value
                // does not change.
                if (order == value)
                    return;
                order = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        private string notes;

        public string Notes
        {
            get { return notes; }
            set
            {
                // OnPropertyChanged should not be called if the property value
                // does not change.
                if (notes == value)
                    return;
                notes = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        private string createdby;

        public string CreatedBy
        {
            get { return createdby; }
            set
            {
                // OnPropertyChanged should not be called if the property value
                // does not change.
                if (createdby == value)
                    return;
                createdby = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        private string updatedby;

        public string UpdatedBy
        {
            get { return updatedby; }
            set
            {
                // OnPropertyChanged should not be called if the property value
                // does not change.
                if (updatedby == value)
                    return;
                updatedby = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        private string deletedby;

        public string DeletedBy
        {
            get { return deletedby; }
            set
            {
                // OnPropertyChanged should not be called if the property value
                // does not change.
                if (deletedby == value)
                    return;
                deletedby = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        private string assignedto;

        public string AssignedTo
        {
            get { return assignedto; }
            set
            {
                // OnPropertyChanged should not be called if the property value
                // does not change.
                if (assignedto == value)
                    return;
                assignedto = value;
                OnPropertyChanged();
            }
        }
        [JsonIgnore]
        private DateTime startdate;

        public DateTime StartDate
        {
            get { return startdate; }
            set
            {
                // OnPropertyChanged should not be called if the property value
                // does not change.
                if (startdate == value)
                    return;
                startdate = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        private DateTime enddate;

        public DateTime EndDate
        {
            get { return enddate; }
            set
            {
                // OnPropertyChanged should not be called if the property value
                // does not change.
                if (enddate == value)
                    return;
                enddate = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        private string effortestimate;

        public string EffortEstimate
        {
            get { return effortestimate; }
            set
            {
                // OnPropertyChanged should not be called if the property value
                // does not change.
                if (effortestimate == value)
                    return;
                effortestimate = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        private int recurrent;

        public int Recurrent
        {
            get { return recurrent; }
            set
            {
                // OnPropertyChanged should not be called if the property value
                // does not change.
                if (recurrent == value)
                    return;
                recurrent = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        private string rewardtype;

        public string RewardType
        {
            get { return rewardtype; }
            set
            {
                // OnPropertyChanged should not be called if the property value
                // does not change.
                if (rewardtype == value)
                    return;
                rewardtype = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        private float rewardamount;

        public float RewardAmount
        {
            get { return rewardamount; }
            set
            {
                // OnPropertyChanged should not be called if the property value
                // does not change.
                if (rewardamount == value)
                    return;
                rewardamount = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        private string punishmenttype;

        public string PunishmentType
        {
            get { return punishmenttype; }
            set
            {
                // OnPropertyChanged should not be called if the property value
                // does not change.
                if (punishmenttype == value)
                    return;
                punishmenttype = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        private float punishmentamount;

        public float PunishmentAmount
        {
            get { return punishmentamount; }
            set
            {
                // OnPropertyChanged should not be called if the property value
                // does not change.
                if (punishmentamount == value)
                    return;
                punishmentamount = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        private string dependenton;

        public string DependentOn
        {
            get { return dependenton; }
            set
            {
                // OnPropertyChanged should not be called if the property value
                // does not change.
                if (dependenton == value)
                    return;
                dependenton = value;
                OnPropertyChanged();
            }
        }

        //public event PropertyChangedEventHandler PropertyChanged;

        //void OnPropertyChanged(string propertyName = null)
        //{
        //    PropertyChangedEventHandler handler = PropertyChanged;
        //    if (handler != null)
        //        handler(this, new PropertyChangedEventArgs(propertyName));
        //}

        //[JsonIgnore]
        //public List<Todo.Item> SubItems { get; set; }

        [Version]
        public string Version { get; set; }

        //public override string ToString()
        //{
        //    return "    Text: " + Name + "\n    Status: " + Status + "\n    Owned by: " + OwnedBy;
        //}

        public override string ToString()
        {
            return Name;

        }
    }
}
