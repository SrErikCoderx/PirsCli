// Main entry point for the CLI application

using System;
using System.Collections.Generic;
using System.IO;
using Pirs;

public class Program
{
    public static void Main(string[] args)
    {
        if (args.Length < 2)
        {
            PrintUsage();
            return;
        }

        string command = args[0].ToLower();
        string filePath = args[1];

        if (!File.Exists(filePath))
        {
            Console.WriteLine($"Error: File not found at '{filePath}'");
            return;
        }

        try
        {
            var extractor = new PirsExtractor();
            if (command == "-l" || command == "list")
            {
                Console.WriteLine($"Contents of '{Path.GetFileName(filePath)}':");
                List<PirsEntry> entries = extractor.ListEntries(filePath);
                PrintEntries(entries);
            }
            else if (command == "-x" || command == "extract")
            {
                if (args.Length < 3)
                {
                    Console.WriteLine("Error: Output directory not specified for extraction.");
                    PrintUsage();
                    return;
                }
                string outputDir = args[2];
                Console.WriteLine($"Extracting all files from '{Path.GetFileName(filePath)}' to '{outputDir}'...");
                
                List<PirsEntry> entries = extractor.ListEntries(filePath);
                if (entries.Count == 0)
                {
                    Console.WriteLine("No files found to extract.");
                    return;
                }

                foreach (var entry in entries)
                {
                    if (entry.IsDirectory) continue;
                    
                    Console.WriteLine($"- Extracting {entry.Filename}...");
                    string outputFilePath = Path.Combine(outputDir, entry.Filename);
                    extractor.ExtractFile(filePath, entry, outputFilePath);
                }
                Console.WriteLine("Extraction complete.");
            }
            else
            {
                Console.WriteLine($"Error: Unknown command '{command}'");
                PrintUsage();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

    private static void PrintEntries(List<PirsEntry> entries)
    {
        if (entries.Count == 0)
        {
            Console.WriteLine("No entries found.");
        }
        else
        {
            foreach (var entry in entries)
            {
                Console.WriteLine($"- {entry.ToString()}");
            }
        }
    }

    private static void PrintUsage()
    {
        Console.WriteLine("wxPirs CLI Decompilation");
        Console.WriteLine("Usage:");
        Console.WriteLine("  PirsCli -l <file_path>          (List contents)");
        Console.WriteLine("  PirsCli -x <file_path> <out_dir> (Extract contents)");
    }
}