#!/usr/bin/env bash
# Copies this deploy/ folder to a server, for use by both the app server (deploy.sh/watch.sh) and the Redis
# server (redis/deploy-redis.sh). Run this locally, not on the server.
#
# Usage: deploy/copy-to-server.sh user@host [remote-dir]
set -euo pipefail

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

REMOTE="${1:?Usage: $0 user@host [remote-dir]}"
REMOTE_DIR="${2:-jobsproviderapi-deploy}"

echo "Copying $script_dir/ to $REMOTE:~/$REMOTE_DIR/"

# --delete keeps the remote folder in sync with this one (e.g. removing scripts that were deleted locally),
# while the excludes protect server-local, never-committed files: secrets (.env), deploy state
# (.current-version), and logs (watch.log).
rsync -avz --delete \
    --exclude '.env' \
    --exclude '.current-version' \
    --exclude '*.log' \
    "$script_dir/" "$REMOTE:~/$REMOTE_DIR/"

ssh "$REMOTE" "chmod +x ~/$REMOTE_DIR/deploy.sh ~/$REMOTE_DIR/watch.sh ~/$REMOTE_DIR/redis/deploy-redis.sh"

echo "Done. Scripts are executable at $REMOTE:~/$REMOTE_DIR/"
