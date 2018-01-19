using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace GraphLibrary
{
    public class User
    {
        public long UserId { get; set; }
        public string DepartmentName { get; set; }
        public long InDegreeCentrality { get; set; }
        public long OutDegreeCentrality { get; set; }
        public double ClosenessCentrality { get; set; }
        public double BetweennessCentrality { get; set; }
        public long EigenvectorCentrality { get; set; }
        public long ComponentNo { get; set; }
        public long ComponentSize { get; set; }
    }
}
