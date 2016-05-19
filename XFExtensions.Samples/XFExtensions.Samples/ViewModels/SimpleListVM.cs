using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Xamarin.Forms;
using System.Threading.Tasks;
using System.ComponentModel;

namespace XFExtensions.Samples
{
    public class SimpleListVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        
        // Commands
        public ICommand AddOrUpdateItemCommand { get; protected set; }
        public ICommand DeleteItemCommand { get; protected set; }
        public ICommand SelectedItemCommand { get; protected set; }

        private string _enteredText = "a test";
        public string EnteredText 
        {
            get { return _enteredText; }
            set
            {
                _enteredText = value;
                
                if (PropertyChanged != null)
                {
                    PropertyChanged(this,
                        new PropertyChangedEventArgs("EnteredText"));
                }
            }
        }

        // Currently selected item, -1 for new item
        private int _index = -1;
        public int Index
        {
            get { return _index; }
            set
            {
                _index = value;
                
                if (PropertyChanged != null)
                {
                    PropertyChanged(this,
                        new PropertyChangedEventArgs("Index"));
                }
            }
        }

        // Error Message
        private string _errorMessage = "";
        public string ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                _errorMessage = value;
                
                if (PropertyChanged != null)
                {
                    PropertyChanged(this,
                        new PropertyChangedEventArgs("ErrorMessage"));
                }
            }
        }

        // List
        public ObservableCollection<string> StringList { get; set; }

        // Constructor
        public SimpleListVM()
        {
            // setup the commands
            AddOrUpdateItemCommand = new Command(() =>
            {
                if (Index > -1)
                {
                    // update
                    if (string.IsNullOrWhiteSpace(EnteredText))
                        DeleteItemCommand.Execute(Index);
                    StringList[Index] = EnteredText;
                    Index = -1; // reset to de-select
                }
                else
                {
                    // add
                    // don't add duplicate items
                    if (StringList.Contains(EnteredText))
                    {
                        ShowError("That's already in the list!");
                        return;
                    }
                    StringList.Add(EnteredText);
                }
                EnteredText = string.Empty;
            });

            DeleteItemCommand = new Command<int>((index) =>
            {
                StringList.RemoveAt(index);
            });

            SelectedItemCommand = new Command((entry) =>
            {
                var text = entry as string;
                Index = StringList.IndexOf(text);
                EnteredText = text;
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

