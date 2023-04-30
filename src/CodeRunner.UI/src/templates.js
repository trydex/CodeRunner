export default [
    {
        processCount: 1,
        code: `
using System;

Console.WriteLine("Hello World");`
    },
    {
        processCount: 5,
        code: `
using System;
using System.Diagnostics;

Console.WriteLine("Hi from process #" + Process.GetCurrentProcess().Id);`
    },
    {
        processCount: 3,
        code: `
using System;

for(var i = 0; i < 10; i++)
  Console.WriteLine(i);`
    }
]