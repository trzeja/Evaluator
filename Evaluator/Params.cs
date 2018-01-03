using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Evaluator
{
    public static class Params
    {
        public static int Bins { get; }

        public static int SMin { get; }

        public static double Y { get; }

        public static int RegionsToRemain { get; }

        public static double BinSize { get; } 

        static Params()
        {
            XDocument xdoc = XDocument.Load("Params.xml");
            Bins = int.Parse(xdoc.Descendants("Bins").First().Value);
            SMin = int.Parse(xdoc.Descendants("SMin").First().Value);
            RegionsToRemain = int.Parse(xdoc.Descendants("RegionsToRemain").First().Value);
            Y = double.Parse(xdoc.Descendants("Y").First().Value);
            BinSize = NumOfPossibleCValues / Bins;
        }

        public const int Neighbors = 8;

        public const double MaxC = 255;

        public const double NumOfPossibleCValues = MaxC + 1 + MaxC;

        public const byte MaxLBP = 255;

        public const int SignalMax = 255;
    }

}
