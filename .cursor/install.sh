#!/usr/bin/env bash
#
# Cloud Agent bootstrap for the Processor-Emulator repository.
#
# The primary product (ProcessorEmulator.csproj) is a Windows-only WPF/WinForms
# app (net6.0-windows) and can neither build with markup compilation nor run on
# Linux. Cloud Agents run Linux, so this script provisions the .NET 6 SDK the
# repo pins and warms the cross-platform console demo (BoltDemo_Standalone),
# which is the component that actually builds AND runs on Linux.
#
# The script is idempotent: re-running it skips an already-installed SDK and
# only refreshes the demo build.
set -euo pipefail

DOTNET_DIR="/usr/local/dotnet"
DOTNET_CHANNEL="6.0"

export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_NOLOGO=1

if [ ! -x "${DOTNET_DIR}/dotnet" ]; then
  echo "Installing .NET SDK ${DOTNET_CHANNEL} into ${DOTNET_DIR}..."
  tmp="$(mktemp -d)"
  curl -fsSL https://dot.net/v1/dotnet-install.sh -o "${tmp}/dotnet-install.sh"
  chmod +x "${tmp}/dotnet-install.sh"
  sudo mkdir -p "${DOTNET_DIR}"
  sudo "${tmp}/dotnet-install.sh" --channel "${DOTNET_CHANNEL}" --install-dir "${DOTNET_DIR}"
  rm -rf "${tmp}"
else
  echo ".NET SDK already present: $(${DOTNET_DIR}/dotnet --version)"
fi

# Expose dotnet on PATH for interactive agent shells (idempotent symlink).
sudo ln -sf "${DOTNET_DIR}/dotnet" /usr/local/bin/dotnet

export DOTNET_ROOT="${DOTNET_DIR}"
export PATH="${DOTNET_DIR}:${PATH}"

echo "Using dotnet $(dotnet --version)"

# Warm up the cross-platform console demo (restore + build). This is the target
# that runs end-to-end on Linux.
dotnet build BoltDemo_Standalone/BoltDemo.csproj -c Release

echo "Environment ready."
