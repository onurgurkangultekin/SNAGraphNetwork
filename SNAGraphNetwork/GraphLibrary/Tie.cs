using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphLibrary
{
    public class Tie
    {
        public long UserId { get; set; }
        public string DepartmentName { get; set; }
        public double MaxTieStrength { get; set; }
        public long MaxTieUserId { get; set; }
        public long MinTieStrength { get; set; }
        public long MinTieUserId { get; set; }
    }
}
