#!/usr/bin/env bash
# ==============================================================================
# Clevora - Health Probe Cron Script
# ==============================================================================
set -euo pipefail

ENDPOINT="http://127.0.0.1:5000/health/ready"
STATUS_CODE=$(curl -s -o /dev/null -w "%{http_code}" --max-time 5 "${ENDPOINT}" || echo "000")

if [ "${STATUS_CODE}" != "200" ]; then
    echo "[$(date)] ALERT: Clevora health check failed with status code ${STATUS_CODE}! Restarting service..." >&2
    systemctl restart clevora.service
fi
