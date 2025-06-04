using System.Text.Json;
using System.CommandLine;
class Program
{

    static void Main(string[] args)
    {
        Console.WriteLine("Blobun Worldpak -> Minipak Converter");

        var worldpakPath = new Option<string?>(
            name: "--input",
            description: "The filepath to the worldpak to convert.") { IsRequired = true };
        worldpakPath.AddAlias("-i");
        
        var levelOutput = new Option<string?>(
            name: "--output",
            description: "The filepath to where to place the converted level file.") { IsRequired = true };
        levelOutput.AddAlias("-o");

        var mapOutput = new Option<string?>(
            name: "--map",
            description: "If given, produce a pico8 cartridge with a map to use as a lookup table.");
        mapOutput.AddAlias("-m");

        var rootCommand = new RootCommand("Simple program for converting Blobun worldpaks to be usable by Blobun Mini.");
        rootCommand.AddOption(worldpakPath);
        rootCommand.AddOption(levelOutput);
        rootCommand.AddOption(mapOutput);

        rootCommand.SetHandler((input, output, map) => 
            { 
                ParseWorldpak(input!, output!, map!); 
            },
            worldpakPath, levelOutput, mapOutput);

        rootCommand.Invoke(args);


        
    }

    static void ParseWorldpak(string worldpakPath, string levelOutput, string? mapOutput)
    {
        if (!File.Exists(worldpakPath))
        {
            Console.WriteLine("Given worldpak does not exist.");
            return;
        }

        try
        {
            MiniPak _worldpak = JsonSerializer.Deserialize<MiniPak>(File.ReadAllText(worldpakPath)) ?? throw new Exception("Could not read worldpak file.");

            Console.WriteLine("Loaded file {0}: {1}", worldpakPath, _worldpak.pak_name);

            //Convert the worldpak to be used on pico8
            _worldpak.ToPico8(levelOutput, mapOutput);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Could not load worldpak {worldpakPath}.");
            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);
        }
    }
}
