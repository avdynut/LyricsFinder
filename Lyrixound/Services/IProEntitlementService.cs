using System;

namespace Lyrixound.Services;

public interface IProEntitlementService
{
    bool IsPro { get; }

    event EventHandler EntitlementChanged;
}
