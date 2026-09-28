using ReceptionTracker.Domain.Common;

namespace ReceptionTracker.Domain.Orders;

public class Pallet
{
    private readonly List<Carton> _cartons = [];

    public int Id { get; private set; }
    public string Code { get; private set; }
    public IReadOnlyCollection<Carton> Cartons => _cartons;

    public IEnumerable<ProductLine> Products => _cartons.SelectMany(c => c.Products);
    public ReceptionStatus Status => GetProgress().Status;

    // Required by EF Core.
    private Pallet()
    {
        Code = null!;
    }

    public Pallet(string code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        Code = code;
    }

    public Carton AddCarton(string code)
    {
        if (_cartons.Any(c => c.Code == code))
        {
            throw new DomainException($"Carton '{code}' already exists on pallet '{Code}'.");
        }

        var carton = new Carton(code);
        _cartons.Add(carton);
        return carton;
    }

    /// <summary>Validating (or un-validating) a pallet applies to all its cartons and products.</summary>
    public void SetReceived(bool isReceived)
    {
        foreach (var carton in _cartons)
        {
            carton.SetReceived(isReceived);
        }
    }

    public ReceptionProgress GetProgress() => ReceptionProgress.From(Products);
}
