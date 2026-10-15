using IndustryApp.Domain.Common;

namespace IndustryApp.Domain.Catalog;

// [UC-02][PAT-02.2] Composite
// Niyet   : Üç seviyeli hasar ağacını (ör. Kaporta → Çamurluk → Sol ön çamurluk) tek bir arayüzle dolaşmak.
// Neden   : Sihirbaz, eşleştirme (kök kategoriler) ve admin ekranı aynı ağacı gezer; dal ve yaprak için
//           ayrı kod yazılırsa her yeni seviye her istemcide değişiklik ister.
// Kazanım : Tek kategori ile alt ağaç aynı şekilde ele alınır; yeni kategori veya seviye yalnızca veridir.
// Metrik  : Yeni kategori eklemek 0 kod değişikliği gerektirir (DamageCategory testleri, PA2/PA3 SDD).
public sealed class DamageCategory : BaseEntity<int>, IDamageComponent
{
    /// <summary>damage_categories.level CHECK kısıtıyla aynı üst sınır (T-6.4).</summary>
    public const short MaxLevel = 3;

    private readonly List<DamageCategory> _children = new();

    private DamageCategory()
    {
    }

    public int? ParentId { get; private set; }

    /// <summary>1. seviyede kendisi; eşleştirmede tek karşılaştırmayla uzmanlık kontrolü sağlar.</summary>
    public int? RootId { get; private set; }

    public string Code { get; private set; } = default!;

    public string Name { get; private set; } = default!;

    public short Level { get; private set; }

    public short SortOrder { get; private set; }

    public bool IsActive { get; private set; }

    public IReadOnlyCollection<DamageCategory> SubCategories => _children;

    IReadOnlyList<IDamageComponent> IDamageComponent.Children => _children;

    public bool IsLeaf => _children.Count == 0;

    public bool IsRoot => ParentId is null;

    /// <summary>Kök kategori kimliği; kök kategori henüz kaydedilmemişse kendi kimliği.</summary>
    public int EffectiveRootId => RootId ?? Id;

    public static DamageCategory Create(string code, string name, short sortOrder, DamageCategory? parent = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Kategori kodu boş olamaz.", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Kategori adı boş olamaz.", nameof(name));

        var level = (short)((parent?.Level ?? 0) + 1);
        if (level > MaxLevel)
            throw new DomainException(ErrorCodes.CategoryMaxDepth, $"Hasar ağacı en fazla {MaxLevel} seviyelidir.");
        if (parent is { IsActive: false })
            throw new DomainException(ErrorCodes.CategoryInactive, "Pasif kategorinin altına kategori eklenemez.");

        var category = new DamageCategory
        {
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Level = level,
            SortOrder = sortOrder,
            IsActive = true,
            ParentId = parent?.Id,
            RootId = parent?.EffectiveRootId
        };
        parent?._children.Add(category);
        return category;
    }

    /// <summary>Kendisini ve alt ağacını özyinelemeli arar.</summary>
    public bool Contains(int categoryId) => Id == categoryId || _children.Any(c => c.Contains(categoryId));

    /// <summary>Alt ağaçta kimliği verilen kategoriyi bulur; yoksa null.</summary>
    public DamageCategory? Find(int categoryId) =>
        Id == categoryId ? this : _children.Select(c => c.Find(categoryId)).FirstOrDefault(c => c is not null);

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Kategori adı boş olamaz.", nameof(name));
        Name = name.Trim();
    }

    public void ChangeSortOrder(short sortOrder) => SortOrder = sortOrder;

    public void Activate() => IsActive = true;

    /// <summary>Pasif kategori yeni raporlarda görünmez; mevcut raporlar etkilenmez (YA-01).</summary>
    public void Deactivate() => IsActive = false;
}
