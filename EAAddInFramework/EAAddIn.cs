using EAAddInFramework.DataAccess;
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

        private readonly Atom<Lazy<Option<ModelEntity>>> contextItem =
            new LoggedAtom<Lazy<Option<ModelEntity>>>("ea.contextItem", new Lazy<Option<ModelEntity>>(() => Options.None<ModelEntity>()));

        private readonly MenuHandler menuHandler;

        private readonly Atom<Option<MDGTechnology>> technology = new LoggedAtom<Option<MDGTechnology>>("ea.addIn.technology", Options.None<MDGTechnology>());

        private readonly Atom<IEntityWrapper> entityWrapper = new LoggedAtom<IEntityWrapper>("ea.addIn.entityWrapper", new EntityWrapper());

        public EAAddIn()
        {
            logger.Info("Init add-in {0}", AddInName);
            menuHandler = new MenuHandler(contextItem);
        }

        public abstract String AddInName { get; }

        public abstract Option<IEntityWrapper> Bootstrap(IReadableAtom<EA.Repository> repository);

        public virtual Option<MDGTechnology> BootstrapTechnology()
        {
            return Options.None<MDGTechnology>();
        }

        #region component registration
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
            switch(type){
                case EA.ObjectType.otElement:
                    SetContextItem(() => Options.Some(eaRepository.Val.GetElementByGuid(guid)));
                    break;
                case EA.ObjectType.otConnector:
                    SetContextItem(() => Options.Some(eaRepository.Val.GetConnectorByGuid(guid)));
                    break;
                case EA.ObjectType.otDiagram:
                    SetContextItem(() => Options.Some(eaRepository.Val.GetDiagramByGuid(guid)));
                    break;
                case EA.ObjectType.otPackage:
                    SetContextItem(() => Options.Some(eaRepository.Val.GetPackageByGuid(guid)));
                    break;
                default:
                    SetContextItem(() => Options.None<object>());
                    break;
            }
        }

        private void SetContextItem(Func<Option<dynamic>> getEaObject)
        {
            contextItem.Exchange(
                new Lazy<Option<ModelEntity>>(
                    () => getEaObject().Select(eaObject => entityWrapper.Val.Wrap(eaObject) as ModelEntity),
                    System.Threading.LazyThreadSafetyMode.None),
                GetType());
        }

        private void MenuLocationChanged(string menuLocation)
        {
            if (menuLocation == "MainMenu")
            {
                SetContextItem(() => Options.None<object>());
            }
        }
        #endregion

        #region add in actions
        public string EA_Connect(EA.Repository repository)
        {
            RepositoryChanged(repository);

            logger.Info("Start add-in {0}", AddInName);
            Bootstrap(eaRepository).Do(wrapper =>
            {
                entityWrapper.Exchange(wrapper, GetType());
            });

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
            technology.Exchange(BootstrapTechnology(), GetType());
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

        public readonly EventManager<ModelEntity, EntityModified> OnEntityCreated =
            new EventManager<ModelEntity, EntityModified>(
                EntityModified.NotModified,
                (acc, v) => acc == EntityModified.Modified ? acc : v);

        public readonly EventManager<ModelEntity, object> OnEntityModified =
            new EventManager<ModelEntity, object>(
                Unit.Instance,
                (acc, _) => acc);

        public readonly EventManager<ModelEntity, DeleteEntity> OnDeleteEntity =
            new EventManager<ModelEntity, DeleteEntity>(
                DeleteEntity.Delete,
                (acc, v) => acc == DeleteEntity.PreventDelete ? acc : v);

        private R Handle<R>(EventManager<ModelEntity, R> em, Func<dynamic> getEaObject)
        {
            return em.Handle(() => entityWrapper.Val.Wrap(getEaObject()));
        }

        /// <summary>
        /// EA_OnPostNewElement notifies Add-Ins that a new element has been created on a diagram. It enables Add-Ins to
        /// modify the element upon creation.
        /// 
        /// This event occurs after a user has dragged a new element from the Toolbox or Resources window onto a diagram.
        /// The notification is provided immediately after the element is added to the model. Set Repository.SuppressEADialogs
        /// to true to suppress Enterprise Architect from showing its default dialogs.
        /// </summary>
        /// <returns>Return True if the element has been updated during this notification. Return False otherwise.</returns>
        public bool EA_OnPostNewElement(EA.Repository repository, EA.EventProperties info)
        {
            RepositoryChanged(repository);

            var elementId = info.ExtractElementId();
            logger.Debug("Element with id {0} created", elementId);

            var entityModified = Handle(OnEntityCreated, () => eaRepository.Val.GetElementByID(elementId));

            return entityModified.AsBool;
        }

        public bool EA_OnPreDeleteConnector(EA.Repository repository, EA.EventProperties info)
        {
            RepositoryChanged(repository);

            var connectorId = info.ExtractConnectorId();
            logger.Debug("Attempt to delete connector with id {0}", connectorId);

            var deleteConnector = Handle(OnDeleteEntity, () => eaRepository.Val.GetConnectorByID(connectorId));

            return deleteConnector.AsBool;
        }

        public void EA_OnNotifyContextItemModified(EA.Repository repository, string guid, EA.ObjectType ot)
        {
            RepositoryChanged(repository);

            logger.Debug("Context item {0} of type {1} modified", guid, ot);

            if (ot == EA.ObjectType.otElement)
            {
                Handle(OnEntityModified, () => eaRepository.Val.GetElementByGuid(guid));
            }
        }

        public void EA_OnContextItemChanged(EA.Repository repository, string guid, EA.ObjectType ot)
        {
            RepositoryChanged(repository);
            ContextItemChanged(ot, guid);

            logger.Debug("Context item changed to {0} of object type {1}", guid, ot);
        }

        /// <summary>
        /// EA_OnContextItemDoubleClicked notifies Add-Ins that the user has double-clicked the item currently in context.
        /// 
        /// This event occurs when a user has double-clicked (or pressed ( Enter ) ) on the item in context, either in a diagram
        /// or in the Project Browser. Add-Ins to handle events can subscribe to this broadcast function.
        /// </summary>
        /// <returns>Return True to notify Enterprise Architect that the double-click event has been handled by an Add-In.
        /// Return False to enable Enterprise Architect to continue processing the event.</returns>
        public bool EA_OnContextItemDoubleClicked(EA.Repository repository, string guid, EA.ObjectType ot)
        {
            RepositoryChanged(repository);

            logger.Debug("Context item {0} of object type {1} double clicked", guid, ot);

            if (ot == EA.ObjectType.otElement)
            {
                //return customDetailViewHandler.CallElementDetailViews(() => eaRepository.Val.GetElementByGuid(guid)).Val;
            }

            return false;
        }
        #endregion
    }

    public class EntityModified
    {
        public static readonly EntityModified Modified = new EntityModified(true);
        public static readonly EntityModified NotModified = new EntityModified(false);

        private EntityModified(bool val) { AsBool = val; }

        public bool AsBool { get; private set; }
    }

    public class DeleteEntity
    {
        public static readonly DeleteEntity Delete = new DeleteEntity(true);
        public static readonly DeleteEntity PreventDelete = new DeleteEntity(false);

        private DeleteEntity(bool val) { AsBool = val; }

        public bool AsBool { get; private set; }
    }
}
