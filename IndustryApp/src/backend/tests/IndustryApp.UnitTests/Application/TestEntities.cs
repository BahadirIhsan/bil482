using System.Reflection;
using System.Runtime.CompilerServices;
using IndustryApp.Domain.Accounts;
using IndustryApp.Domain.Catalog;

namespace IndustryApp.UnitTests.Application;

/// <summary>
/// Referans veri entity'leri (sözleşme, kategori kimliği) ekrandan/koddan oluşturulmaz; testte yansımayla kurulur.
/// </summary>
internal static class TestEntities
{
    public static LegalDocument LegalDocument(int id, string code, string targetRole = "All")
    {
        var doc = (LegalDocument)RuntimeHelpers.GetUninitializedObject(typeof(LegalDocument));
        Set(doc, "Id", id);
        Set(doc, nameof(IndustryApp.Domain.Accounts.LegalDocument.Code), code);
        Set(doc, nameof(IndustryApp.Domain.Accounts.LegalDocument.Version), "1.0");
        Set(doc, nameof(IndustryApp.Domain.Accounts.LegalDocument.TargetRole), targetRole);
        Set(doc, nameof(IndustryApp.Domain.Accounts.LegalDocument.Title), code);
        Set(doc, nameof(IndustryApp.Domain.Accounts.LegalDocument.Body), "-");
        Set(doc, nameof(IndustryApp.Domain.Accounts.LegalDocument.IsCurrent), true);
        return doc;
    }

    public static DamageCategory WithId(this DamageCategory category, int id)
    {
        Set(category, "Id", id);
        return category;
    }

    private static void Set(object target, string property, object value)
    {
        for (var type = target.GetType(); type is not null; type = type.BaseType)
        {
            var prop = type.GetProperty(property, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            if (prop?.SetMethod is not null)
            {
                prop.SetValue(target, value);
                return;
            }
        }

        throw new InvalidOperationException($"{target.GetType().Name}.{property} ayarlanamadı.");
    }
}
