#!/usr/bin/env bash
# Gece yedeği (SDD 15.3 adım 9): veritabanı (pg_dump, custom format) ve fotoğraf birimi (tar.gz); 7 günden eskiler silinir.
#
# Kurulum (sunucuda, her gece 03:00):
#   chmod +x deploy/backup.sh
#   crontab -e  →  0 3 * * * /opt/industryapp/IndustryApp/deploy/backup.sh >> /var/log/industryapp-backup.log 2>&1
#
# Ayarlar: BACKUP_DIR (varsayılan /var/backups/industryapp), KEEP_DAYS (varsayılan 7).
set -euo pipefail

cd "$(dirname "$0")/.."
BACKUP_DIR="${BACKUP_DIR:-/var/backups/industryapp}"
KEEP_DAYS="${KEEP_DAYS:-7}"
STAMP="$(date +%Y%m%d-%H%M)"
mkdir -p "$BACKUP_DIR"

echo "[$(date '+%Y-%m-%dT%H:%M:%S%z')] yedek başlıyor → $BACKUP_DIR"

docker compose exec -T db pg_dump -U industryapp -d industryapp -Fc > "$BACKUP_DIR/db-$STAMP.dump.partial"
mv "$BACKUP_DIR/db-$STAMP.dump.partial" "$BACKUP_DIR/db-$STAMP.dump"

API_CONTAINER="$(docker compose ps -q api)"
if [ -z "$API_CONTAINER" ]; then
  echo "api konteyneri çalışmıyor; fotoğraf yedeği alınamadı" >&2
  exit 1
fi
docker run --rm --volumes-from "$API_CONTAINER":ro -v "$BACKUP_DIR":/backup alpine:3 \
  tar czf "/backup/uploads-$STAMP.tar.gz" -C /data/uploads .

find "$BACKUP_DIR" -maxdepth 1 \( -name 'db-*.dump' -o -name 'uploads-*.tar.gz' \) -mtime +"$KEEP_DAYS" -delete

echo "[$(date '+%Y-%m-%dT%H:%M:%S%z')] yedek tamam: db-$STAMP.dump, uploads-$STAMP.tar.gz"
