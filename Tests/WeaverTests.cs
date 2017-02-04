namespace Tests
{
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Mono.Cecil;
    using NUnit.Framework;

    [TestFixture]
    public class WeaverTests
    {
        private Assembly assembly;
        private string beforeAssemblyPath;
        private string afterAssemblyPath;

        [TestFixtureSetUp]
        public void Setup()
        {
            beforeAssemblyPath =
                Path.GetFullPath(Path.Combine(TestContext.CurrentContext.TestDirectory,
                    @"..\..\..\AssemblyToProcess\bin\Debug\AssemblyToProcess.dll"));
#if (!DEBUG)

        beforeAssemblyPath = beforeAssemblyPath.Replace("Debug", "Release");
#endif

            afterAssemblyPath = beforeAssemblyPath.Replace(".dll", "2.dll");
            File.Copy(beforeAssemblyPath, afterAssemblyPath, true);

            var moduleDefinition = ModuleDefinition.ReadModule(afterAssemblyPath);
            var weavingTask = new ModuleWeaver
            {
                ModuleDefinition = moduleDefinition
            };

            weavingTask.Execute();
            moduleDefinition.Write(afterAssemblyPath);

            assembly = Assembly.LoadFile(afterAssemblyPath);
        }

#if (DEBUG)
        [Test]
        public void PeVerify()
        {
            Verifier.Verify(beforeAssemblyPath, afterAssemblyPath);
        }
#endif

        [Test]
        public void ValidateProperties()
        {
            var type = assembly.GetTypes().Single(x => x.Name == "Class1");
            Assert.IsTrue(type.GetProperty("MyInt").CanWrite);
        }
    }
}