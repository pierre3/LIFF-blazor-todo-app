using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using System.Web.Http;
using TodoBot.Server.Services;
using TodoBot.Shared;

namespace TodoBot.Server
{
    public class TodoBotFunction
    {
        private readonly ITodoRepository todoRepository;
        private readonly ILineTokenService lineTokenService;

        public TodoBotFunction(ITodoRepository todoRepository, ILineTokenService lineTokenService)
        {
            this.todoRepository = todoRepository;
            this.lineTokenService = lineTokenService;
        }

        [FunctionName("CreateTodo")]
        public async Task<IActionResult> CreateTodo(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "todoList")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation($"{nameof(CreateTodo)} method prosessing...");
            if (!await lineTokenService.VerifyTokenAsync(req.Headers[ApiServer.AccessTokenHeaderName]))
            {
                return new ForbidResult();
            }

            try
            {
                var json = await req.ReadAsStringAsync();
                var todo = JsonConvert.DeserializeObject<Todo>(json);

                if (string.IsNullOrEmpty(todo?.UserId))
                {
                    return new BadRequestObjectResult(JsonConvert.SerializeObject(new { Message = $"{nameof(todo.UserId)} is required." }));
                }

                await todoRepository.CreateTodoAsync(todo);

                return new CreatedResult("", $"{{\"id\":\"{todo.Id}\"}}");
            }
            catch (JsonSerializationException e)
            {
                return new BadRequestObjectResult(e.Message);
            }
        }

        [FunctionName("UpdateTodo")]
        public async Task<IActionResult> UpdateTodo(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "todoList/{id}")] HttpRequest req,
            string id,
            ILogger log)
        {
            log.LogInformation($"{nameof(UpdateTodo)} method prosessing...");
            if (!await lineTokenService.VerifyTokenAsync(req.Headers[ApiServer.AccessTokenHeaderName]))
            {
                return new ForbidResult();
            }

            try
            {
                var json = await req.ReadAsStringAsync();
                var todo = JsonConvert.DeserializeObject<Todo>(json);

                await todoRepository.UpdateTodoAsync(id, todo);
                return new OkResult();
            }
            catch (JsonSerializationException e)
            {
                return new BadRequestObjectResult(e.Message);
            }


        }

        [FunctionName("GetTodoList")]
        public async Task<IActionResult> GetTodoList(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "{userId}/todoList")] HttpRequest req,
            string userId,
            ILogger log)
        {
            log.LogInformation($"{nameof(GetTodoList)} method prosessing...");
            if (!await lineTokenService.VerifyTokenAsync(req.Headers[ApiServer.AccessTokenHeaderName]))
            {
                return new ForbidResult();
            }
            
            try
            {
                var todolist = await todoRepository.GetTodoListAsync(userId);
                return new OkObjectResult(todolist);
            }
            catch (JsonSerializationException e)
            {
                return new BadRequestObjectResult(e.Message);
            }
            catch (Exception e)
            {
                log.LogError(e, $"{nameof(GetTodoList)} GetTodoListAsync faild");
                return new InternalServerErrorResult();
            }

        }

        [FunctionName("GetTodo")]
        public async Task<IActionResult> GetTodo(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "{userId}/todoList/{id}")] HttpRequest req,
            string userId,
            string id,
            ILogger log)
        {
            log.LogInformation($"{nameof(GetTodo)} method prosessing...");
            if (!await lineTokenService.VerifyTokenAsync(req.Headers[ApiServer.AccessTokenHeaderName]))
            {
                return new ForbidResult();
            }

            try
            {
                var todo = await todoRepository.GetTodoAsync(userId, id);
                return new OkObjectResult(todo);
            }
            catch (JsonSerializationException e)
            {
                return new BadRequestObjectResult(e.Message);
            }
            catch (Exception e)
            {
                log.LogError(e, $"{nameof(GetTodo)} GetTodoAsync faild");
                return new InternalServerErrorResult();
            }
        }

        [FunctionName("DeleteTodo")]
        public async Task<IActionResult> DeleteTodo(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "{userId}/todoList/{id}")] HttpRequest req,
            string userId,
            string id,
            ILogger log)
        {
            log.LogInformation($"{nameof(DeleteTodo)} method prosessing...");
            if (!await lineTokenService.VerifyTokenAsync(req.Headers[ApiServer.AccessTokenHeaderName]))
            {
                return new ForbidResult();
            }

            try
            {
                await todoRepository.DeleteTodoAsync(userId, id);
                return new OkResult();
            }
            catch (JsonSerializationException e)
            {
                return new BadRequestObjectResult(e.Message);
            }
            catch (Exception e)
            {
                log.LogError(e, $"{nameof(DeleteTodo)} DeleteTodoAsync faild");
                return new InternalServerErrorResult();
            }
        }
    }
}
