using System;
using System.Collections.Generic;

namespace Shop.ComponentTests.Db.Models;

public partial class Basket
{
    public Guid Id { get; set; }

    public string PromoCode { get; set; } = null!;

    public Guid Userid { get; set; }

    public decimal Total { get; set; }

    public Guid? OrderId { get; set; }

    public virtual Order? Order { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; } = new List<OrderItem>();

    public virtual User User { get; set; } = null!;
}
