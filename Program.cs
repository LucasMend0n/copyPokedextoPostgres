using System.Net.Http.Headers;
using Npgsql;
using testeArquivo;
using Newtonsoft.Json;

public class Program

{


    static async Task getPokemonsfromAPI()
    {
        string apiUrl = "https://pokeapi.co/api/v2/pokemon?limit=1281";

        using (HttpClient httpClient = new HttpClient())
        {
            HttpResponseMessage resp = await httpClient.GetAsync(apiUrl);
            if (resp.IsSuccessStatusCode)
            {
                string content = await resp.Content.ReadAsStringAsync();
                var responseApi = JsonConvert.DeserializeObject<JsonPokemon>(content);

                var pokemonsFromAPI = responseApi.Results;

                if (pokemonsFromAPI != null)
                {
                    using (var connection = new NpgsqlConnection("INSANE SQL STRING CONNECTION"))
                    {
                        await connection.OpenAsync();

                        foreach (var pokemon in pokemonsFromAPI)
                        {
                            string selectQuery = "SELECT COUNT(*) FROM tb_pokemons WHERE name = @name;";
                            int count;

                            using (var selectCommand = new NpgsqlCommand(selectQuery, connection))
                            {
                                selectCommand.Parameters.AddWithValue("name", pokemon.Name);
                                count = Convert.ToInt32(await selectCommand.ExecuteScalarAsync());
                            }
                            if (count == 0)
                            {
                                var mappedPokemon = new Pokemon { Name = pokemon.Name };
                                string insertQuery = "INSERT INTO tb_pokemons (name) VALUES (@name);";

                                using (var insertCommand = new NpgsqlCommand(insertQuery, connection))
                                {
                                    insertCommand.Parameters.AddWithValue("name", pokemon.Name);
                                    await insertCommand.ExecuteNonQueryAsync();
                                }
                            }
                        }
                        Console.WriteLine("Pokémons salvos no banco de dados com sucesso!");
                    }
                }
            }
            else
            {
                Console.WriteLine($"Erro na requisição. Status Code: {resp.StatusCode}, Mensagem: {resp.ReasonPhrase}");
            }
        }
    }

    static void Main()
    {
        try
        {
            var task = getPokemonsfromAPI();
            task.Wait();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ocorreu um erro: {ex.Message}");
        }
    }
}