using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ModuleLoadingSystem
{
    public class ModuleLoadingSystem : IDisposable
    {
        private CSharpCodeProvider _provider = new CSharpCodeProvider();

        public void Dispose()
        {
            _provider.Dispose();
        }

        private string[] getReferencedAssemblies(string[] sources)
        {
            List<string> references = new List<string>();
            foreach(string source in sources)
            {
                using (StringReader sr = new StringReader(source))
                {
                    string line = sr.ReadLine();
                    line = line.Remove(0, 3); // Remove `// `
                    foreach(string reference in line.Split(','))
                    {
                        references.Add(reference);
                    }
                }
            }

            return references.Distinct().ToArray();
        }

        public Type Compile(String[] sources, string typename)
        {
            if(sources.Length < 1)
            {
                throw new Exception("Not enough files");
            }

            CompilerParameters compilerParameters = new CompilerParameters();

            compilerParameters.GenerateExecutable = false;
            compilerParameters.GenerateInMemory = true;
            compilerParameters.IncludeDebugInformation = false;
            compilerParameters.TreatWarningsAsErrors = false;
            compilerParameters.ReferencedAssemblies.AddRange(getReferencedAssemblies(sources));

            CompilerResults result = _provider.CompileAssemblyFromSource(compilerParameters, sources);
            if (result.Errors.HasErrors)
            {
                Console.Error.WriteLine("Failed to compile");
                foreach (CompilerError error in result.Errors)
                {
                    Console.Error.WriteLine("\t" + error.ErrorText);
                }
                throw new Exception("Failed to compile source");
            }

            return result.CompiledAssembly.GetType(typename);
        }
    }
}
