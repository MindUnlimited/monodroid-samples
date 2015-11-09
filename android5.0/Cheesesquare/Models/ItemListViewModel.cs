using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Linq;
using Xamarin.Forms;

namespace Todo.Models
{
    public class ItemListViewModel : INotifyPropertyChanged
    {
        public class FixedObservableCollection<T> : ObservableCollection<T>
        {
            protected override void MoveItem(int oldIndex, int newIndex)
            {
                //base.MoveItem(oldIndex, newIndex);
                T oItem = base[oldIndex];
                base.InsertItem(oldIndex, base[newIndex]);
                base.RemoveAt(oldIndex + 1);

                base.InsertItem(newIndex, oItem);
                base.RemoveAt(newIndex + 1);

            }
        }

        StackLayout footer;

        public event PropertyChangedEventHandler PropertyChanged;
        private FixedObservableCollection<Item> _reports;
        public FixedObservableCollection<Item> Reports
        {
            get { return _reports; }
            set 
            {
                if (value == _reports) return;
                _reports = value;
                _reports.CollectionChanged += (o, e) =>
                {
                    Footer = FooterInfo;
                };

                OnPropertyChanged("Reports");
                Footer = FooterInfo;
            } 
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged == null)
                return;

            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public StackLayout Footer
        {
            set
            {
                if (footer != value)
                {
                    footer = value;
                    OnPropertyChanged("Footer");
                    //SetNewColor();
                }
            }
            get
            {
                return footer;
            }
        }

        public StackLayout FooterInfo
        {
            get
            {
                var footer = new StackLayout {Orientation = StackOrientation.Horizontal, Spacing = 5, HorizontalOptions = LayoutOptions.End};

                int cancelled = 0;
                int conceived = 0;
                int started = 0;
                int blocked = 0;
                int planned = 0;
                int completed = 0;

                //-1: Cancelled
                //0: Conceived
                //1: Planned
                //2: Initiated
                //3: <25% completed
                //4: <50%
                //5: <75%
                //6: On hold / Blocked
                //7: Completed


                foreach (Item it in Reports)
                {
                    switch (it.Status)
                    {
                        case -1:
                            cancelled += 1;
                            break;
                        case 0:
                            conceived += 1;
                            break;
                        case 1:
                            planned += 1;
                            break;
                        case 2:
                        case 3:
                        case 4:
                        case 5:
                            started += 1;
                            break;
                        case 6:
                            blocked += 1;
                            break;
                        case 7:
                            completed += 1;
                            break;
                        default:
                            break;
                    }
                }

                int imgSize = Device.OnPlatform(iOS: 25, Android: 15, WinPhone: 25);
                int fontSize = Device.OnPlatform(iOS: 20, Android: 15, WinPhone: 20);
                int spacing = 5;

                StackLayout stackConceived = new StackLayout { Orientation = StackOrientation.Horizontal, Padding = 0, Spacing = spacing, HorizontalOptions = LayoutOptions.CenterAndExpand };
                stackConceived.Children.Add(new Image
                {
                    HeightRequest = imgSize,
                    WidthRequest = imgSize,
                    Source = Device.OnPlatform(
                        iOS: ImageSource.FromFile("Images/TaskConceived64.png"),
                        Android: ImageSource.FromFile("TaskConceived64.png"),
                        WinPhone: ImageSource.FromFile("Assets/ItemIcons/TaskConceived64.png"))
                });
                stackConceived.Children.Add(new Label { Text = conceived.ToString(), YAlign = TextAlignment.Center, FontSize = fontSize, TextColor = App.BLUE });

                StackLayout stackStarted = new StackLayout { Orientation = StackOrientation.Horizontal, Padding = 0, Spacing = spacing, HorizontalOptions = LayoutOptions.CenterAndExpand };
                stackStarted.Children.Add(new Image { HeightRequest = imgSize, WidthRequest = imgSize, Source = Device.OnPlatform(
                        iOS: ImageSource.FromFile("Images/TaskStarted64.png"),
                        Android: ImageSource.FromFile("TaskStarted64.png"),
                        WinPhone: ImageSource.FromFile("Assets/ItemIcons/TaskStarted64.png"))});
                stackStarted.Children.Add(new Label { Text = started.ToString(), YAlign = TextAlignment.Center, FontSize = fontSize, TextColor = App.YELLOW});

                StackLayout stackOnHold = new StackLayout { Orientation = StackOrientation.Horizontal, Padding = 0, Spacing = spacing, HorizontalOptions = LayoutOptions.CenterAndExpand };
                stackOnHold.Children.Add(new Image { HeightRequest = imgSize, WidthRequest = imgSize, Source = Device.OnPlatform(
                        iOS: ImageSource.FromFile("Images/TaskOnHold64.png"),
                        Android: ImageSource.FromFile("TaskOnHold64.png"),
                        WinPhone: ImageSource.FromFile("Assets/ItemIcons/TaskOnHold64.png"))});
                stackOnHold.Children.Add(new Label { Text = blocked.ToString(), YAlign = TextAlignment.Center, FontSize = fontSize, TextColor = App.RED });

                StackLayout stackCompleted = new StackLayout { Orientation = StackOrientation.Horizontal, Padding = 0, Spacing = spacing, HorizontalOptions = LayoutOptions.CenterAndExpand };
                stackCompleted.Children.Add(new Image { HeightRequest = imgSize, WidthRequest = imgSize, 
                    Source = Device.OnPlatform(
                        iOS: ImageSource.FromFile("Images/TaskCompleted64.png"),
                        Android: ImageSource.FromFile("TaskCompleted64.png"),
                        WinPhone: ImageSource.FromFile("Assets/ItemIcons/TaskCompleted64.png")) });
                stackCompleted.Children.Add(new Label { Text = completed.ToString(), YAlign = TextAlignment.Center, FontSize = fontSize, TextColor = App.GREEN });

                footer.Children.Add(stackConceived);
                footer.Children.Add(stackStarted);
                footer.Children.Add(stackOnHold);
                footer.Children.Add(stackCompleted);

                //footer.Children.Add(new Label { Text ="conceived: " + conceived.ToString() + " started: " + started.ToString() + " blocked: " + blocked.ToString() + " planned: " + planned.ToString() + " completed: " + completed.ToString() });
                return footer;
            } 
        }

