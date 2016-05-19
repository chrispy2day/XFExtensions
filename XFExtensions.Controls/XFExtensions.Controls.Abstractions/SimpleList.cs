using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Input;
using Xamarin.Forms;

namespace XFExtensions.Controls.Abstractions
{
    public class SimpleList : StackLayout
    {
        public SimpleList()
        {
            // default values
            TextColor = Color.Black;
            ItemHeightRequest = 22;
        }

        public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create (
            nameof (ItemsSource), 
            typeof (IEnumerable), 
            typeof (SimpleList), 
            null, 
            BindingMode.OneWay,
            propertyChanged: ItemsSourceChanged);

        public IEnumerable ItemsSource 
        {
            get { return (IEnumerable) GetValue(ItemsSourceProperty); }
            set { 
                SetValue(ItemsSourceProperty, value); 
            } 
        }

        private static void ItemsSourceChanged (BindableObject bindable, object oldValue, object newValue)
        {
            var list = (SimpleList) bindable;
            list.RepopulateList();

            var colChange = list.ItemsSource as INotifyCollectionChanged;
            if (colChange != null)
                colChange.CollectionChanged += list.colChange_CollectionChanged;
        }

        public static readonly BindableProperty ItemTemplateProperty = BindableProperty.Create (
            nameof (ItemTemplate), 
            typeof (DataTemplate), 
            typeof (SimpleList), 
            null, 
            BindingMode.OneWay, 
            propertyChanged: ItemTemplateChanged);

        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        private static void ItemTemplateChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            var list = (SimpleList)bindable;
            list.RepopulateList();
        }
        
        public static readonly BindableProperty ItemSelectedCommandProperty =
            BindableProperty.Create (
                nameof (ItemSelectedCommand),
                typeof (ICommand),
                typeof (SimpleList),
                null,
                BindingMode.OneWay);

        public ICommand ItemSelectedCommand 
        {
            get { return (ICommand)GetValue (ItemSelectedCommandProperty); }
            set { SetValue (ItemSelectedCommandProperty, value); }
        }

        public string DisplayMember { get; set; }

        public Color TextColor { get; set; }

        public int ItemHeightRequest { get; set; }

        private ICommand _selectedCommand;

        private void RepopulateList()
        {
            Children.Clear();
            if (ItemsSource == null)
                return;
            
            // build our own internal command so that we can respond to selected events
            // correctly even if the command was set after the items were rendered
            if (_selectedCommand == null)
                _selectedCommand = new Command<object>(o =>
                {
                    if (ItemSelectedCommand != null && ItemSelectedCommand.CanExecute(o))
                        ItemSelectedCommand.Execute(o);
                });

            foreach (var item in ItemsSource)
            {
                var child = PopulateChild (item);
                Children.Add(child);
            }
        }

        private void colChange_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
                this.Children.RemoveAt(e.OldStartingIndex);

            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    var view = PopulateChild (item);
                    this.Children.Insert(e.NewStartingIndex, view);
                }
            }

            if (e.OldItems != null || e.NewItems != null)
            {
                this.UpdateChildrenLayout();
                this.InvalidateLayout();
            }
        }

        private View PopulateChild (object item)
        {
            View child;
            if (ItemTemplate != null)
            {
                child = ItemTemplate.CreateContent() as View;
                if (child == null)
                    return null;
                child.BindingContext = item;
            }
            else if (!string.IsNullOrEmpty(DisplayMember))
            {
                child = new Label { BindingContext = item, TextColor = TextColor, HeightRequest = ItemHeightRequest };
                child.SetBinding(Label.TextProperty, DisplayMember);
            }
            else
            {
                child = new Label { Text = item.ToString(), TextColor = TextColor, HeightRequest = ItemHeightRequest };
            }
            
            // add an internal tapped handler
            var itemTapped = new TapGestureRecognizer {Command = _selectedCommand, CommandParameter = item};
            child.GestureRecognizers.Add(itemTapped);
            return child;
        }
    }
}
