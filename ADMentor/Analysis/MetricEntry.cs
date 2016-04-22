using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADMentor.Analysis
{
    public class MetricEntry
    {
        public MetricEntry(String property, object value) : this("", "", property, value) { }

        public MetricEntry(String category, String group, String property, object value)
        {
            this.Category = category;
            this.Group = group;
            this.Property = property;
            this.Value = value;
        }

        public string Category { get; private set; }

        public string Group { get; private set; }

        public string Property { get; private set; }

        public object Value { get; private set; }

        public String Key()
        {
            return String.Format("{0}.{1}.{2}", Category, Group, Property);
        }

        public MetricEntry Copy(String category = null, String group = null, String property = null, String value = null)
        {
            return new MetricEntry(
                category == null ? Category : category,
                group == null ? Group : group,
                property == null ? Property : property,
                value == null ? Value : value);
        }

        public override string ToString()
        {
            return String.Format("{0} = {1}", Key(), Value);
        }
    }
}
