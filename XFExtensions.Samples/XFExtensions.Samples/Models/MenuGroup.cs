using System.Collections.Generic;

namespace XFExtensions.Samples.Models
{
    public class SamplesMenuGroup : List<SamplesMenuItem>
    {
        public string GroupName { get; set; }

        public SamplesMenuGroup(string name)
        {
            GroupName = name;
        }
    }
}