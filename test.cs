using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;

class Program
{
    static void Main()
    {
        var files = Directory.GetFiles(@"OCC.Client\OCC.Client\Features", "*.axaml", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            var content = File.ReadAllText(file);
            if (!content.Contains("<DataGrid")) continue;
            
            var rowMatch = Regex.Match(content, @"RowDefinitions=""([^""]+)""");
            var dgRowMatch = Regex.Match(content, @"<DataGrid[^>]*Grid\.Row=""(\d+)""");
            
            string r = rowMatch.Success ? rowMatch.Groups[1].Value : "NONE";
            string dr = dgRowMatch.Success ? dgRowMatch.Groups[1].Value : "NONE";
            
            Console.WriteLine($"{Path.GetFileName(file)}|{r}|{dr}");
        }
    }
}
