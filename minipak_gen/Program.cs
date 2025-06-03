using System.Text.Json;

class Program
{

    static void Main(string[] args)
    {

        Console.WriteLine("Blobun Worldpak -> Minipak Converter");
        string _local_app_data = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string _worldpak_path;
        // If no worldpak was provided, use our default path
        if (args.Length == 0)
        {
            Console.WriteLine("No worldpak provided. Using default...");
            _worldpak_path = $@"{_local_app_data}\Blobun\worldpak\usermade\pak_cyansorcery_pico8wp.json";
        }
        else
        {
            //Check if the worldpak exists
            _worldpak_path = args[0];
        }

        if (!File.Exists(_worldpak_path))
        {
            Console.WriteLine("Given worldpak does not exist.");
            return;
        }

        try
        {
            MiniPak _worldpak = JsonSerializer.Deserialize<MiniPak>(File.ReadAllText(_worldpak_path)) ?? throw new Exception("Could not read worldpak file.");

			Console.WriteLine("Loaded file {0}: {0}", _worldpak_path, _worldpak.pak_name);

            //Convert the worldpak to be used on pico8
            _worldpak.ToPico8();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Could not load worldpak {_worldpak_path}.");
            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);
        }
    }
}
