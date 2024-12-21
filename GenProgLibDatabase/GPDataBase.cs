namespace GenProgLibDatabase;

using System.Text.Json;
using GeneticProgrammingLib;
using Contracts;
using Microsoft.Win32.SafeHandles;

public class GPDataBase
{
    private const string DIR = "C:/Users/doodo/Desktop/GenProgData";
    private const string RUNSJSON = $"{DIR}/runs.json";
    private const string TMPRUNSJSON = $"{DIR}/tmp_runs.json";
    public static Random rnd = new Random();
    public static object used = new object();
    public static bool SaveEvolution(string id, AsyncEvolution Evolution) {
        if (!System.IO.File.Exists(RUNSJSON)) { 
            new DirectoryInfo(DIR).Create();
            System.IO.File.Create(RUNSJSON);
            System.IO.File.WriteAllText(RUNSJSON, "[]");
        }
        string state = JsonSerializer.Serialize(Evolution);
        RunInfo Experiment = new RunInfo($"{id}", $"{DIR}/{id}.json");
        List<RunInfo> Runs;
        try
        {
            Runs = JsonSerializer.Deserialize<List<RunInfo>>(System.IO.File.ReadAllText(RUNSJSON));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error occured while reading from {RUNSJSON}:\n{ex.Message}");
            throw new Exception($"Error occured while reading from {RUNSJSON}:\n{ex.Message}");
        }
        try
        {
            System.IO.File.WriteAllText(Experiment.FileName, state);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error occured while writing file:\n{ex.Message}");
            if (System.IO.File.Exists(Experiment.FileName))
            {
                System.IO.File.Delete(Experiment.FileName);
            }
            throw new Exception($"Error occured while writing file:\n{ex.Message}");
        }
        if (Runs.Contains(Experiment))
            return true;
        Runs.Add(Experiment);
        try  
        {
            System.IO.File.WriteAllText(TMPRUNSJSON, JsonSerializer.Serialize(Runs));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error occured while writing in {RUNSJSON}:\n{ex.Message}");
            if (System.IO.File.Exists(TMPRUNSJSON))
            {
                System.IO.File.Delete(TMPRUNSJSON);
            }
            System.IO.File.Delete(Experiment.FileName);
            throw new Exception($"Error occured while writing in {RUNSJSON}:\n{ex.Message}");
        }
        try 
        {
            System.IO.File.Move(TMPRUNSJSON, RUNSJSON, true);
        }
        catch (Exception ex) 
        { 
            Console.WriteLine($"Unable to rewrite {RUNSJSON}:\n{ex.Message}");
            System.IO.File.Delete(Experiment.FileName);
            throw new Exception($"Unable to rewrite {RUNSJSON}:\n{ex.Message}");
        }
        finally
        {
            System.IO.File.Delete(TMPRUNSJSON); 
        }
        Console.WriteLine("Saved.");
        return true;
    }
    public static string NewEvolution(EvolutionParameters args) {
        Console.WriteLine($"NewEvolution");
        AsyncEvolution Evolution = new AsyncEvolution(
            args.Rounds, 
            args.Players, 
            args.Courts, 
            0, 
            args.MutationRate, 
            args.CrossoverRate, 
            args.PopulationSize,
            args.PopulationSize
        );
        string id = $"{((UInt32)DateTime.Now.GetHashCode()).ToString()}{rnd.Next(0, 10000)}";
        if (!SaveEvolution(id, Evolution)) return string.Empty;
        return id;
    }
    public static EvolutionResult EvolutionStep(string id) {
        Console.WriteLine($"EvolutionStep(id={id})");
        if (!System.IO.File.Exists(RUNSJSON)) { 
            Console.WriteLine("runs.json does not exist");
            throw new Exception("runs.json does not exist");
        }
        List<RunInfo> Runs;
        try {
            Runs = JsonSerializer.Deserialize<List<RunInfo>>(System.IO.File.ReadAllText(RUNSJSON));
        }
        catch {
            Console.WriteLine("Unable to load runs.json");
            throw new Exception("Unable to load runs.json");
        }
        if (!Runs.Select(x => x.Name).Contains(id)) {
            Console.WriteLine($"Wrong id: {id}");
            throw new Exception($"Wrong id: {id}");
        }
        string file_name = Runs.Where(x => x.Name == id).First().FileName;
        string file_content;
        try
        {
            file_content = System.IO.File.ReadAllText(file_name);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error occured while reading data: {ex.Message}");
            throw new Exception($"Error occured while reading data: {ex.Message}");
        }
        AsyncEvolution Evolution;
        try
        {
            Evolution = JsonSerializer.Deserialize<AsyncEvolution>(file_content);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Could not load data: \n{ex.Message}");
            throw new Exception($"Could not load data: \n{ex.Message}");
        }
        try {
            Evolution.Step();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error occured in Evolution.Step(): \n{ex.Message}");
            throw new Exception($"Error occured in Evolution.Step(): \n{ex.Message}");
        }
        var BestTable = Evolution.BestTable();
        var BestRank = Evolution.BestRank();
        if (!SaveEvolution(id, Evolution)) throw new Exception("Internal Error");
        return new EvolutionResult(BestTable, BestRank);
    }
    public static bool DeleteEvolution(string id) {
        Console.WriteLine($"DeleteEvolution(id={id})");
        if (!System.IO.File.Exists(RUNSJSON)) { 
            Console.WriteLine("runs.json does not exist");
            throw new Exception("runs.json does not exist");
        }
        List<RunInfo> Runs;
        try {
            Runs = JsonSerializer.Deserialize<List<RunInfo>>(System.IO.File.ReadAllText(RUNSJSON));
        }
        catch {
            Console.WriteLine("Unable to load runs.json");
            throw new Exception("Unable to load runs.json");
        }
        if (!Runs.Select(x => x.Name).Contains(id)) {
            Console.WriteLine($"Wrong id : {id}");
            throw new Exception($"Wrong id : {id}");
        }
        
        string file_name = Runs.Where(x => x.Name == id).First().FileName;

        Runs.Remove(new RunInfo(id, file_name));
        try  
        {
            System.IO.File.WriteAllText(TMPRUNSJSON, JsonSerializer.Serialize(Runs));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error occured while writing in {TMPRUNSJSON}:\n{ex.Message}");
            if (System.IO.File.Exists(TMPRUNSJSON))
            {
                System.IO.File.Delete(TMPRUNSJSON);
            }
            throw new Exception($"Error occured while writing in {TMPRUNSJSON}:\n{ex.Message}");
        }
        try 
        {
            System.IO.File.Move(TMPRUNSJSON, RUNSJSON, true);
        }
        catch (Exception ex) 
        { 
            Console.WriteLine($"Unable to rewrite {RUNSJSON}:\n{ex.Message}");
            throw new Exception($"Unable to rewrite {RUNSJSON}:\n{ex.Message}");
        }
        finally
        {
            System.IO.File.Delete(TMPRUNSJSON); 
        }

        try {
            System.IO.File.Delete(file_name);
        }
        catch {
            Console.WriteLine($"Unable to delete file {file_name}");
            throw new Exception($"Unable to delete file {file_name}");
        }
        Console.WriteLine("Deleted.");
        return true;
    }
}
