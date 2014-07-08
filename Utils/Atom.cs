using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    /// <summary>
    /// Encapsualtes a stateful reference or value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Atom<T>: IReadableAtom<T>, IWriteableAtom<T>
    {
        private T value;

        public Atom(T init)
        {
            value = init;
        }

        virtual public T Val
        {
            get
            {
                return value;
            }
        }

        public void Swap(Func<T, T> fn, Type sender)
        {
            Exchange(fn(value), sender);
        }

        virtual public void Exchange(T v, Type sender)
        {
            value = v;
        }
    }

    public interface IReadableAtom<T>
    {
        T Val { get; }
    }

    public interface IWriteableAtom<T>
    {
        void Swap(Func<T, T> fn, Type sender);

        void Exchange(T v, Type sender);
    }
}
