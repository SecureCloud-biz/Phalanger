﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace VersionReplacer
{
    class Program
    {
        static void Main(string[] args)
        {
            var fname = args[0];
            var changeset = args[1];
            bool isvsix = fname.EndsWith(".vsixmanifest", StringComparison.InvariantCultureIgnoreCase);
            bool iscs = fname.EndsWith(".cs", StringComparison.InvariantCultureIgnoreCase);

            Console.WriteLine("Updating version info in '{0}'", fname);

            if (!isvsix && !iscs)
                return;

            MatchEvaluator versionChanger = (match) =>
                {
                    var line = match.Groups[0].Value;
                    var old = match.Groups[1];
                    return line.Remove(old.Index - match.Groups[0].Index) + changeset + line.Substring(old.Index - match.Groups[0].Index + old.Length);
                };

            var result = new StringBuilder();
            foreach (var line in File.ReadAllLines(fname))
            {
                if (isvsix)
                {
                    if (line.Contains("<Version>"))
                    {
                        // 1.2.3
                        var regex = new Regex(@"<Version>[0-9]+\.[0-9]+\.(.+)</Version>");
                        var newline = (regex.Replace(line, versionChanger));
                        result.AppendLine(newline);
                        continue;
                    }
                }
                else if (iscs)
                {
                    if (line.Contains("public const string Version"))
                    {
                        // 1.2.3
                        var regex = new Regex(@"public\s+const\s+string\s+Version\s+\=\s+""[0-9]+\.[0-9]+\.(.+)""");
                        var newline = (regex.Replace(line, versionChanger));
                        result.AppendLine(newline);
                        continue;
                    }

                    if (line.Contains("[assembly: AssemblyFileVersion"))
                    {
                        // 1.2.3
                        var regex = new Regex(@"[assembly\:\s+AssemblyFileVersion\(""[0-9]+\.[0-9]+\.([0-9]+)""\)");
                        var newline = (regex.Replace(line, versionChanger));
                        result.AppendLine(newline);
                        continue;
                    }
                }
                
                // copy the line to output:
                result.AppendLine(line);
            }

            File.SetAttributes(fname, FileAttributes.Normal);
            File.WriteAllText(fname, result.ToString());
        }
    }
}
