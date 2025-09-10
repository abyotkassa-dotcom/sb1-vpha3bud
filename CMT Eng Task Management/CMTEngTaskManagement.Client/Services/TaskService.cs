using System.Net.Http.Json;
using CMTEngTaskManagement.Shared.DTOs;

namespace CMTEngTaskManagement.Client.Services
{
    public interface ITaskService
    {
        Task<IEnumerable<TaskDto>> GetTasksAsync(TaskFilterRequest filter);
        Task<TaskDto?> GetTaskAsync(int id);
        Task<TaskDto?> CreateTaskAsync(CreateTaskRequest request);
        Task<TaskDto?> UpdateTaskAsync(UpdateTaskRequest request);
        Task<bool> DeleteTaskAsync(int id);
    }

    public class TaskService : ITaskService
    {
        private readonly HttpClient _httpClient;

        public TaskService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<TaskDto>> GetTasksAsync(TaskFilterRequest filter)
        {
            try
            {
                var queryString = BuildQueryString(filter);
                var tasks = await _httpClient.GetFromJsonAsync<IEnumerable<TaskDto>>($"api/tasks?{queryString}");
                return tasks ?? Enumerable.Empty<TaskDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching tasks: {ex.Message}");
                return Enumerable.Empty<TaskDto>();
            }
        }

        public async Task<TaskDto?> GetTaskAsync(int id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<TaskDto>($"api/tasks/{id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching task {id}: {ex.Message}");
                return null;
            }
        }

        public async Task<TaskDto?> CreateTaskAsync(CreateTaskRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/tasks", request);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<TaskDto>();
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating task: {ex.Message}");
                return null;
            }
        }

        public async Task<TaskDto?> UpdateTaskAsync(UpdateTaskRequest request)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/tasks/{request.TaskId}", request);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<TaskDto>();
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating task: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> DeleteTaskAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/tasks/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting task {id}: {ex.Message}");
                return false;
            }
        }

        private string BuildQueryString(TaskFilterRequest filter)
        {
            var parameters = new List<string>();

            if (!string.IsNullOrEmpty(filter.Search))
                parameters.Add($"search={Uri.EscapeDataString(filter.Search)}");

            if (!string.IsNullOrEmpty(filter.Status))
                parameters.Add($"status={Uri.EscapeDataString(filter.Status)}");

            if (filter.ViewCompleted)
                parameters.Add("viewCompleted=true");

            if (filter.ShowDuplicates)
                parameters.Add("showDuplicates=true");

            if (!string.IsNullOrEmpty(filter.SortBy))
                parameters.Add($"sortBy={Uri.EscapeDataString(filter.SortBy)}");

            if (filter.FilterMyTasks)
                parameters.Add("filterMyTasks=true");

            return string.Join("&", parameters);
        }
    }
}