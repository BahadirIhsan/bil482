-- =============================================================================
-- IndustryApp – referans veri (T-6.8)
-- Roller, araç türleri, 3 seviyeli hasar kategori ağacı ve sözleşmeler.
-- Tekrar çalıştırılabilir (ON CONFLICT DO NOTHING). İlk migration'dan hemen sonra bir kez çalıştırılır;
-- yeni kategori gerektiğinde satırlar dosyanın sonuna eklenip betik yeniden çalıştırılır.
-- İşlem verisi (kullanıcı, rapor, teklif, bildirim) burada YOKTUR; yalnızca ekranlardan oluşur (K-07).
--
-- Çalıştırma: docker compose exec -T db psql -U industryapp -d industryapp < database/reference-data.sql
-- =============================================================================
BEGIN;

-- Roller (Identity normalize edilmiş adları büyük harfle arar)
INSERT INTO roles (id, name, normalized_name, concurrency_stamp) VALUES
    (gen_random_uuid(), 'Customer', 'CUSTOMER', gen_random_uuid()::text),
    (gen_random_uuid(), 'Mechanic', 'MECHANIC', gen_random_uuid()::text),
    (gen_random_uuid(), 'Admin',    'ADMIN',    gen_random_uuid()::text)
ON CONFLICT (normalized_name) DO NOTHING;

-- Araç türleri
INSERT INTO vehicle_types (code, name, sort_order) VALUES
    ('CAR', 'Otomobil', 1),
    ('SUV', 'SUV / Arazi', 2),
    ('LCV', 'Hafif ticari', 3),
    ('MOTORCYCLE', 'Motosiklet', 4),
    ('MINIBUS', 'Minibüs / Panelvan', 5)
ON CONFLICT (code) DO NOTHING;

-- -----------------------------------------------------------------------------
-- Hasar kategorileri – 1. seviye (uzmanlıklar bu seviyeye bağlanır; root_id kendisine eşitlenir)
-- -----------------------------------------------------------------------------
INSERT INTO damage_categories (code, name, level, sort_order) VALUES
    ('BODY', 'Kaporta', 1, 1),
    ('PAINT', 'Boya', 1, 2),
    ('MECHANICAL', 'Mekanik', 1, 3),
    ('ELECTRICAL', 'Elektrik', 1, 4),
    ('GLASS', 'Cam', 1, 5),
    ('WHEEL', 'Lastik ve jant', 1, 6)
ON CONFLICT (code) DO NOTHING;

UPDATE damage_categories SET root_id = id WHERE level = 1 AND root_id IS NULL;

-- -----------------------------------------------------------------------------
-- 2. seviye
-- -----------------------------------------------------------------------------
-- Kaporta
INSERT INTO damage_categories (code, name, level, sort_order, parent_id, root_id)
SELECT v.code, v.name, 2, v.sort_order, p.id, p.id
FROM (VALUES ('BODY_FRONT_BUMPER', 'Ön tampon', 1), ('BODY_REAR_BUMPER', 'Arka tampon', 2),
             ('BODY_HOOD', 'Kaput', 3), ('BODY_FENDER', 'Çamurluk', 4), ('BODY_DOOR', 'Kapı', 5),
             ('BODY_TRUNK', 'Bagaj kapağı', 6), ('BODY_ROOF', 'Tavan', 7))
     AS v(code, name, sort_order)
JOIN damage_categories p ON p.code = 'BODY'
ON CONFLICT (code) DO NOTHING;

-- Boya
INSERT INTO damage_categories (code, name, level, sort_order, parent_id, root_id)
SELECT v.code, v.name, 2, v.sort_order, p.id, p.id
FROM (VALUES ('PAINT_PARTIAL', 'Lokal boya', 1), ('PAINT_PANEL', 'Parça boya', 2),
             ('PAINT_FULL', 'Komple boya', 3), ('PAINT_SCRATCH', 'Çizik ve rötuş', 4),
             ('PAINT_POLISH', 'Pasta cila', 5))
     AS v(code, name, sort_order)
JOIN damage_categories p ON p.code = 'PAINT'
ON CONFLICT (code) DO NOTHING;

-- Mekanik
INSERT INTO damage_categories (code, name, level, sort_order, parent_id, root_id)
SELECT v.code, v.name, 2, v.sort_order, p.id, p.id
FROM (VALUES ('MECH_ENGINE', 'Motor', 1), ('MECH_SUSPENSION', 'Süspansiyon ve alt takım', 2),
             ('MECH_BRAKE', 'Fren sistemi', 3), ('MECH_STEERING', 'Direksiyon sistemi', 4),
             ('MECH_COOLING', 'Soğutma ve radyatör', 5), ('MECH_EXHAUST', 'Egzoz sistemi', 6),
             ('MECH_TRANSMISSION', 'Şanzıman ve debriyaj', 7))
     AS v(code, name, sort_order)
JOIN damage_categories p ON p.code = 'MECHANICAL'
ON CONFLICT (code) DO NOTHING;

-- Elektrik
INSERT INTO damage_categories (code, name, level, sort_order, parent_id, root_id)
SELECT v.code, v.name, 2, v.sort_order, p.id, p.id
FROM (VALUES ('ELEC_HEADLIGHT', 'Far', 1), ('ELEC_TAILLIGHT', 'Stop lambası', 2),
             ('ELEC_SENSOR', 'Park sensörü ve kamera', 3), ('ELEC_AIRBAG', 'Hava yastığı sistemi', 4),
             ('ELEC_WIRING', 'Kablo tesisatı', 5), ('ELEC_MIRROR', 'Elektrikli ayna', 6))
     AS v(code, name, sort_order)
