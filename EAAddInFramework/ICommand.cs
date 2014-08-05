﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EAAddInFramework
{
    public interface ICommand<in T, out R>
    {
        R Execute(T arg);

        bool CanExecute(T arg);
    }

    public static class Command
    {
        /// <summary>
        /// Creates a command that is always executable.
        /// </summary>
        public static ICommand<T, R> Create<T, R>(Func<T, R> fn)
        {
            return new CommandAdapter<T, R>(fn, arg => true);
        }
    }

    class CommandAdapter<T, R> : ICommand<T, R>
    {
        public CommandAdapter(Func<T, R> exec, Func<T, bool> canExec)
        {
            Exec = exec;
            CanExec = canExec;
        }

        public Func<T, R> Exec { get; private set; }

        public Func<T, bool> CanExec { get; set; }

        public R Execute(T arg)
        {
            return Exec(arg);
        }

        public bool CanExecute(T arg)
        {
            return CanExec(arg);
        }
    }
}
