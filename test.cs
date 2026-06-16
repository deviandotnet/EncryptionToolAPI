using System;
using System.Reflection;
using System.Linq;

class Program {
    static void Main() {
        var a = Assembly.LoadFile("C:\Users\Administrator\.nuget\packages\microsoft.openapi\2.7.5\lib\netstandard2.0\Microsoft.OpenApi.dll");
        foreach(var t in a.GetTypes().Where(t => t.Name.Contains("OpenApiInfo"))) {
            Console.WriteLine(t.FullName);
        }
    }
}
