using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADAddInTests
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

        private static readonly EA.Repository Repository;

        static RepositoryUnderTest()
        {
            Repository = new EA.Repository();
            Repository.CreateModel(EA.CreateModelType.cmEAPFromBase, RepositoryPath, 0);
            Repository.OpenFile(RepositoryPath);
        }

        public RepositoryUnderTest()
        {
            var rootModelName = "Model_" + RandomNumberGenerator.Next();
            RootModel = Repository.Models.AddNew(rootModelName, "") as EA.Package;
            if (!RootModel.Update())
            {
                throw new ApplicationException(RootModel.GetLastError());
            }
        }

        private sealed class Finalizer
        {
            ~Finalizer()
            {
                Repository.Exit();
                File.Delete(RepositoryPath);
            }
        }

        public EA.Package RootModel { get; private set; }

        public EA.Repository Repo { get { return Repository; } }
    }
}
