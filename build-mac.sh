#!/bin/bash
# Tsumiki Mac 版統合ビルドスクリプト

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

echo "=== Building Tsumiki for macOS ==="
echo ""

# 1. リソース生成
echo "[1/4] Generating image resources..."
if ! command -v python3 &> /dev/null; then
    echo "Error: python3 is required" >&2
    exit 1
fi

# 必要な Python パッケージの確認
python3 -c "import PIL, brotli" 2>/dev/null || {
    echo "Error: Required Python packages not installed"
    echo "Please install: pip3 install Pillow brotli"
    exit 1
}

python3 Tools/generate_image_resources.py
echo ""

# 2. シェーダーコンパイル
echo "[2/4] Compiling Metal shaders..."
./Tools/compile_shaders.sh
echo ""

# 3. ネイティブライブラリビルド
echo "[3/4] Building native Metal renderer..."
cd Tsumiki.View.Mac/Native
make clean
make
cd ../..
echo ""

# 4. C# プロジェクトビルド
echo "[4/4] Building C# projects..."
dotnet build Tsumiki/Tsumiki.csproj -c Release -r osx-arm64

echo ""
echo "=== Build complete! ==="
echo ""
echo "Output: Tsumiki/bin/Release/net10.0/osx-arm64/"
