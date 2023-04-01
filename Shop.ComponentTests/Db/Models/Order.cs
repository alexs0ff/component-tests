using System;
using System.Collections.Generic;

namespace Shop.ComponentTests.Db.Models;

public partial class Order
{
    public Guid Id { get; set; }

    public Guid BasketId { get; set; }

    public string Status { get; set; } = null!;

    public virtual Basket Basket { get; set; } = null!;
}
