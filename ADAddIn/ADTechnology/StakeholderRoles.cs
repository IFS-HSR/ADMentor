using EAAddInFramework.MDGBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdAddIn.ADTechnology
{
    public static class StakeholderRoles
    {
        public static readonly ElementStereotype StakeholderRole = new ElementStereotype(
            name: "adStakeholderRole",
            displayName: "Stakeholder Role",
            type: ElementType.Actor);

        public static readonly TaggedValue StakeholderRoleRef = new TaggedValue(
            name: "Stakeholder Role",
            type: TaggedValueTypes.Reference(StakeholderRole));
    }
}
