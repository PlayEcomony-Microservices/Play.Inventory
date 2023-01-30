using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Play.Inventory.Service.Dtos
{
   public record GrantItemsDto(Guid userId, Guid CatalogItemId, int Quantity);

   public record InventoryItemDto(Guid CatalogItemId, int Quantity, DateTimeOffset AcquiredDate);
}