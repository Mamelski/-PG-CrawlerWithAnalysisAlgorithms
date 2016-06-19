using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crawler
{
    using System.Collections.Concurrent;
    using System.IO;

    public class GraphSerializer
    {

        public void Serialize(ConcurrentDictionary<Uri, Node> graph)
        {
            using (var sw = new StreamWriter("Test1.txt"))
            {
                foreach (var node in graph)
                {
                    sw.WriteLine(node.Key.ToString());
                    foreach (var subnode in node.Value.Neighbours)
                    {
                        sw.WriteLine("-" + subnode.Uri);
                    }
                }
            }
        }
    }
}
