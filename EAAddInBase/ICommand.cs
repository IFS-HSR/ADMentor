using EAAddInBase.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EAAddInBase.Utils;

namespace EAAddInBase
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

        public static ICommand<T, R> Create<T, R>(Func<T, R> exec, Func<T, Boolean> canExec)
        {
            return new CommandAdapter<T, R>(exec, canExec);
        }

        public static ICommand<S, R> Adapt<S, T, R>(this ICommand<T, R> cmd, Func<S, Option<T>> map)
        {
            return Create<S, R>(arg =>
            {
                return cmd.Execute(map(arg).Value);
            }, arg =>
            {
                return map(arg).IsDefined && cmd.CanExecute(map(arg).Value);
            });
        }

        public static ICommand<Option<ModelEntity>, object> ToMenuHandler<T, R>(this ICommand<T, R> cmd)
            where T : ModelEntity
            where R : class
        {
            return cmd.Adapt((Option<ModelEntity> ci) => from e in ci
                                                         from t in e.TryCast<T>()
                                                         select t);
        }

        public static ICommand<ModelEntity, EntityModified> ToEntityCreatedHandler<T>(this ICommand<T, EntityModified> cmd)
            where T : ModelEntity
        {
            return cmd.Adapt((ModelEntity entity) => entity.TryCast<T>());
        }

        public static ICommand<ModelEntity, object> ToEntityModifiedHandler<T, R>(this ICommand<T, R> cmd)
            where T : ModelEntity
            where R : class
        {
            return cmd.Adapt((ModelEntity entity) => entity.TryCast<T>());
        }

        public static ICommand<ModelEntity, DeleteEntity> ToOnDeleteEntityHandler<T>(this ICommand<T, DeleteEntity> cmd)
            where T : ModelEntity
        {
            return cmd.Adapt((ModelEntity entity) => entity.TryCast<T>());
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
