#!/bin/bash
set -eu

pushd "$(dirname "$0")" > /dev/null
SCRIPT_PATH=$(pwd)
popd > /dev/null
source "$SCRIPT_PATH/common.sh"

if [ "$(dpkg --print-architecture)" = "amd64" ]; then
    ARCH="x64"
elif [ "$(dpkg --print-architecture)" = "i386" ]; then
    ARCH="x86"
elif [ "$(dpkg --print-architecture)" = "arm64" ]; then
    ARCH="arm64"
elif [ "$(dpkg --print-architecture)" = "armhf" ]; then
    ARCH="arm"
else
    echo "Unsupported architecture: $(dpkg --print-architecture)"
    exit 1
fi

FFMPEG_FLAGS+=(
    # --enable-vaapi
    # --enable-vdpau
    # --enable-hwaccel='h264_vaapi,h264_vdpau'
    # --enable-hwaccel='hevc_vaapi,hevc_vdpau'
    # --enable-hwaccel='vp8_vaapi,vp8_vdpau'
    # --enable-hwaccel='vp9_vaapi,vp9_vdpau'

    --target-os=linux
)

pushd . > /dev/null

prep_ffmpeg linux-$ARCH
# Apply patch from upstream to fix errors with new binutils versions:
# Ticket: https://fftrac-bg.ffmpeg.org/ticket/10405
# This patch should be removed when FFmpeg is updated to >=6.1
patch -p1 < "$SCRIPT_PATH/fix-binutils-2.41.patch"
build_ffmpeg
popd > /dev/null

# gcc creates multiple symlinks per .so file for versioning.
# We delete the symlinks and rename the real files to include the major library version
rm linux-$ARCH/*.so
for f in linux-$ARCH/*.so.*.*.*; do
    mv -vf "$f" "${f%.*.*}"
done