        public static void Sort<T>(FixedObservableCollection<T> collection, Func<T, int> keySelector)
        {
            List<T> sorted = collection.OrderBy(keySelector).ToList();
            for (int i = 0; i < sorted.Count(); i++)
                collection.Move(collection.IndexOf(sorted[i]), i);
        }

        public static void Sort<T>(FixedObservableCollection<T> collection, List<T> sorted)
        {
            for (int i = 0; i < sorted.Count(); i++)
                collection.Move(collection.IndexOf(sorted[i]), i);
        }

        public List<Item> getSortedList(Todo.App.DomainPages sortOn)
        {
            List<Item> sorted = new List<Item>();
            Func<Item,int> integerSelector = null;
            Func<Item, string> stringSelector = null;

            switch (sortOn)
            {
                case App.DomainPages.Important:
                    integerSelector = (it => it.Importance);
                    break;
                case App.DomainPages.Urgent:
                    integerSelector = (it => it.Urgency);
                    break;
                case App.DomainPages.Current:
                    stringSelector = (it => it.EndDate);
                    break;
                case App.DomainPages.Completed:
                    stringSelector = (it => it.Name);
                    break;
                case App.DomainPages.Inbox:
                    stringSelector = (it => it.Name);
                    break;
                default:
                    break;
            }

            if (integerSelector != null)
            {
                IEnumerable<Item> goals = _reports.Where(x => x.Type == 2);
                List<Item> goals_sorted = goals.OrderByDescending(integerSelector).ToList();

                if (goals.Count() > 0)
                {
                    foreach (Item goal in goals_sorted)
                    {
                        sorted.Add(goal);

                        IEnumerable<Item> projects = _reports.Where(x => x.Type == 3 && x.Parent == goal.ID);

                        if (projects.Count() > 0)
                        {
                            List<Item> projects_sorted = projects.OrderByDescending(integerSelector).ToList();

                            foreach (Item project in projects_sorted)
                            {
                                sorted.Add(project);

                                IEnumerable<Item> tasks = _reports.Where(x => x.Type == 4 && x.Parent == project.ID);

                                if (tasks.Count() > 0)
                                {
                                    List<Item> tasks_sorted = tasks.OrderByDescending(integerSelector).ToList();
                                    sorted.AddRange(tasks_sorted);
                                }
                            }

                        }
                    }
                }
            }
            else if (stringSelector != null)
            {
                IEnumerable<Item> goals = _reports.Where(x => x.Type == 2);
                List<Item> goals_sorted = goals.OrderByDescending(stringSelector).ToList();

                if (goals.Count() > 0)
                {
                    foreach (Item goal in goals_sorted)
                    {
                        sorted.Add(goal);

                        IEnumerable<Item> projects = _reports.Where(x => x.Type == 3 && x.Parent == goal.ID);

                        if (projects.Count() > 0)
                        {
                            List<Item> projects_sorted = projects.OrderByDescending(stringSelector).ToList();

                            foreach (Item project in projects_sorted)
                            {
                                sorted.Add(project);

                                IEnumerable<Item> tasks = _reports.Where(x => x.Type == 4 && x.Parent == project.ID);

                                if (tasks.Count() > 0)
                                {
                                    List<Item> tasks_sorted = tasks.OrderByDescending(stringSelector).ToList();
                                    sorted.AddRange(tasks_sorted);
                                }
                            }

                        }
                    }
                }
            }

            return sorted;
        }

        public void FilterAndSort(Todo.App.DomainPages domainPage)
        {
            if (domainPage == Todo.App.DomainPages.Completed || domainPage == Todo.App.DomainPages.Inbox)
                FilterOn(domainPage);
            else
                SortOn(domainPage);
        }

