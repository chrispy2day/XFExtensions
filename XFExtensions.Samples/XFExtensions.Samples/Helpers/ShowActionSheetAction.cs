using System;
using Xamarin.Forms;
using System.Collections.Generic;
using System.Windows.Input;
using System.Linq;

namespace XFExtensions.Samples.Helpers
{
    public class ActionSheetButtonInfo
    {
        public string ButtonText {get; set;}
        public ICommand ButtonCommand {get; set;}
    }
    public class ShowActionSheetAction : TriggerAction<VisualElement>
    {
        public string Title { get; set; }
        public ActionSheetButtonInfo CancelButton { get; set; }
        public ActionSheetButtonInfo DestructionButton { get; set; }
        public List<ActionSheetButtonInfo> AdditionalButtons { get; private set; }

        public ShowActionSheetAction ()
        {
            AdditionalButtons = new List<ActionSheetButtonInfo>();
        }

        protected override async void Invoke(VisualElement sender)
        {
            if (CancelButton == null || string.IsNullOrEmpty(CancelButton.ButtonText))
                throw new InvalidOperationException("Cancel text must be set for the action sheet.");
            
            // get the current page
            var page = ((App)Application.Current).MainPage;
            var result = await page.DisplayActionSheet(
                Title,
                CancelButton.ButtonText,
                DestructionButton?.ButtonText,
                AdditionalButtons.Select(b => b.ButtonText).ToArray());
            if (result == CancelButton.ButtonText)
                CancelButton.ButtonCommand?.Execute(null);
            else if (result == DestructionButton?.ButtonText)
                DestructionButton.ButtonCommand?.Execute(null);
            else
                AdditionalButtons.SingleOrDefault(b => b.ButtonText == result)?.ButtonCommand?.Execute(null);
        }
    }
}

