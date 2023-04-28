using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace CodeRunner.Worker.Services
{
    internal class Runner
    {
        const string RuntimeConfigContent = """
                {
                  "runtimeOptions": {
                    "tfm": "net7.0",
                    "framework": {
                      "name": "Microsoft.NETCore.App",
                      "version": "7.0.0"
                    }
                  }
                }
            """;

        public void Execute(byte[] compiledAssembly, string[] args)
        {
            var assemblyLoadContextWeakRef = LoadAndExecute(compiledAssembly, args);

            for (var i = 0; i < 8 && assemblyLoadContextWeakRef.IsAlive; i++)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static WeakReference LoadAndExecute(byte[] compiledAssembly, string[] args)
        {
            using (var asm = new MemoryStream(compiledAssembly))
            {
                var assemblyLoadContext = new SimpleUnloadableAssemblyLoadContext();
                var assembly = assemblyLoadContext.LoadFromStream(asm);

                var entry = assembly.EntryPoint;

                _ = entry != null && entry.GetParameters().Length > 0
                    ? entry.Invoke(null, new object[] { args })
                    : entry.Invoke(null, null);

                assemblyLoadContext.Unload();

                return new WeakReference(assemblyLoadContext);
            }
        }

        public (string ouput, string error) ExecuteInSeparateProcess(byte[] compiledAssembly, string[] args)
        {
            var tempFileName = Path.GetTempFileName();
            var dllPath = tempFileName + ".dll";
            var runtimeConfigPath = tempFileName + ".runtimeconfig.json";

            File.WriteAllBytes(dllPath, compiledAssembly);
            File.WriteAllText(runtimeConfigPath, RuntimeConfigContent);

            var processStartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"{dllPath} {string.Join(" ", args)}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };

            var curDir = Path.GetDirectoryName(typeof(object).Assembly.Location);
            processStartInfo.EnvironmentVariables["PATH"] += ";" + curDir;

            var process = new Process { StartInfo = processStartInfo };
            process.Start();
            process.WaitForExit();

            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();

            Console.WriteLine($"Output: {output}");
            Console.WriteLine($"Error: {error}");

            File.Delete(dllPath);
            File.Delete(runtimeConfigPath);

            return (output, error);
        }
    }
}