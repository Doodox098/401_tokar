namespace APITest;

using WEBAPI;
using Contracts;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Text;
using System.Net.Http.Json;

public class GeneticLibControllerTest : IClassFixture<WebApplicationFactory<API.Startup>>
{
    private readonly WebApplicationFactory<API.Startup> factory;
    public GeneticLibControllerTest(WebApplicationFactory<API.Startup> factory) {
        this.factory = factory;
    }
    [Fact]
    public async void PutTest()
    {
        var client = factory.CreateClient();

        EvolutionParameters args = new EvolutionParameters ();
        args.Rounds = 10;
        args.Players = 10;
        args.Courts = 10;
        args.MutationRate = 0.5;
        args.CrossoverRate = 0.8;
        args.PopulationSize = 100;

        var content = new StringContent(
            JsonSerializer.Serialize(args), 
            Encoding.UTF8, 
            "application/json");

        var response = await client.PutAsync("experiments", content);
        response.EnsureSuccessStatusCode();
    }
    [Fact]
    public async void PostDeleteTest()
    {
        var client = factory.CreateClient();

        EvolutionParameters args = new EvolutionParameters ();
        args.Rounds = 10;
        args.Players = 10;
        args.Courts = 10;
        args.MutationRate = 0.5;
        args.CrossoverRate = 0.8;
        args.PopulationSize = 100;
        
        var content = new StringContent(
            JsonSerializer.Serialize(args), 
            Encoding.UTF8, 
            "application/json");

        var response = await client.PutAsync("experiments", content);
        var id = await response.Content.ReadAsStringAsync();

        content = new StringContent(
            JsonSerializer.Serialize(id), 
            Encoding.UTF8, 
            "application/json");

        response = await client.PostAsync($"experiments/{id}", content);
        var result = await response.Content.ReadFromJsonAsync<EvolutionResult>();
        Assert.Equal(result.Table.Length, args.Rounds);
        foreach (var court in result.Table) {
            SortedSet<int> Players = new SortedSet<int>();
            foreach (var pair in court) {
                Players.UnionWith(pair.Value);
            }
            Assert.Equal(Players.Count, args.Players);
        }
        response.EnsureSuccessStatusCode();

        response = await client.DeleteAsync($"experiments/{id}");

        Assert.False(System.IO.File.Exists($"C:/Users/doodo/Desktop/GenProgData/{id}.json"));
        
        response = await client.PostAsync($"experiments/{id}", content);
        Assert.False(response.IsSuccessStatusCode);
    }
}