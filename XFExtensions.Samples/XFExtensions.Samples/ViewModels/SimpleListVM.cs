using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Xamarin.Forms;
using System.Threading.Tasks;
using PropertyChanged;

namespace XFExtensions.Samples
{
    [ImplementPropertyChanged]
    public class SimpleListVM
    {
        // Commands
        public ICommand AddOrUpdateItemCommand { get; protected set; }
        public ICommand DeleteItemCommand { get; protected set; }
        public ICommand SelectedItemCommand { get; protected set; }

        // Entered text
        public string Entry { get; set; }

        // Currently selected item, -1 for new item
        public int Index { get; set; } = -1;

        // Error Message
        public string ErrorMessage { get; private set; } = "";

        // List
        public ObservableCollection<string> StringList { get; set; }

        // Constructor
        public SimpleListVM()
        {
            // setup the commands
            AddOrUpdateItemCommand = new Command(async () =>
            {
                if (Index > -1)
                {
                    // update
                    if (string.IsNullOrWhiteSpace(Entry))
                        DeleteItemCommand.Execute(Index);
                    StringList[Index] = Entry;
                    Index = -1; // reset to de-select
                }
                else
                {
                    // add
                    // don't add duplicate items
                    if (StringList.Contains(Entry))
                    {
                        ShowError("That's already in the list!");
                        return;
                    }
                    StringList.Add(Entry);
                }
                Entry = string.Empty;
            });

            DeleteItemCommand = new Command<int>((index) =>
            {
                StringList.RemoveAt(index);
            });

            SelectedItemCommand = new Command((entry) =>
            {
                var text = entry as string;
                Index = StringList.IndexOf(text);
                Entry = text;
            });

            // initialize the list
            StringList = new ObservableCollection<string> {"apple", "banana", "cherry", "dragon fruit" };
        }

        private void ShowError(string message)
        {
            // for now just display it, may remove it after a little time or do a toast in the future
            ErrorMessage = message; 
        }
    }
}

