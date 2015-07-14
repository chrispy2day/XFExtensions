using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using PropertyChanged;
using Xamarin.Forms;
using XFExtensions.Samples.Models;

namespace XFExtensions.Samples.ViewModels
{
    [ImplementPropertyChanged]
    public class DynamicListVM
    {
        private Action<string, string> _popup;

        public string EntryText { get; set; }
        public ICommand AddItemCommand { get; protected set; }
        public ICommand UpdateItemCommand { get; protected set; }
        public ICommand SelectedItemCommand { get; protected set; }
        public ICommand DeleteItemCommand { get; protected set; }
        public ObservableCollection<DynamicListItem> Items { get; set; }
        public ObservableCollection<string> SimpleItems { get; set; }

        public DynamicListVM(Action<string, string> popup)
        {
            _popup = popup;

            Items = new ObservableCollection<DynamicListItem>
            {
                new DynamicListItem() {Id = 1, Name = "Initial"},
                new DynamicListItem() {Id = 2, Name = "Added"}
            };
            SimpleItems = new ObservableCollection<string> { "Initial", "Added" };

            AddItemCommand = new Command(() =>
            {
                var max = 0;
                if (Items.Count > 0)
                    max = Items.Max(i => i.Id);
                Items.Add(new DynamicListItem { Id = ++max, Name = EntryText });
                SimpleItems.Add(EntryText);
            });
            UpdateItemCommand = new Command(() =>
            {
                Items[0].Name = "Update: " + Items[0].Name;
                SimpleItems[0] = "Update: " + SimpleItems[0];
            });
            SelectedItemCommand = new Command<DynamicListItem>((item) => _popup("Item Selected", string.Format("You selected {0}", item.Name)));
            DeleteItemCommand = new Command<DynamicListItem>((item) => Items.Remove(item));
        }
    }
}
