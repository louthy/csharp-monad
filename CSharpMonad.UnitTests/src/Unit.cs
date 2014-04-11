using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monad.UnitTests
{
    public class Unit
    {
        private static Unit singleton = new Unit();

        private Unit()
        {
        }

        public static Unit Return()
        {
            return singleton;
        }

        public static Unit Return(Action action)
        {
            action();
            return Return();
        }
    }
}