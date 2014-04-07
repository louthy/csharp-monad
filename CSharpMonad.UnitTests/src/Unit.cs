using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monad.UnitTests
{
    public class Unit
    {
        public static Unit Return()
        {
            return new Unit();
        }

        public static Unit Return(Action action)
        {
            action();
            return Return();
        }
    }
}