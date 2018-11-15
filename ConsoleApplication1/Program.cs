using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            string test = " FACET NORMAL 234.13E12 24.1 1.2467858  OUTER LOOP  VERTEX 1.0 2.0 3.0 VERTEX 4.0 5.0 6.0 VERTEX 7.0 8.0 9.0  ENDLOOP ENDFACET    FACET NORMAL 2.13E12 24.1 1.2467858  OUTER LOOP  VERTEX 1.0 2.0 3.0 VERTEX 4.0 5.0 6.0 VERTEX 7.0 8.0 9.0  ENDLOOP ENDFACET   ";
            Regex re = new Regex(@"^(\s*FACET\s+NORMAL\s+(\d+\.\d+(E\d+)?)\s+(\d+\.\d+(E\d+)?)\s+(\d+\.\d+(E\d+)?)\s+OUTER\s+LOOP(\s*VERTEX\s+(\d+\.\d+(E\d+)?)\s+(\d+\.\d+(E\d+)?)\s+(\d+\.\d+(E\d+)?)\s*){3}\s+ENDLOOP\s+ENDFACET\s*)+$");
            if (!re.IsMatch(test)) System.Diagnostics.Debugger.Break();
            MatchCollection facets = new Regex(@"(\s*FACET\s+NORMAL\s+(?<i>\d+\.\d+(E\d+)?)\s+(?<j>\d+\.\d+(E\d+)?)\s+(?<k>\d+\.\d+(E\d+)?)\s+OUTER\s+LOOP(\s*VERTEX\s+(\d+\.\d+(E\d+)?)\s+(\d+\.\d+(E\d+)?)\s+(\d+\.\d+(E\d+)?)\s*){3}\s+ENDLOOP\s+ENDFACET\s*)").Matches(test);
            if(facets.Count < 1) Debugger.Break();
            foreach(Match facet in facets)
            {
                MatchCollection vertexes = new Regex(@"(\s*VERTEX\s+(?<x>\d+\.\d+(E\d+)?)\s+(?<y>\d+\.\d+(E\d+)?)\s+(?<z>\d+\.\d+(E\d+)?)\s*)").Matches(facet.Value);
                foreach(Match vertex in vertexes)
                {

                }
            }
            Debugger.Break();
        }
    }
}
