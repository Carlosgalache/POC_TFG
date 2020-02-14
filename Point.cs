using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POCLeapMotion
{
    public class Point
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }

        public char Name { get; set; }

        public Point(int x, int y, int z, char name)
        {
            X = x;
            Y = y;
            Z = z;
            Name = name;
        }
    }
}
