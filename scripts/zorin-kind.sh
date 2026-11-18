#!/usr/bin/env bash
# scripts/zorin-kind.sh — Zorin OS + Podman + kind local validation.
#
# Two paths are supported:
#
#   1. Podman Desktop (recommended): use the GUI's "Push image to Kind cluster"
#      button — see https://podman-desktop.io/docs/kind/building-an-image-and-testing-it-in-kind.
#      Image references in manifests stay as `localhost/moviesapp-<app>:latest`.
#      No local registry needed.
#
#   2. Pure CLI: the script also works on raw `podman` + `kind` without the GUI.
#      It detects which path you're on and acts accordingly.
#
# Usage:
#   ./scripts/zorin-kind.sh         # up + verify (or just "up" if k8s/ is empty)
#   ./scripts/zorin-kind.sh down    # tear down the cluster
#
# Requires: podman, kubectl, jq, openssl, kind.

set -euo pipefail

CLUSTER=moviesapp
NS=moviesapp
OVERLAY=/tmp/moviesapp-k8s-zorin
SRC=k8s
APPS=(angular-client mvc-web web-api)

cmd="${1:-up}"

color() { printf '\033[%sm%s\033[0m\n' "$1" "$2"; }
info()  { color "1;34" "ℹ  $*"; }
ok()    { color "1;32" "✅ $*"; }
warn()  { color "1;33" "⚠  $*"; }
err()   { color "1;31" "❌ $*" >&2; exit 1; }

[[ -d "$SRC" ]] || err "no $SRC/ directory; run from the repo root"
command -v podman   >/dev/null || err "podman not installed"
command -v kubectl  >/dev/null || err "kubectl not installed"
command -v kind     >/dev/null || err "kind not installed (https://kind.sigs.k8s.io)"
command -v openssl  >/dev/null || err "openssl not installed"
command -v jq       >/dev/null || err "jq not installed"

# Detect Podman Desktop. The CLI tools (podman, podman compose) are present
# either way, so we check for the desktop integration daemon or its config.
has_podman_desktop() {
  pgrep -f "podman-desktop" >/dev/null 2>&1 || \
  [[ -d "$HOME/.local/share/containers/podman-desktop" ]] || \
  command -v podman-desktop >/dev/null 2>&1
}

# Build images with `podman build`. Image references use `localhost/...` because
# that's what kind's image cache expects (no registry needed).
build_images() {
  info "Building images with Podman"
  for app in "${APPS[@]}"; do
    podman build -t "localhost/moviesapp-${app}:latest" "apps/${app}"
  done
}

create_cluster() {
  if kubectl config get-clusters 2>/dev/null | grep -q "^kind-${CLUSTER}$"; then
    info "kind cluster $CLUSTER already exists"
    return
  fi
  info "Creating kind cluster $CLUSTER"
  cat <<EOF | kind create cluster --name "$CLUSTER" --config -
kind: Cluster
apiVersion: kind.x-k8s.io/v1alpha4
nodes:
  - role: control-plane
    extraPortMappings:
      - containerPort: 80
        hostPort: 80
EOF
  # After the cluster exists, load the images. (Podman Desktop users may have
  # already done this via the GUI; `kind load` is idempotent.)
  for app in "${APPS[@]}"; do
    kind load docker-image "localhost/moviesapp-${app}:latest" --name "$CLUSTER"
  done
}

# Build the overlay for $SRC/*.yaml with three substitutions:
#   1. <ACR>.azurecr.io/moviesapp-<app> → localhost/moviesapp-<app>
#   2. <sha>                           → latest
#   3. managed-csi                     → standard (kind default StorageClass)
build_overlay() {
  info "Building overlay at $OVERLAY"
  rm -rf "$OVERLAY"; mkdir -p "$OVERLAY"
  if [[ -z "$(ls -A "$SRC" 2>/dev/null)" ]]; then
    warn "$SRC/ is empty — skipping overlay generation. Start the cluster anyway for hands-on kubectl practice."
    return 0
  fi
  for f in "$SRC"/*.yaml; do
    [[ "$(basename "$f")" == "13-web-api-backup-cronjob.yaml" ]] && continue
    [[ "$(basename "$f")" == "40-ingress.yaml" ]] && continue
    sed -e "s|<ACR>\.azurecr\.io/moviesapp-|localhost/moviesapp-|g" \
        -e 's|<sha>|latest|g' \
        -e 's|managed-csi|standard|g' \
        "$f" > "$OVERLAY/$(basename "$f")"
  done
}

apply_manifests() {
  info "Applying manifests"
  kubectl create namespace "$NS" --dry-run=client -o yaml | kubectl apply -f -
  local jwt; jwt=$(openssl rand -base64 32)
  kubectl -n "$NS" create secret generic moviesapp-secrets \
    --from-literal="JwtSettings__SecretKey=${jwt}" \
    --dry-run=client -o yaml | kubectl apply -f -
  kubectl apply -f "$OVERLAY"
}

verify() {
  info "Waiting for pods"
  kubectl -n "$NS" wait --for=condition=Ready pod \
    -l 'app in (web-api, mvc-web, angular-client)' --timeout=180s
  ok "All three pods Running"

  info "Verifying mvc-web → web-api"
  local resp
  resp=$(kubectl -n "$NS" exec deploy/mvc-web -- curl -s http://web-api:8080/health || true)
  if [[ "$resp" == *'"status":"ok"'* ]]; then
    ok "mvc-web reaches web-api: $resp"
  else
    err "in-cluster call failed: '$resp'"
  fi

  info "Port-forwards (Ctrl-C to stop):"
  kubectl -n "$NS" port-forward svc/angular-client 4080:4000 &
  kubectl -n "$NS" port-forward svc/mvc-web        8088:8080 &
  wait
}

down() {
  info "Deleting kind cluster"
  kind delete cluster --name "$CLUSTER" 2>/dev/null || true
  rm -rf "$OVERLAY"
  ok "Down"
}

case "$cmd" in
  up)
    if has_podman_desktop; then
      info "Podman Desktop detected — using native image push (no local registry needed)"
      info "If you've already pushed images via the GUI, this is enough. Otherwise:"
      info "  - Build images:  podman build -t localhost/moviesapp-<app>:latest apps/<app>"
      info "  - Push to kind:  Podman Desktop → Images → overflow menu → Push image to Kind cluster"
      echo
      info "Building images anyway as a fallback in case you haven't already."
      build_images
    else
      info "Podman Desktop not detected — using CLI path (kind load docker-image)"
      build_images
    fi
    create_cluster
    build_overlay
    if [[ -z "$(ls -A "$OVERLAY" 2>/dev/null)" ]]; then
      info "Cluster is up at $CLUSTER — use it for hands-on kubectl practice."
      info "Manifests under $SRC/ are empty (expected during the learning track)."
    else
      apply_manifests
      verify
    fi
    ;;
  down)
    down
    ;;
  *)
    err "unknown command: $cmd (use 'up' or 'down')"
    ;;
esac
