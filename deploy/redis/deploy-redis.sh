#!/usr/bin/env bash
set -euo pipefail

IMAGE="redis:7-alpine"
CONTAINER_NAME="jobsprovider-redis"
VOLUME_NAME="jobsprovider-redis-data"
HOST_PORT="${HOST_PORT:-6379}"
REDIS_PASSWORD="${REDIS_PASSWORD:?REDIS_PASSWORD must be set}"

echo "Pulling $IMAGE"
docker pull "$IMAGE"

docker rm -f "$CONTAINER_NAME" >/dev/null 2>&1 || true

docker volume create "$VOLUME_NAME" >/dev/null

docker run -d \
    --name "$CONTAINER_NAME" \
    --restart unless-stopped \
    -p "${HOST_PORT}:6379" \
    -v "${VOLUME_NAME}:/data" \
    "$IMAGE" \
    redis-server --appendonly yes --requirepass "$REDIS_PASSWORD"

echo "Redis is running as '$CONTAINER_NAME' on port $HOST_PORT."
echo "Data persists in the '$VOLUME_NAME' Docker volume across container restarts."
echo
echo "Connection string for this app (replace <host> with this server's address/hostname):"
echo "  <host>:${HOST_PORT},password=${REDIS_PASSWORD},abortConnect=false"
echo
echo "Restrict access to the app server's IP only, e.g.:"
echo "  ufw allow from <app-server-ip> to any port ${HOST_PORT} proto tcp"
