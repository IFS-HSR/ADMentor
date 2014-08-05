﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    /// <summary>
    /// A type with only one possible value.
    /// 
    /// This can be useful when working with instances of generic classes when a type argument is not used.
    /// E.g. an Action<Int> can be seen as Func<Int, Unit>.
    /// </summary>
    public class Unit
    {
        private Unit() { }

        private static Unit instance = new Unit();

        public static Unit Instance { get { return instance; } }
    }
}
