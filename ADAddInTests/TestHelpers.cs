using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADAddInTests
{
    public static class TestHelpers
    {
        public static EA.Repository LoadDefaultTestModel()
        {
            var repo = new EA.Repository();
            repo.OpenFile("TestModel.eap");
            return repo;
        }
    }
}
