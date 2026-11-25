#!/usr/bin/env bash
# ==============================================================================
# Clevora - Deployment Script
# ==============================================================================
set -euo pipefail

DEPLOY_DIR="/var/www/clevora"
REPO_DIR="/home/ubuntu/clevora"

echo "=== Deploying Clevora (.NET 10) ==="

# 1. Mount point validation
if [ ! -d "/home/data" ]; then
    echo "FATAL: /home/data volume not mounted! Deployment aborted." >&2
    exit 1
fi

# 2. Build & Publish
cd "${REPO_DIR}"
git pull origin main
dotnet publish src/Clevora.Web/Clevora.Web.csproj -c Release -o /tmp/clevora_publish

# 3. Stop, update files, and restart
sudo systemctl stop clevora.service
sudo rsync -av --delete /tmp/clevora_publish/ "${DEPLOY_DIR}/"
sudo chown -R www-data:www-data "${DEPLOY_DIR}"
sudo systemctl start clevora.service

# 4. Probe Health
sleep 3
HEALTH_STATUS=$(curl -s -o /dev/null -w "%{http_code}" http://127.0.0.1:5000/health/ready || true)
if [ "${HEALTH_STATUS}" == "200" ]; then
    echo "=== Deployment Successful! Application is Healthy. ==="
else
    echo "WARNING: Health check returned status ${HEALTH_STATUS}." >&2
fi
