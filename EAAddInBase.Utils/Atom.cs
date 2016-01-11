using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EAAddInBase.Utils
{
    /// <summary>
    /// Encapsualtes a stateful reference or value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Atom<T> : IReadableAtom<T>, IWriteableAtom<T>
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

    public interface IObservableAtom<T>
    {
        void AddListener(Action<T> l);

        void RemoveListener(Action<T> l);
    }

    public class ObservableAtom<T> : Atom<T>, IObservableAtom<T>
    {
        private readonly ISet<Action<T>> listeners = new HashSet<Action<T>>();

        public ObservableAtom(T t) : base(t) { }

        public override void Exchange(T v, Type sender)
        {
            base.Exchange(v, sender);
            listeners.ForEach(l =>
            {
                l(v);
            });
        }

        public void AddListener(Action<T> l)
        {
            listeners.Add(l);
        }

        public void RemoveListener(Action<T> l)
        {
            listeners.Remove(l);
        }
    }
}
