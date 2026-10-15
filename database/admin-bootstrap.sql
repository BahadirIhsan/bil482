-- =============================================================================
-- IndustryApp – ilk admin (T-6.9)
-- Admin hesabı herkese açık kayıtla oluşturulamaz. Önce uygulamadan normal müşteri olarak kaydolunur,
-- sonra aşağıdaki e-posta değiştirilerek bu betik BİR KEZ çalıştırılır. Şifre Identity'nin kendi
-- özetiyle oluştuğu için SQL'de şifre yazılmaz.
--
-- Çalıştırma: docker compose exec -T db psql -U industryapp -d industryapp < database/admin-bootstrap.sql
-- Kullanıcı bir sonraki girişte yeni JWT aldığında Admin rolüyle yönetim ekranlarını görür.
-- =============================================================================
INSERT INTO user_roles (user_id, role_id)
SELECT u.id, r.id
FROM users u
JOIN roles r ON r.normalized_name = 'ADMIN'
WHERE u.normalized_email = UPPER('admin@ornek.com')
ON CONFLICT DO NOTHING;
