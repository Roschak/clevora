#!/usr/bin/env bash
# ==============================================================================
# Clevora - SQLite Safe Backup Script (WAL-safe)
# ==============================================================================
set -euo pipefail

DB_PATH="/home/data/app.db"
BACKUP_DIR="/home/data/backups"
TIMESTAMP=$(date +"%Y%m%d_%H%M%S")
TARGET_FILE="${BACKUP_DIR}/clevora_${TIMESTAMP}.db.gz"

mkdir -p "${BACKUP_DIR}"

if [ ! -f "${DB_PATH}" ]; then
    echo "ERROR: Database file ${DB_PATH} not found!" >&2
    exit 1
fi

echo "[$(date)] Starting WAL-safe SQLite backup..."

# Use sqlite3 online backup API to safely copy database during active writes
TEMP_BAK="/tmp/clevora_backup_${TIMESTAMP}.db"
sqlite3 "${DB_PATH}" ".backup '${TEMP_BAK}'"

# Compress backup
gzip -c "${TEMP_BAK}" > "${TARGET_FILE}"
rm -f "${TEMP_BAK}"

echo "[$(date)] Backup completed: ${TARGET_FILE} ($(du -h "${TARGET_FILE}" | cut -f1))"

# Retention: retain last 14 days of backups
find "${BACKUP_DIR}" -type f -name "clevora_*.db.gz" -mtime +14 -delete
echo "[$(date)] Cleaned old backups older than 14 days."
