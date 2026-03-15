using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("File Comparison Tool");
        Console.WriteLine("====================");

        // Get file paths from user
        Console.Write("Enter path to first file: ");
        string file1Path = Console.ReadLine();

        Console.Write("Enter path to second file: ");
        string file2Path = Console.ReadLine();

        // Validate files exist
        if (!File.Exists(file1Path))
        {
            Console.WriteLine($"Error: File '{file1Path}' not found.");
            return;
        }

        if (!File.Exists(file2Path))
        {
            Console.WriteLine($"Error: File '{file2Path}' not found.");
            return;
        }

        try
        {
            // Read both files into lists to preserve duplicates
            var allLines1 = File.ReadAllLines(file1Path).ToList();
            var allLines2 = File.ReadAllLines(file2Path).ToList();

            // Read into sets for comparison
            var lines1Set = new HashSet<string>(allLines1);
            var lines2Set = new HashSet<string>(allLines2);

            string file1Name = Path.GetFileName(file1Path);
            string file2Name = Path.GetFileName(file2Path);

            // Find duplicates in each file
            var duplicatesFile1 = FindDuplicates(allLines1);
            var duplicatesFile2 = FindDuplicates(allLines2);

            // Find matching lines (intersection)
            var matchingLines = lines1Set.Intersect(lines2Set).ToList();

            // Find unique lines in file1
            var uniqueInFile1 = lines1Set.Except(lines2Set).ToList();

            // Find unique lines in file2
            var uniqueInFile2 = lines2Set.Except(lines1Set).ToList();

            // Write matching lines to output file
            string matchingOutputPath = "matching_lines.txt";
            using (var writer = new StreamWriter(matchingOutputPath))
            {
                foreach (var line in matchingLines.OrderBy(l => l))
                {
                    writer.WriteLine(line);
                }
            }
            Console.WriteLine($"✓ Matching lines written to: {matchingOutputPath} ({matchingLines.Count} lines)");

            // Write unique lines to output file with source file name
            string uniqueOutputPath = "unique_lines.txt";
            using (var writer = new StreamWriter(uniqueOutputPath))
            {
                // Write unique lines from file1
                foreach (var line in uniqueInFile1.OrderBy(l => l))
                {
                    writer.WriteLine($"{line}\t{file1Name}");
                }

                // Write unique lines from file2
                foreach (var line in uniqueInFile2.OrderBy(l => l))
                {
                    writer.WriteLine($"{line}\t{file2Name}");
                }
            }
            Console.WriteLine($"✓ Unique lines written to: {uniqueOutputPath} ({uniqueInFile1.Count + uniqueInFile2.Count} lines)");

            // Write duplicates report
            string duplicatesOutputPath = "duplicates_report.txt";
            using (var writer = new StreamWriter(duplicatesOutputPath))
            {
                writer.WriteLine($"DUPLICATES REPORT\n");

                writer.WriteLine($"=== Duplicates in {file1Name} ===");
                if (duplicatesFile1.Count > 0)
                {
                    foreach (var kvp in duplicatesFile1.OrderBy(k => k.Key))
                    {
                        writer.WriteLine($"{kvp.Key}\tCount: {kvp.Value}");
                    }
                }
                else
                {
                    writer.WriteLine("No duplicates found");
                }

                writer.WriteLine($"\n=== Duplicates in {file2Name} ===");
                if (duplicatesFile2.Count > 0)
                {
                    foreach (var kvp in duplicatesFile2.OrderBy(k => k.Key))
                    {
                        writer.WriteLine($"{kvp.Key}\tCount: {kvp.Value}");
                    }
                }
                else
                {
                    writer.WriteLine("No duplicates found");
                }
            }
            Console.WriteLine($"✓ Duplicates report written to: {duplicatesOutputPath}");

            // Print summary
            Console.WriteLine();
            Console.WriteLine("Summary:");
            Console.WriteLine($"  File 1: {file1Name} ({allLines1.Count} total lines, {lines1Set.Count} unique)");
            Console.WriteLine($"  File 2: {file2Name} ({allLines2.Count} total lines, {lines2Set.Count} unique)");
            Console.WriteLine($"  Matching lines: {matchingLines.Count}");
            Console.WriteLine($"  Unique in {file1Name}: {uniqueInFile1.Count}");
            Console.WriteLine($"  Unique in {file2Name}: {uniqueInFile2.Count}");
            Console.WriteLine($"  Duplicates in {file1Name}: {duplicatesFile1.Count}");
            Console.WriteLine($"  Duplicates in {file2Name}: {duplicatesFile2.Count}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    static Dictionary<string, int> FindDuplicates(List<string> lines)
    {
        var duplicates = new Dictionary<string, int>();
        var counts = new Dictionary<string, int>();

        foreach (var line in lines)
        {
            if (counts.ContainsKey(line))
            {
                counts[line]++;
            }
            else
            {
                counts[line] = 1;
            }
        }

        foreach (var kvp in counts)
        {
            if (kvp.Value > 1)
            {
                duplicates[kvp.Key] = kvp.Value;
            }
        }

        return duplicates;
    }
}
