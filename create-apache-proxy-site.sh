#!/usr/bin/env bash
set -euo pipefail

usage() {
  cat <<'EOF'
Usage:
  create-apache-proxy-site.sh --conf-name NAME --server-name DOMAIN --proxy-port PORT [--vhost-port PORT]

Required:
  --conf-name    Apache site config name (without .conf), e.g. logsheetxtractor
  --server-name  ServerName value, e.g. logsheetxtractor.janglos.io
  --proxy-port   Local port to proxy to, e.g. 3000

Optional:
  --vhost-port   VirtualHost listen port (default: 80)
EOF
}

CONF_NAME=""
SERVER_NAME=""
PROXY_PORT=""
VHOST_PORT="80"

while [[ $# -gt 0 ]]; do
  case "$1" in
    --conf-name)
      CONF_NAME="${2:-}"
      shift 2
      ;;
    --server-name)
      SERVER_NAME="${2:-}"
      shift 2
      ;;
    --proxy-port)
      PROXY_PORT="${2:-}"
      shift 2
      ;;
    --vhost-port)
      VHOST_PORT="${2:-}"
      shift 2
      ;;
    -h|--help)
      usage
      exit 0
      ;;
    *)
      echo "Unknown argument: $1" >&2
      usage
      exit 1
      ;;
  esac
done

if [[ -z "$CONF_NAME" || -z "$SERVER_NAME" || -z "$PROXY_PORT" ]]; then
  echo "Missing required arguments." >&2
  usage
  exit 1
fi

if ! [[ "$PROXY_PORT" =~ ^[0-9]+$ ]] || ! [[ "$VHOST_PORT" =~ ^[0-9]+$ ]]; then
  echo "Ports must be numeric." >&2
  exit 1
fi

CONF_PATH="/etc/apache2/sites-available/${CONF_NAME}.conf"

if [[ $EUID -ne 0 ]]; then
  SUDO="sudo"
else
  SUDO=""
fi

$SUDO tee "$CONF_PATH" >/dev/null <<EOF
<VirtualHost *:${VHOST_PORT}>
    ServerName ${SERVER_NAME}

    # Enable proxying
    ProxyPreserveHost On
    ProxyRequests Off

    # Important: Route WebSocket traffic for SignalR FIRST
    # The URL matches the port you exposed in docker-compose.yml
    RewriteEngine On
    RewriteCond %{HTTP:UPGRADE} ^WebSocket$ [NC]
    RewriteCond %{HTTP:CONNECTION} Upgrade$ [NC]
    RewriteRule /(.*) ws://127.0.0.1:${PROXY_PORT}/\$1 [P,L]

    # Proxy all other standard HTTP traffic to the frontend NGINX container
    ProxyPass / http://127.0.0.1:${PROXY_PORT}/
    ProxyPassReverse / http://127.0.0.1:${PROXY_PORT}/
</VirtualHost>
EOF

$SUDO a2ensite "${CONF_NAME}.conf"
$SUDO systemctl reload apache2

echo "Created: ${CONF_PATH}"
echo "Enabled site: ${CONF_NAME}.conf"
echo "Reloaded apache2"
