using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace LongPollingApi.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class UpdateController : ControllerBase
    {
        private readonly IItemService _itemService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UpdateController(IItemService itemService, IHttpContextAccessor httpContextAccessor)
        {
            _itemService = itemService;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return await Task.FromResult(Ok("Hello"));
        }

        [HttpGet]
        [Route("longpoll")]
        public async Task<IActionResult> LongPoll(CancellationToken cancellationToken)
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(30));

            var timeoutTask = Task.Delay(-1, cts.Token);
            var itemArrivedTask = _itemService.WaitForNewItem();

            var completedTask = await Task.WhenAny(itemArrivedTask, timeoutTask);
            if (completedTask == itemArrivedTask)
            {
                var item = await itemArrivedTask;
                _itemService.Reset();
                return Ok(item);
            }

            return NoContent();
        }

        [HttpGet]
        [Route("sse")]
        public async Task ServerSentEvent(CancellationToken ct)
        {
            var ctx = _httpContextAccessor.HttpContext;
            ctx.Response.Headers.Append("Content-Type", "text/event-stream");

            while (!ct.IsCancellationRequested)
            {
                var item = await _itemService.WaitForNewItem();

                await ctx.Response.WriteAsync($"data: ");
                await JsonSerializer.SerializeAsync(ctx.Response.Body, item);
                await ctx.Response.WriteAsync($"\n\n");
                await ctx.Response.Body.FlushAsync();

                _itemService.Reset();
            }
        }
    }
}