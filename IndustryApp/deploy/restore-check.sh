#!/usr/bin/env bash
# Yedek doğrulama (SDD 15.3 adım 9): bir pg_dump yedeğini AYRI bir veritabanına (industryapp_restore_check) geri yükler
# ve tablo sayılarını yazar. Canlı veritabanına dokunmaz. Yayına almadan önce en az bir kez çalıştırılır.
#
# Kullanım: deploy/restore-check.sh /var/backups/industryapp/db-20261111-0300.dump
set -euo pipefail

DUMP="${1:?Kullanım: restore-check.sh <db-YYYYMMDD-HHMM.dump>}"
CHECK_DB=industryapp_restore_check
cd "$(dirname "$0")/.."

docker compose exec -T db dropdb -U industryapp --if-exists "$CHECK_DB"
docker compose exec -T db createdb -U industryapp "$CHECK_DB"
docker compose exec -T db pg_restore -U industryapp -d "$CHECK_DB" --no-owner < "$DUMP"

docker compose exec -T db psql -U industryapp -d "$CHECK_DB" -c "
  SELECT 'users' AS tablo, count(*) FROM users
  UNION ALL SELECT 'accident_reports', count(*) FROM accident_reports
  UNION ALL SELECT 'offers', count(*) FROM offers
  UNION ALL SELECT 'report_photos', count(*) FROM report_photos;"

echo "Geri yükleme başarılı. Kontrol veritabanını silmek için: docker compose exec -T db dropdb -U industryapp $CHECK_DB"
