using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    class TableData
    {
        public List<string> columns = new List<string>();
        // a list of lists which has keys and values for the keys
        public List<List<string>> data = new List<List<string>>();
    }
}