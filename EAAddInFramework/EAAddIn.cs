using EAAddInFramework.MDGBuilder;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using Utils;

namespace EAAddInFramework
{
    public abstract class EAAddIn
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly Atom<EA.Repository> eaRepository = new LoggedAtom<EA.Repository>("ea.repository", null);

        private readonly Atom<Option<ContextItem>> contextItem = new LoggedAtom<Option<ContextItem>>("ea.contextItem", Options.None<ContextItem>());

        private readonly CustomDetailViewHandler customDetailViewHandler;

        private readonly MenuHandler menuHandler;

        private readonly Atom<Option<MDGTechnology>> technology = new LoggedAtom<Option<MDGTechnology>>("ea.addIn.technology", Options.None<MDGTechnology>());

        public EAAddIn()
        {
            logger.Info("Init add-in {0}", AddInName);
            customDetailViewHandler = new CustomDetailViewHandler(eaRepository);
            menuHandler = new MenuHandler(contextItem);
        }

        public abstract String AddInName { get; }

        public abstract void bootstrap(IReadableAtom<EA.Repository> repository);

        public virtual Option<MDGTechnology> bootstrapTechnology()
        {
            return Options.None<MDGTechnology>();
        }

        #region component registration
        protected void Register(ICustomDetailViewComponent component)
        {
            customDetailViewHandler.Register(component);
        }

        protected void Register(IMenuItem menu)
        {
            menuHandler.Register(menu);
        }
        #endregion

        #region manage state
        private void RepositoryChanged(EA.Repository repository)
        {
            eaRepository.Exchange(repository, GetType());
        }

        private void ContextItemChanged(EA.ObjectType type, string guid)
        {
            if (type == EA.ObjectType.otRepository)
            {
                contextItem.Exchange(Options.None<ContextItem>(), GetType());
            }
            else
            {
                contextItem.Exchange(new ContextItem(type, guid).AsOption(), GetType());
            }
        }

        private void MenuLocationChanged(string menuLocation)
        {
            if (menuLocation == "MainMenu")
            {
                contextItem.Exchange(Options.None<ContextItem>(), GetType());
            }
        }
        #endregion

        #region add in actions
        public string EA_Connect(EA.Repository repository)
        {
            RepositoryChanged(repository);

            logger.Info("Start add-in {0}", AddInName);
            bootstrap(eaRepository);

            return "";
        }

        public void EA_Disconnect()
        {
            logger.Info("Stop add-In {0}", AddInName);
            LogManager.Shutdown();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        public object EA_OnInitializeTechnologies(EA.Repository repository)
        {
            technology.Exchange(bootstrapTechnology(), GetType());
            return technology.Val.Select(tech =>
                {
                    logger.Info("Initialise MDG Technology {0}", tech.ID);
                    logger.Debug("Technology XML: {0}", tech.ToXml().ToString());
                    return tech.ToXml().ToString();
                }).GetOrElse("");
        }

        public String EA_OnRetrieveModelTemplate(EA.Repository repository, String location)
        {
            return (from tech in technology.Val
                    from template in tech.ModelTemplates
                    where template.ResourceName == location
                    select template.GetXmi())
                   .FirstOption()
                   .GetOrElse("");
        }
        #endregion

        #region menu actions
        public object EA_GetMenuItems(EA.Repository repository, String menuLocation, String menuName)
        {
            RepositoryChanged(repository);
            MenuLocationChanged(menuLocation);

            var itemNames = menuHandler.GetMenuItems(menuName);

            logger.Debug("Define menu entries {0} for parent menu \"{1}\" in {2}",
                String.Join(", ", itemNames.AsOption().GetOrElse(new string[] { })), menuName, menuLocation);
            return itemNames;
        }

        public void EA_GetMenuState(EA.Repository repository, string menuLocation, string menuName, string itemName, ref bool isEnabled, ref bool isChecked)
        {
            RepositoryChanged(repository);
            MenuLocationChanged(menuLocation);

            var enabled = menuHandler.IsItemEnabled(menuName, itemName);

            logger.Debug("\"{0}\" in \"{1}\" is enabled = {2}", itemName, menuName, enabled);

            isEnabled = enabled;
        }

        public void EA_MenuClick(EA.Repository repository, string menuLocation, string menuName, string itemName)
        {
            RepositoryChanged(repository);
            MenuLocationChanged(menuLocation);

            logger.Debug("Handle click on \"{0}\" in \"{1}\"", itemName, menuName);

            menuHandler.HandleClick(menuName, itemName);
        }
        #endregion

        #region element actions
        public bool EA_OnPreNewElement(EA.Repository repository, EA.EventProperties info)
        {
            RepositoryChanged(repository);

            logger.Debug("Create element with type = {0}, stereotype = {1}, parentId = {2}, diagramId = {3}",
                info.ExtractType(), info.ExtractStereotype(), info.ExtractParentId(), info.ExtractDiagramId());

            return true;
        }

        public bool EA_OnPostNewElement(EA.Repository repository, EA.EventProperties info)
        {
            RepositoryChanged(repository);

            var elementId = info.ExtractElementId();
            logger.Debug("Element with id {0} created", elementId);

            return customDetailViewHandler.CallElementDetailViews(() => eaRepository.Val.GetElementByID(elementId), true);
        }

        public void EA_OnNotifyContextItemModified(EA.Repository repository, string guid, EA.ObjectType ot)
        {
            RepositoryChanged(repository);

            logger.Debug("Element {0} of type {1} modified", guid, ot);
        }

        public void EA_OnContextItemChanged(EA.Repository repository, string guid, EA.ObjectType ot)
        {
            RepositoryChanged(repository);
            ContextItemChanged(ot, guid);

            logger.Debug("Context item changed to {0} of object type {1}", guid, ot);
        }

        public bool EA_OnContextItemDoubleClicked(EA.Repository repository, string guid, EA.ObjectType ot)
        {
            RepositoryChanged(repository);

            logger.Debug("Context item {0} of object type {1} double clicked", guid, ot);

            if (ot == EA.ObjectType.otElement)
            {
                return customDetailViewHandler.CallElementDetailViews(() => eaRepository.Val.GetElementByGuid(guid), false);
            }

            return false;
        }
        #endregion
    }
}
