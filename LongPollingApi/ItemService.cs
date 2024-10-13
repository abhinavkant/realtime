
namespace LongPollingApi
{
    public class Item
    {
        public required int Id { get; set; }
        public required string Data { get; set; }
    }

    public interface IItemService
    {
        bool AnyNewItems();

        Item GetNewItem();

        void Reset();

        public void NotifyNewItemAvailable();

        public Task<Item?> WaitForNewItem();
    }

    public class ItemService : IItemService
    {
        public bool AnyNewItems() => Random.Shared.Next(0, 100) == 1;
        public Item GetNewItem() => new Item
        {
            Id = Random.Shared.Next(0, 100),
            Data = Random.Shared.Next(0, 500).ToString()

        };

        private TaskCompletionSource<Item?> _tcs = new();

        public void Reset()
        {
            _tcs = new TaskCompletionSource<Item?>();
        }

        public void NotifyNewItemAvailable()
        {
            _tcs.TrySetResult(new Item
            {
                Id = Random.Shared.Next(0, 100),
                Data = Random.Shared.Next(0, 500).ToString()

            });
        }

        public Task<Item?> WaitForNewItem()
        {
            // Simulate some delay in Item arrival
            Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(Random.Shared.Next(0, 29)));
                NotifyNewItemAvailable();
            });

            return _tcs.Task;
        }
    }


}