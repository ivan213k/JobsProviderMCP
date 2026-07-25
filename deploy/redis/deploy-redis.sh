#!/usr/bin/env bash
set -euo pipefail

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

if [[ -f "$script_dir/.env" ]]; then
    set -a
    # shellcheck disable=SC1091
    source "$script_dir/.env"
    set +a
fi

IMAGE="redis:7-alpine"
CONTAINER_NAME="jobsprovider-redis"
VOLUME_NAME="jobsprovider-redis-data"
NETWORK_NAME="jobsprovider-net"
HOST_PORT="${HOST_PORT:-6379}"
REDIS_PASSWORD="${REDIS_PASSWORD:?REDIS_PASSWORD must be set}"

echo "Pulling $IMAGE"
docker pull "$IMAGE"

docker rm -f "$CONTAINER_NAME" >/dev/null 2>&1 || true

docker volume create "$VOLUME_NAME" >/dev/null
docker network create "$NETWORK_NAME" >/dev/null 2>&1 || true

docker run -d \
    --name "$CONTAINER_NAME" \
    --restart unless-stopped \
    --network "$NETWORK_NAME" \
    -p "${HOST_PORT}:6379" \
    -v "${VOLUME_NAME}:/data" \
    "$IMAGE" \
    redis-server --appendonly yes --requirepass "$REDIS_PASSWORD"

echo "Redis is running as '$CONTAINER_NAME' on port $HOST_PORT, reachable from the local network."
echo "Data persists in the '$VOLUME_NAME' Docker volume across container restarts."
echo
echo "If the app is a Docker container on this same host, run it on the '$NETWORK_NAME' network too (deploy.sh"
echo "already does this) and use the container name as the host — 'localhost' will NOT work, since each"
echo "container has its own network namespace:"
echo "  ${CONTAINER_NAME}:${HOST_PORT},password=${REDIS_PASSWORD},abortConnect=false"
echo
echo "From elsewhere on the LAN (or a non-containerized process on this host), use this server's LAN IP/hostname"
echo "instead of '${CONTAINER_NAME}'."
echo
echo "requirepass is set, but the port is reachable by anything on the local network. If this server also has a"
echo "public interface, restrict the port to your LAN's actual CIDR, e.g.:"
echo "  ufw allow from <your-lan-cidr, e.g. 192.168.1.0/24> to any port ${HOST_PORT} proto tcp"
