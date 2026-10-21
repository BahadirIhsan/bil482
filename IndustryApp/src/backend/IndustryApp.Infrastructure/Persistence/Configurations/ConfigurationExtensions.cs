using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IndustryApp.Infrastructure.Persistence.Configurations;

internal static class ConfigurationExtensions
{
    public const string Now = "now()";

    /// <summary>Metin olarak saklanan enum için CHECK ifadesi: "status IN ('Open','Expired')".</summary>
    public static string InCheck<TEnum>(string column)
        where TEnum : struct, Enum =>
        $"{column} IN ({string.Join(",", Enum.GetNames<TEnum>().Select(n => $"'{n}'"))})";

    /// <summary>
    /// PostgreSQL xmin sistem kolonunu eşzamanlılık belirteci yapar (T-6.1). Aynı kaydı iki işlem aynı anda
    /// değiştirirse ikincisi DbUpdateConcurrencyException → 409 alır. (Npgsql 8'de UseXminAsConcurrencyToken
    /// yerine önerilen karşılık; migration gerçek bir kolon oluşturmaz.)
    /// </summary>
    public static void UseXminConcurrencyToken<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : class =>
        builder.Property<uint>("Version").HasColumnName("xmin").HasColumnType("xid").IsRowVersion();
}
