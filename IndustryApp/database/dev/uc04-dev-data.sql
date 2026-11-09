-- =====================================================================================
-- UC-04 yerel test verisi (rehber 2.6) — YALNIZCA GELİŞTİRME VERİTABANI
-- UC-02 (rapor sihirbazı) ve UC-03 (teklif formu) henüz olmadığı için UC-04'ü denemek üzere
-- bir Open rapor, eşleşmeler ve her ustadan bir Pending teklif ekler.
-- Üretimde ASLA çalıştırılmaz; Faz 3 sonunda silinir.
--
-- Ön koşul: kullanıcılar uygulama ekranından kaydolmuş olmalı (seed yoktur, K-07):
--   müşteri : musteri@test.local
--   ustalar : usta1@test.local, usta2@test.local, usta3@test.local  (USTA%@TEST.LOCAL)
--
-- Çalıştırma (repo kökünden):
--   docker compose exec -T db psql -U industryapp -d industryapp < database/dev/uc04-dev-data.sql
--
-- Her çalıştırma YENİ bir rapor açar; seçim, geri alma ve iptal senaryoları tekrar tekrar denenebilir.
-- =====================================================================================

-- 1) Ustaları onayla (admin ekranı YA-01 gelene kadar). Ustalar çıkış yapıp tekrar girince
--    token'larında mechanic_status = Approved olur.
UPDATE mechanic_profiles
SET approval_status = 'Approved', approved_at = now()
WHERE user_id IN (SELECT id FROM users WHERE normalized_email LIKE 'USTA%@TEST.LOCAL')
  AND approval_status <> 'Approved';

-- 2) Rapor + hasar kalemi + eşleşmeler + teklifler
DO $$
DECLARE
    c   uuid     := (SELECT id FROM users WHERE normalized_email = 'MUSTERI@TEST.LOCAL');
    vt  smallint := (SELECT id FROM vehicle_types WHERE code = 'CAR');
    cat integer  := (SELECT id FROM damage_categories WHERE code = 'BODY_FRONT_BUMPER');
    r   uuid     := gen_random_uuid();
    mechanic_count integer;
BEGIN
    IF c IS NULL THEN
        RAISE EXCEPTION 'musteri@test.local bulunamadı. Önce uygulamadan müşteri olarak kaydolun.';
    END IF;
    IF NOT EXISTS (SELECT 1 FROM customer_profiles WHERE user_id = c) THEN
        RAISE EXCEPTION 'musteri@test.local için müşteri profili yok (kullanıcı müşteri olarak kaydolmamış).';
    END IF;

    SELECT count(*) INTO mechanic_count
    FROM users u JOIN mechanic_profiles m ON m.user_id = u.id
    WHERE u.normalized_email LIKE 'USTA%@TEST.LOCAL';
    IF mechanic_count = 0 THEN
        RAISE EXCEPTION 'usta%%@test.local ile kayıtlı usta yok. Önce uygulamadan usta olarak kaydolun.';
    END IF;

    INSERT INTO accident_reports (id, customer_id, vehicle_type_id, description, city, district, latitude, longitude, status, expires_at)
    VALUES (r, c, vt, 'UC-04 yerel test raporu', 'Ankara', 'Çankaya', 39.92, 32.85, 'Open', now() + interval '72 hours');

    INSERT INTO report_damage_items (report_id, damage_category_id, severity)
    VALUES (r, cat, 'Medium');

    INSERT INTO report_matches (report_id, mechanic_id, strategy_code)
    SELECT r, u.id, 'CITY'
    FROM users u JOIN mechanic_profiles m ON m.user_id = u.id
    WHERE u.normalized_email LIKE 'USTA%@TEST.LOCAL';

    -- Fiyat 4.000 / 4.500 / 5.000 ₺, süre 4 / 3 / 2 gün, oluşturulma zamanı farklı:
    -- böylece üç sıralama (fiyat, en hızlı, en yeni) birbirinden farklı sonuç verir.
    WITH ranked AS (
        SELECT u.id AS mechanic_id, row_number() OVER (ORDER BY u.email) AS n
        FROM users u JOIN mechanic_profiles m ON m.user_id = u.id
        WHERE u.normalized_email LIKE 'USTA%@TEST.LOCAL'
    )
    INSERT INTO offers (id, report_id, mechanic_id, total_price, estimated_days, status, created_at)
    SELECT gen_random_uuid(), r, mechanic_id,
           3500 + 500 * n,
           GREATEST(5 - n, 1),
           'Pending',
           now() - (n * interval '7 minutes')
    FROM ranked;

    -- Fiyat kırılımı: işçilik + parça (toplam = total_price)
    INSERT INTO offer_price_items (offer_id, item_type, description, amount, sort_order)
    SELECT id, 'Labor', 'İşçilik', total_price - 1500, 0 FROM offers WHERE report_id = r
    UNION ALL
    SELECT id, 'Parts', 'Ön tampon', 1500, 1 FROM offers WHERE report_id = r;

    RAISE NOTICE 'Rapor oluşturuldu: % (müşteri ekranı: /customer/reports/%)', r, r;
END $$;
