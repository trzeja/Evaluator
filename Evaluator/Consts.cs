﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evaluator
{
    public static class Consts
    {
        public const int Bins = 16;

        public const int Neighbors = 8;
        //public const double MaxContrast = 255;

        public const double MaxC = 255;

        public const double NumOfPossibleCValues = MaxC + 1 + MaxC; //255 + 1 + 255

        public const double BinSize = NumOfPossibleCValues / Bins;


        public const byte MaxLBP = 255;



        public const int minimumBlockSize = 16;

    }

}
