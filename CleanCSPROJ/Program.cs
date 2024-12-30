using System;

Console.WriteLine("Enter path to .csproj file:");
var fn = Console.ReadLine();
var lines = File.ReadAllLines(fn).ToList();

//here can be used 1 file and 2 pointers, if skip the line then p2 (source)++, but p1 (target) stay at the same pos

var used = new HashSet<string>();

using(var sw = new StreamWriter(fn+"_clean"))
{
    var duplicatesCount = 0;
    foreach(var l in lines)
    {
        if(l.Contains("Compile Include="))
        {
            var f = l.Split("\"")[1];
            if(used.Contains(f))
            {
                duplicatesCount++;
                if(!l.EndsWith("/>"))
                {
                    Console.WriteLine("Please see file: "+f);
                }
                else
                    continue;
            }
            used.Add(f);
        }

        sw.WriteLine(l);
    }
    Console.WriteLine("duplicatesCount:"+duplicatesCount);
}
