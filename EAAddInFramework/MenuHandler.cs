using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace EAAddInFramework
{
    public interface IMenuItem
    {
        string Name { get; }

        IList<IMenuItem> Children { get; }

        bool IsVisible(Option<ContextItem> contextItem);

        bool IsEnabled(Option<ContextItem> contextItem);

        void OnClick(Option<ContextItem> contextItem);
    }

    public class Menu : IMenuItem
    {
        public Menu(String name, params IMenuItem[] items)
        {
            Name = name;
            Children = items.ToList();
        }

        public String Name { get; private set; }

        public IList<IMenuItem> Children { get; private set; }

        public virtual bool IsVisible(Option<ContextItem> contextItem)
        {
            return Children.Any(child => child.IsVisible(contextItem));
        }

        public virtual bool IsEnabled(Option<ContextItem> contextItem)
        {
            return Children.Any(child => child.IsEnabled(contextItem));
        }

        public void OnClick(Option<ContextItem> contextItem) { }
    }

    public class MenuItem : IMenuItem
    {
        public MenuItem(String name, ICommand<Option<ContextItem>, Object> cmd)
        {
            Name = name;
            Command = cmd;
        }

        public string Name { get; private set; }

        public ICommand<Option<ContextItem>, Object> Command { get; private set; }

        public IList<IMenuItem> Children
        {
            get { return new List<IMenuItem>() { }; }
        }

        public virtual bool IsVisible(Option<ContextItem> contextItem)
        {
            return true;
        }

        public virtual bool IsEnabled(Option<ContextItem> contextItem)
        {
            return Command.CanExecute(contextItem);
        }

        public void OnClick(Option<ContextItem> contextItem)
        {
            Command.Execute(contextItem);
        }
    }

    class MenuHandler
    {
        private readonly IReadableAtom<Option<ContextItem>> contextItem;

        public MenuHandler(IReadableAtom<Option<ContextItem>> contextItem)
        {
            this.contextItem = contextItem;
        }

        private Menu root = new Menu("");

        public void Register(IMenuItem menu)
        {
            var newItems = root.Children.Concat(new IMenuItem[] { menu });
            root = new Menu("", newItems.ToArray());
        }

        public String[] GetMenuItems(String parentMenu)
        {
            var itemNames = (from item in GetChildren(parentMenu, root)
                             where item.IsVisible(contextItem.Val)
                             select GetEAMenuName(item)).ToArray();

            if (itemNames.Count() == 0)
            {
                return null;
            }
            else
            {
                return itemNames;
            }
        }

        public void HandleClick(String parentName, String itemName)
        {
            GetItem(parentName, itemName, root)
                .Do(item => item.OnClick(contextItem.Val));
        }

        public bool IsItemEnabled(String parentName, String itemName)
        {
            return GetItem(parentName, itemName, root)
                .Match(
                    item => item.IsEnabled(contextItem.Val),
                    () => true
                );
        }

        private static Option<IMenuItem> GetItem(String parentName, String itemName, IMenuItem root)
        {
            return (from item in GetChildren(parentName, root)
                    where GetEAMenuName(item) == itemName
                    select item).FirstOption();
        }

        private static IEnumerable<IMenuItem> GetChildren(String parentName, IMenuItem root)
        {
            if (parentName == GetEAMenuName(root))
            {
                return from child in root.Children
                       select child;
            }
            else
            {
                return from child in root.Children
                       from grandChild in GetChildren(parentName, child)
                       select grandChild;
            }
        }

        private static String GetEAMenuName(IMenuItem item)
        {
            if (item.Children.Count > 0 && item.Name != "")
            {
                return String.Format("-{0}", item.Name);
            }
            else
            {
                return item.Name;
            }
        }
    }
}
