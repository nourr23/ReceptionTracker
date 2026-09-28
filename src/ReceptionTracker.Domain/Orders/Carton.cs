namespace ReceptionTracker.Domain.Orders;

public class Carton
{
    private readonly List<ProductLine> _products = [];

    public int Id { get; private set; }
    public string Code { get; private set; }
    public IReadOnlyCollection<ProductLine> Products => _products;

    public ReceptionStatus Status => GetProgress().Status;

    // Required by EF Core.
    private Carton()
    {
        Code = null!;
    }

    public Carton(string code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        Code = code;
    }

    public ProductLine AddProduct(string reference, string name, string color, string size, int expectedQuantity)
    {
        var product = new ProductLine(reference, name, color, size, expectedQuantity);
        _products.Add(product);
        return product;
    }

    /// <summary>Validating (or un-validating) a carton applies to all its products.</summary>
    public void SetReceived(bool isReceived)
    {
        foreach (var product in _products)
        {
            product.SetReceived(isReceived);
        }
    }

    public ReceptionProgress GetProgress() => ReceptionProgress.From(_products);
}
