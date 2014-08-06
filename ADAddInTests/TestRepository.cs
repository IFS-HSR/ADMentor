using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADAddInTests
{
    class TestRepository : IDisposable
    {
        private String path = AppDomain.CurrentDomain.BaseDirectory + "\\testModel.eap";

        public TestRepository()
        {
            EARepo = new EA.Repository();
            EARepo.CreateModel(EA.CreateModelType.cmEAPFromBase, path, 0);
            EARepo.OpenFile(path);
            RootModel = EARepo.Models.AddNew("RootModel", "") as EA.Package;
            if (!RootModel.Update())
            {
                throw new ApplicationException(RootModel.GetLastError());
            }
        }

        public EA.Repository EARepo { get; private set; }

        public EA.Package RootModel { get; private set; }

        public void Dispose()
        {
            EARepo.Exit();
            File.Delete(path);
        }
    }
}
