using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Utils;

namespace EAAddInFramework.MDGBuilder
{
    /// <summary>
    /// Model templates as shown in the new model wizard.
    /// 
    /// More infos on the MTS format for model templates:
    /// http://www.sparxsystems.com/enterprise_architect_user_guide/10/extending_uml_models/model_templates2.html
    /// </summary>
    public class ModelTemplate
    {
        public ModelTemplate(String name, String description, String resourceName, ModelIcon icon = null)
        {
            Assembly = Assembly.GetCallingAssembly();
            Name = name;
            Description = description;
            ResourceName = resourceName;
            Icon = icon.AsOption();
        }

        public string Name { get; private set; }

        public string Description { get; private set; }

        public string ResourceName { get; private set; }

        public Option<ModelIcon> Icon { get; private set; }

        private Assembly Assembly { get; set; }

        public XElement ToXml()
        {
            return new XElement("Model",
                new XAttribute("name", Name),
                new XAttribute("description", Description),
                new XAttribute("location", ResourceName),
                new XAttribute("icon", Icon.Select(i => i.Name).GetOrElse("")));
        }

        internal String GetXmi()
        {
            return Utils.Resources.GetAsString(Assembly, ResourceName);
        }
    }

    public class ModelIcon : Enumeration
    {
        public static readonly ModelIcon UseCaseModel = new ModelIcon("29");
        public static readonly ModelIcon DynamicModel = new ModelIcon("30");
        public static readonly ModelIcon ClassModel = new ModelIcon("31");
        public static readonly ModelIcon ComponentModel = new ModelIcon("32");
        public static readonly ModelIcon DeploymentModel = new ModelIcon("33");
        public static readonly ModelIcon SimpleModel = new ModelIcon("34");

        private ModelIcon(String name) : base(name) { }
    }
}
