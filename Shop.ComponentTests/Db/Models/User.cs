using System;
using System.Collections.Generic;

namespace Shop.ComponentTests.Db.Models;

public partial class User
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Basket> Baskets { get; } = new List<Basket>();
}
