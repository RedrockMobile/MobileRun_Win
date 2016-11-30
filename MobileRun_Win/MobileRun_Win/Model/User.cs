using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobileRun_Win.Model
{
    public class User
    {
        public int statu { get; set; }
        public string info { get; set; }
        public string version { get; set; }
        public Data data { get; set; }
    }

    public class Data
    {
        public string name { get; set; }
        public string gender { get; set; }
        public string token { get; set; }
        public string classNum { get; set; }
        public string major { get; set; }
        public string college { get; set; }
        public string grade { get; set; }
    }
}
