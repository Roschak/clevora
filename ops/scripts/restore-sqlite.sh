#!/usr/bin/env bash
# ==============================================================================
# Clevora - SQLite Restore Script
# ==============================================================================
set -euo pipefail

if [ "$#" -ne 1 ]; then
    echo "Usage: $0 /path/to/clevora_YYYYMMDD_HHMMSS.db.gz" >&2
    exit 1
fi

BACKUP_ARCHIVE="$1"
DB_PATH="/home/data/app.db"

if [ ! -f "${BACKUP_ARCHIVE}" ]; then
    echo "ERROR: Backup file ${BACKUP_ARCHIVE} not found!" >&2
    exit 1
fi

echo "Stopping Clevora service..."
systemctl stop clevora.service || true

if [ -f "${DB_PATH}" ]; then
    cp "${DB_PATH}" "${DB_PATH}.pre_restore_$(date +%s).bak"
fi

echo "Restoring database from ${BACKUP_ARCHIVE}..."
gunzip -c "${BACKUP_ARCHIVE}" > "${DB_PATH}"
chown www-data:www-data "${DB_PATH}"
chmod 660 "${DB_PATH}"

echo "Starting Clevora service..."
systemctl start clevora.service
echo "Database restored successfully!"
