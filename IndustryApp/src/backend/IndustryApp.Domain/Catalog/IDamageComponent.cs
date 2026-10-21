namespace IndustryApp.Domain.Catalog;

/// <summary>
/// Composite deseninin bileşen arayüzü: tek bir kategori ile bütün bir alt ağaç aynı şekilde ele alınır.
/// </summary>
public interface IDamageComponent
{
    int Id { get; }

    string Name { get; }

    IReadOnlyList<IDamageComponent> Children { get; }

    bool IsLeaf { get; }

    bool Contains(int categoryId);
}
