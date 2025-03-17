using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperSmashTrees.Core.Models
{
    public class Velocity
    {
        public double X { get; set; }
        public double Y { get; set; }

        public Velocity(double x, double y)
        {
            X = x;
            Y = y;
        }
    }
}
