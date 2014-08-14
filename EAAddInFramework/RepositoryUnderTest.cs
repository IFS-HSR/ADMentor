using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EAAddInFramework
{
    /// <summary>
    /// Manages an EA repository that may be used for unit tests.
    /// 
    /// Because the repository is shared over all tests, a test should only modify the model
    /// provided by the <c>RootModel</c> property.
    /// </summary>
    public class RepositoryUnderTest
    {
        protected static readonly String RepositoryPath = AppDomain.CurrentDomain.BaseDirectory + "\\testModel.eap";

        private static readonly Random RandomNumberGenerator = new Random();

        /// <summary>
        /// Use separate finalizer object as "static" destructor
        /// </summary>
        private static readonly Finalizer finalizer = new Finalizer();

        private static readonly EA.App App;

        static RepositoryUnderTest()
        {
            App = new EA.App();
            App.Repository.CreateModel(EA.CreateModelType.cmEAPFromBase, RepositoryPath, 0);
            App.Repository.OpenFile(RepositoryPath);
        }

        public RepositoryUnderTest()
        {
            var rootModel = Repo.Models.GetAt(0) as EA.Package;
            var testPackageName = "Test_" + RandomNumberGenerator.Next();
            TestPackage = rootModel.Packages.AddNew(testPackageName, "") as EA.Package;
            TestPackage.Update();
        }

        private sealed class Finalizer
        {
            ~Finalizer()
            {
                App.Repository.CloseFile();
                File.Delete(RepositoryPath);
            }
        }

        public EA.Package TestPackage { get; private set; }

        public EA.Repository Repo { get { return App.Repository; } }
    }
}
