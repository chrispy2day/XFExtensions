using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XFExtensions.Samples.Models;

namespace XFExtensions.Samples.ViewModels
{
    public class MenuVM
    {
        public List<MenuItem> Menu { get; private set; }
        
        public MenuVM()
        {
            // create the menu
            Menu = new List<MenuItem>
            {
                new MenuItem { Category = "SimpleList", Name = "Simple Coded List" },
                new MenuItem { Category = "SimpleList", Name = "Simple XAML List" },
                new MenuItem { Category = "SimpleList", Name = "Templated List" },
            };
        }
    }
}