JOIN damage_categories p ON p.code = 'ELECTRICAL'
ON CONFLICT (code) DO NOTHING;

-- Cam
INSERT INTO damage_categories (code, name, level, sort_order, parent_id, root_id)
SELECT v.code, v.name, 2, v.sort_order, p.id, p.id
FROM (VALUES ('GLASS_WINDSHIELD', 'Ön cam', 1), ('GLASS_REAR', 'Arka cam', 2),
             ('GLASS_SIDE', 'Yan cam', 3), ('GLASS_SUNROOF', 'Sunroof', 4), ('GLASS_MIRROR', 'Ayna camı', 5))
     AS v(code, name, sort_order)
JOIN damage_categories p ON p.code = 'GLASS'
ON CONFLICT (code) DO NOTHING;

-- Lastik ve jant
INSERT INTO damage_categories (code, name, level, sort_order, parent_id, root_id)
SELECT v.code, v.name, 2, v.sort_order, p.id, p.id
FROM (VALUES ('WHEEL_TIRE', 'Lastik', 1), ('WHEEL_RIM', 'Jant', 2), ('WHEEL_ALIGNMENT', 'Rot ve balans', 3))
     AS v(code, name, sort_order)
JOIN damage_categories p ON p.code = 'WHEEL'
ON CONFLICT (code) DO NOTHING;

-- -----------------------------------------------------------------------------
-- 3. seviye (root_id üst kategoriden devralınır)
-- -----------------------------------------------------------------------------
INSERT INTO damage_categories (code, name, level, sort_order, parent_id, root_id)
SELECT v.code, v.name, 3, v.sort_order, p.id, p.root_id
FROM (VALUES ('BODY_FENDER', 'BODY_FENDER_FL', 'Sol ön çamurluk', 1),
             ('BODY_FENDER', 'BODY_FENDER_FR', 'Sağ ön çamurluk', 2),
             ('BODY_FENDER', 'BODY_FENDER_RL', 'Sol arka çamurluk', 3),
             ('BODY_FENDER', 'BODY_FENDER_RR', 'Sağ arka çamurluk', 4),
             ('BODY_DOOR', 'BODY_DOOR_FL', 'Sol ön kapı', 1),
             ('BODY_DOOR', 'BODY_DOOR_FR', 'Sağ ön kapı', 2),
             ('BODY_DOOR', 'BODY_DOOR_RL', 'Sol arka kapı', 3),
             ('BODY_DOOR', 'BODY_DOOR_RR', 'Sağ arka kapı', 4),
             ('ELEC_HEADLIGHT', 'ELEC_HEADLIGHT_L', 'Sol far', 1),
             ('ELEC_HEADLIGHT', 'ELEC_HEADLIGHT_R', 'Sağ far', 2),
             ('ELEC_TAILLIGHT', 'ELEC_TAILLIGHT_L', 'Sol stop lambası', 1),
             ('ELEC_TAILLIGHT', 'ELEC_TAILLIGHT_R', 'Sağ stop lambası', 2),
             ('GLASS_SIDE', 'GLASS_SIDE_FL', 'Sol ön yan cam', 1),
             ('GLASS_SIDE', 'GLASS_SIDE_FR', 'Sağ ön yan cam', 2),
             ('GLASS_SIDE', 'GLASS_SIDE_RL', 'Sol arka yan cam', 3),
             ('GLASS_SIDE', 'GLASS_SIDE_RR', 'Sağ arka yan cam', 4),
             ('WHEEL_TIRE', 'WHEEL_TIRE_FL', 'Sol ön lastik', 1),
             ('WHEEL_TIRE', 'WHEEL_TIRE_FR', 'Sağ ön lastik', 2),
             ('WHEEL_TIRE', 'WHEEL_TIRE_RL', 'Sol arka lastik', 3),
             ('WHEEL_TIRE', 'WHEEL_TIRE_RR', 'Sağ arka lastik', 4),
             ('WHEEL_RIM', 'WHEEL_RIM_FL', 'Sol ön jant', 1),
             ('WHEEL_RIM', 'WHEEL_RIM_FR', 'Sağ ön jant', 2),
             ('WHEEL_RIM', 'WHEEL_RIM_RL', 'Sol arka jant', 3),
             ('WHEEL_RIM', 'WHEEL_RIM_RR', 'Sağ arka jant', 4))
     AS v(parent_code, code, name, sort_order)
JOIN damage_categories p ON p.code = v.parent_code
ON CONFLICT (code) DO NOTHING;

-- -----------------------------------------------------------------------------
-- Sözleşmeler (body metinleri hukuki metin hazır olunca UPDATE ile güncellenir; T-15 yayın öncesi kontrol listesi)
-- -----------------------------------------------------------------------------
INSERT INTO legal_documents (code, version, target_role, title, body) VALUES
    ('TERMS_OF_USE', '1.0', 'All', 'Kullanım Koşulları',
     'Bu metin hukuki onaydan sonra güncellenecektir.'),
    ('CUSTOMER_KVKK_NOTICE', '1.0', 'Customer', 'KVKK Aydınlatma Metni',
     'Bu metin hukuki onaydan sonra güncellenecektir.'),
    ('MECHANIC_SERVICE_AGREEMENT', '1.0', 'Mechanic', 'Hizmet Sağlayıcı Sözleşmesi',
     'Bu metin hukuki onaydan sonra güncellenecektir.')
ON CONFLICT (code, version) DO NOTHING;

COMMIT;
