using System;
using System.Collections.Generic;

namespace Shop.ComponentTests.Db.Models;

public partial class OrderItem
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public int Count { get; set; }

    public decimal Price { get; set; }

    public Guid BasketId { get; set; }

    public virtual Basket Basket { get; set; } = null!;
}
