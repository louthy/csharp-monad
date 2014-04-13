using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monad
{
    /// <summary>
    /// A type with only one value, itself.
    /// This is the functional world's equivalent of Void.
    /// </summary>
    public class Unit
    {
        /// <summary>
        /// Use a singleton so that ref equality works and to reduce any unnecessary
        /// thrashing of the GC from creating an object which is always the same.
        /// </summary>
        private static Unit singleton = new Unit();

        /// <summary>
        /// Private ctor
        /// </summary>
        private Unit()
        {
        }

        /// <summary>
        /// Return
        /// </summary>
        public static Unit Return()
        {
            return singleton;
        }

        /// <summary>
        /// Performs an action which instead of returning void will return Unit
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public static Unit Return(Action action)
        {
            action();
            return Return();
        }
    }
}