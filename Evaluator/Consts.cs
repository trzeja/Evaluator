using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evaluator
{
    public static class Consts
    {
        public const int Bins = 16; // to wg artykulu czyli 4 8 16 

        public const int SMin = 16; //

        public const double Y = 0.04; //

        public const int RegionsToRemain = 7; // //lake 8

        public const int Neighbors = 8; 

        public const double MaxC = 255;

        public const double NumOfPossibleCValues = MaxC + 1 + MaxC; 

        public const double BinSize = NumOfPossibleCValues / Bins;

        public const byte MaxLBP = 255;

        public const int SignalMax = 255;
    
    }

}
