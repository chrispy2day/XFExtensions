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
        public List<MenuGroup> Menus;
        public MenuVM()
        {
            // create the menu
            Menus = new List<MenuGroup>
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
