using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ADAddInTests.DecisionDetails
{
    [TestClass]
    public class DecisionDetailViewComponentTest
    {
        [TestMethod]
        public void ChangeElementName()
        {
            var repo = new EA.Repository();
            repo.OpenFile("TestModel.eap");
        }
    }
}