        public void FilterOn(Todo.App.DomainPages filterOn)
        {
            switch (filterOn)
            {
                case App.DomainPages.Completed:
                    int j = 0;
                    while (j < _reports.Count())
                    {
                        if (_reports[j].Status != 7) // status 7 means completed
                            _reports.RemoveAt(j);
                        else
                            j += 1;
                    }
                    //_reports = (ObservableCollection<Item>)_reports.Where(it => it.Status == 7);
                    break;
                case App.DomainPages.Inbox:
                    int k = 0;
                    while (k < _reports.Count())
                    {
                        if (_reports[k].Parent != null || _reports[k].Type == 1) // remove items with parents and domains
                            _reports.RemoveAt(k);
                        else
                            k += 1;
                    }
                    break;
                default:
                    break;
            }
        }

        public void SortOn(Todo.App.DomainPages sortOn)
        {
            Sort(_reports, getSortedList(sortOn));

            //switch (sortOn)
            //{
            //    case App.DomainPages.Important:
            //        List<Item> sorted = new List<Item>();

            //        IEnumerable<Item> goals = _reports.Where(x => x.Type == 2);
            //        List<Item> goals_sorted = goals.OrderByDescending(it => it.Importance).ToList();
                    
            //        if (goals.Count() > 0)
            //        {
            //            foreach (Item goal in goals_sorted)
            //            {
            //                sorted.Add(goal);

            //                IEnumerable<Item> projects = _reports.Where(x => x.Type == 3 && x.Parent == goal.ID);

            //                if (projects.Count() > 0)
            //                {
            //                    List<Item> projects_sorted = projects.OrderByDescending(it => it.Importance).ToList();

            //                    foreach (Item project in projects_sorted)
            //                    {
            //                        sorted.Add(project);

            //                        IEnumerable<Item> tasks = _reports.Where(x => x.Type == 4 && x.Parent == project.ID);

            //                        if (tasks.Count() > 0)
            //                        {
            //                            List<Item> tasks_sorted = tasks.OrderByDescending(it => it.Importance).ToList();
            //                            sorted.AddRange(tasks_sorted);
            //                        }
            //                    }

            //                }
            //            }
            //        }

            //        Sort(_reports, sorted);


            //        //List<Item> importantSorted = _reports.OrderByDescending(it => it.Importance).ToList();
            //        //for (int i = 0; i < importantSorted.Count(); i++)
            //        //    _reports.Move(_reports.IndexOf(importantSorted[i]), i);
            //        break;
            //    case App.DomainPages.Urgent:
            //        List<Item> urgentSorted = _reports.OrderByDescending(it => it.Urgency).ToList();
            //        for (int i = 0; i < urgentSorted.Count(); i++)
            //            _reports.Move(_reports.IndexOf(urgentSorted[i]), i);
            //        break;
            //    case App.DomainPages.Current:
            //        List<Item> currentSorted = _reports.OrderByDescending(it => it.EndDate).ToList();
            //        for (int i = 0; i < currentSorted.Count(); i++)
            //            _reports.Move(_reports.IndexOf(currentSorted[i]), i);
            //        break;
            //    default:
            //        break;
            //}
        }

        public ItemListViewModel()
        {
            Reports = new FixedObservableCollection<Item>();

            var items = Todo.App.Database.GetItems();

            if (items != null)
            {
                foreach (Item it in items.Result)
                {
                    Reports.Add(it);
                }
            }
            else
                Reports = null;
        }


        public ItemListViewModel(Item domain)
        {
            Reports = new FixedObservableCollection<Item>();

            var childElements = Todo.App.Database.GetChildItems(domain);
            if (childElements != null)
            {
                foreach (Item it in childElements.Result)
                {
                    Reports.Add(it);
                }
            }
            else
                Reports = null;

            //var domains = (List<Item>) Todo.App.Database.GetDomains().Result;
            //var personal = domains[0];
            //var friends = domains[1];
            //var work = domains[2];
            //var community = domains[3];

            //if (domain.Equals("personal"))
            //{
            //    var childElements = Todo.App.Database.GetChildItems(personal);
            //    if (Todo.App.Database.GetChildItems(personal) != null)
            //    {
            //        foreach (Item it in childElements.Result)
            //        {
            //            Reports.Add(it);
            //        }
            //    }
            //}
            //else if (domain.Equals("friends"))
            //{
            //    var childElements = Todo.App.Database.GetChildItems(friends);
            //    if (childElements != null)
            //    {
            //        foreach (Item it in childElements.Result)
            //        {
            //            Reports.Add(it);
            //        }
            //    }
            //}
            //else if (domain.Equals("work"))
            //{
            //    var childElements = Todo.App.Database.GetChildItems(work);
            //    if (Todo.App.Database.GetChildItems(personal) != null)
            //    {
            //        foreach (Item it in childElements.Result)
            //        {
            //            Reports.Add(it);
            //        }
            //    }
            //}
            //else if (domain.Equals("community"))
            //{
            //    var childElements = Todo.App.Database.GetChildItems(community);
            //    if (Todo.App.Database.GetChildItems(personal) != null)
            //    {
            //        foreach (Item it in childElements.Result)
            //        {
            //            Reports.Add(it);
            //        }
            //    }
            //}
            //else
            //    Reports = null;
        }
    }
}