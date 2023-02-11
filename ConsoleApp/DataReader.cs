using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ConsoleApp
{
    public class DataReader
    {
        public void ImportAndPrintData(string fileToImport)
        {
            var importedObjects = new List<ImportedObject>() { };
            var streamReader = new StreamReader(fileToImport);
            var importedLines = new List<string>();

            while (!streamReader.EndOfStream)
            {
                var line = streamReader.ReadLine();

                if (!string.IsNullOrWhiteSpace(line))
                {
                    importedLines.Add(line);
                }
            }

            foreach (string importedLine in importedLines)
            {
                string[] values = importedLine.Split(';');

                var importedObject = new ImportedObject()
                {
                    Type = values[0],
                    Name = values[1],
                    Schema = values[2],
                    ParentName = values[3],
                    ParentType = values[4],
                    DataType = values[5],
                    IsNullable = values.Length < 7 ? default : values[6]
                };

                importedObject.Type = importedObject.Type
                    .Trim()
                    .Replace(" ", "")
                    .Replace(Environment.NewLine, "")
                    .ToUpper();
                importedObject.Name = importedObject.Name
                    .Trim()
                    .Replace(" ", "")
                    .Replace(Environment.NewLine, "");
                importedObject.Schema = importedObject.Schema
                    .Trim()
                    .Replace(" ", "")
                    .Replace(Environment.NewLine, "");
                importedObject.ParentName = importedObject.ParentName
                    .Trim()
                    .Replace(" ", "")
                    .Replace(Environment.NewLine, "");
                importedObject.ParentType = importedObject.ParentType
                    .Trim()
                    .Replace(" ", "")
                    .Replace(Environment.NewLine, "");

                importedObjects.Add(importedObject);
            }

            CountNumberOfChildren(importedObjects);

            PrintFile(importedObjects);

            Console.ReadKey();
        }

        public static void PrintFile(IEnumerable<ImportedObject> importedObjects)
        {
            foreach (var database in importedObjects)
            {
                if (database.Type == "DATABASE")
                {
                    Console.WriteLine($"Database '{database.Name}' ({database.NumberOfChildren} tables)");

                    foreach (var table in importedObjects)
                    {
                        if (table.ParentType.ToUpper() == database.Type)
                        {
                            if (table.ParentName == database.Name)
                            {
                                Console.WriteLine($"\tTable '{table.Schema}.{table.Name}' " +
                                    $"({table.NumberOfChildren} columns)");

                                foreach (var column in importedObjects)
                                {
                                    if (column.ParentType.ToUpper() == table.Type)
                                    {
                                        if (column.ParentName == table.Name)
                                        {
                                            Console.WriteLine($"\t\tColumn '{column.Name}' with {column.DataType} data type " +
                                                $"{(column.IsNullable == "1" ? "accepts nulls" : "with no nulls")}");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void CountNumberOfChildren(IEnumerable<ImportedObject> importedObjects)
        {
            var databaseList = importedObjects.Where(o => o.Type == "DATABASE");
            var tableList = importedObjects.Where(o => o.Type == "TABLE");
            var columnList = importedObjects.Where(o => o.Type == "COLUMN");

            foreach(var database in databaseList)
            {
                database.NumberOfChildren = tableList
                    .Where(t => t.ParentName == database.Name)
                    .Count();
            }

            foreach (var table in tableList)
            {
                table.NumberOfChildren = columnList
                    .Where(t => t.ParentName == table.Name)
                    .Count();
            }
        }
    }
}
