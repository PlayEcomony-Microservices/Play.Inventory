using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Play.Common;
using Play.Inventory.Contracts;
using Play.Inventory.Service.Entities;
using Play.Inventory.Service.Exceptions;

namespace Play.Inventory.Service.Consumers
{
    public class GrantItemsConsumer : IConsumer<GrantItems>
    {
        private readonly IRepository<InventoryItem> _inventoryItemsRepository;
        private readonly IRepository<CatalogItem> _catalogItemsRepository;

        public GrantItemsConsumer(IRepository<InventoryItem> inventoryItemsRepository, IRepository<CatalogItem> catalogItemsRepository)
        {
            _inventoryItemsRepository = inventoryItemsRepository;
            _catalogItemsRepository = catalogItemsRepository;
        }
        public async Task Consume(ConsumeContext<GrantItems> context)
        {
            var message = context.Message;

            var item = await _catalogItemsRepository.GetAsync(message.CatalogItemId);

            if(item is null) throw new UnknownItemException(message.CatalogItemId);
            
            var inventoryItem = await _inventoryItemsRepository.GetAsync(item => item.CatalogItemId == message.CatalogItemId && item.UserId == message.UserId);

            if (inventoryItem is null)
            {
                inventoryItem = new InventoryItem
                {
                    CatalogItemId = message.CatalogItemId,
                    UserId = message.UserId,
                    Quantity = message.Quantity,
                    AcquiredDate = DateTimeOffset.UtcNow
                };


                await _inventoryItemsRepository.CreateAsync(inventoryItem);
            }
            else
            {
                if(inventoryItem.MessageIds.Contains(context.MessageId.Value))
                {
                    await context.Publish(new InventoryItemsGranted(message.CorrelationId));
                    return;
                }

                inventoryItem.Quantity += message.Quantity;
                inventoryItem.MessageIds.Add(context.MessageId.Value);

                await _inventoryItemsRepository.UpdateAsync(inventoryItem);
            }

            await context.Publish(new InventoryItemsGranted(message.CorrelationId));
        }
    }
}