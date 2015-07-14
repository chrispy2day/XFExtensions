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
        private List<MenuGroup> _menus;
        public MenuVM()
        {
            // create the menu
            _menus = new List<MenuGroup>
            {
                new MenuGroup
                {
                    GroupName = "SimpleList",
                    MenuItems = new List<MenuItem>
                    {
                        new MenuItem
                        {
                            Name = "Auto Binding"
                        }
                    }
                }
            };
        }
    }
}
