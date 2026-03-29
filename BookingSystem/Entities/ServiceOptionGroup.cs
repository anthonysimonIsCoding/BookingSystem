using System;
using System.Collections.Generic;
using BookingSystem.Entities.Enums;

namespace BookingSystem.Entities;

public class ServiceOptionGroup
{
    public Guid Id { get; set; }

    public Guid ServiceId { get; set; }

    public string Name { get; set; } = null!; // vd: "Kích thước", "Dịch vụ thêm"

    public OptionGroupType Type { get; set; }

    public bool IsRequired { get; set; } = true;

    public Service Service { get; set; } = null!;

    public ICollection<ServiceOption> Options { get; set; } = new List<ServiceOption>();
}