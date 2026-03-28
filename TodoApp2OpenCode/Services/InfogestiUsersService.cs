using System.Security.Authentication;
using System.Text;
using System.Text.Json;
using TodoApp2OpenCode.Models;



public class InfogestiUsersService
{
    private readonly HttpClient _httpClient;

    public InfogestiUsersService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("external");
    }


    public async Task<UsuarioDTO?> GetUserAccount(string username, string password)
    {

        var authRequest = new
        {
            usuario = username,
            contraseña = password
        };

        HttpResponseMessage response;
        UsuarioDTO? remoteUser;

        var jsonContent = JsonSerializer.Serialize(authRequest);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        response = await _httpClient.PostAsync("autenticar", content);
        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
            {
                Console.WriteLine($"Error with HTTP Code {response.StatusCode}");
            }
            Console.WriteLine($"Error with HTTP Code {response.StatusCode}");
            return null;
        }
        var responseContent = await response.Content.ReadAsStringAsync();
        remoteUser = JsonSerializer.Deserialize<UsuarioDTO>(responseContent);

        if (remoteUser == null)
        {
            Console.WriteLine("Error when deserializing user json, content: " + responseContent);
        }
        return remoteUser;
    }
}