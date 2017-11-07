using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evaluator
{
    public class SubRegion
    {
        public SubRegion()
        {
            Blocks = new List<Rectangle>();
            //Neighbors = new List<SubRegion>();
        }

        //public List<SubRegion> Neighbors { get; set; } //TODO KISS

        public List<Rectangle> Blocks { get; set; }
        

        public int ID { get; set; }
    }
}
