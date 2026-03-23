using System.Text.Json;
using Microsoft.JSInterop;
using TodoApp2OpenCode.Models;

namespace TodoApp2OpenCode.Services
{
    public class UsersService
    {
        private readonly IJSRuntime _jsRuntime;
        private const string USERS_KEY = "flowboard_users";
        private const string CURRENT_USER_KEY = "flowboard_current_user";
        private const string SALT = "FlowBoard_Secure_Salt_2024";

        public UsersService(IJSRuntime jSRuntime)
        {
            _jsRuntime = jSRuntime;
        }

        public async Task<IEnumerable<User>> GetAll()
        {
            var usersJson = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", USERS_KEY);
            
            IEnumerable<User> users = JsonSerializer.Deserialize<IEnumerable<User>>(usersJson) ?? [];

            return users;
        }

    }
}
