#!/bin/bash
# Metal シェーダーをコンパイルして C# コードに埋め込むスクリプト

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"

SHADER_SRC="$PROJECT_ROOT/Tsumiki.View.Mac/Native/Shaders.metal"
SHADER_OUT="$PROJECT_ROOT/Tsumiki.View.Mac/ShaderLibrary.g.cs"

# シェーダーファイルの存在確認
if [ ! -f "$SHADER_SRC" ]; then
    echo "Error: $SHADER_SRC not found" >&2
    exit 1
fi

echo "Compiling Metal shaders..."

# Metal → .air (中間表現)
xcrun -sdk macosx metal -c "$SHADER_SRC" -o /tmp/Shaders.air

# .air → .metallib (Metal ライブラリ)
xcrun -sdk macosx metallib /tmp/Shaders.air -o /tmp/Shaders.metallib

# .metallib を C# コードに埋め込み
python3 "$SCRIPT_DIR/embed_binary.py" /tmp/Shaders.metallib > "$SHADER_OUT"

# 一時ファイルを削除
rm -f /tmp/Shaders.air /tmp/Shaders.metallib

echo "Shader compilation complete: $SHADER_OUT"
