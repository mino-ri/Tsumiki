# 実装完了サマリー

## 作成されたファイル

### C# プロジェクト (Tsumiki.View.Mac)

- Tsumiki.View.Mac.csproj - プロジェクトファイル
- MacInterop.cs - P/Invoke インターフェース
- Renderer.cs - IDrawingContext 実装
- TsumikiCanvas.cs - ITsumikiCanvas 実装

### Objective-C レンダラー (Native/)

- TsumikiMetal.h - C API ヘッダー
- TsumikiMetal.m - Metal レンダラー実装 (20KB)
- Shaders.metal - Metal シェーダー (5KB)
- Makefile - ビルドスクリプト

### リソース生成ツール (Tools/)

- generate_image_resources.py - PNG → C# 埋め込み
- compile_shaders.sh - Metal シェーダーコンパイル
- embed_binary.py - バイナリ埋め込みヘルパー

### ビルドスクリプト

- build-mac.sh - 統合ビルドスクリプト

### プロジェクト設定変更

- Tsumiki/Tsumiki.csproj - 条件付き参照とプラットフォーム定数を追加
- Tsumiki/TsumikiPluginView.cs - 条件付きコンパイルを追加

## 次のステップ（Mac 実機での作業）

### 1. 依存パッケージのインストール

pip3 install Pillow brotli

### 2. ビルド実行

cd /Users/minori/Codes/Tsumiki
./build-mac.sh

### 3. ビルド順序

1. リソース生成 (画像を Brotli 圧縮して C# コードに埋め込み)
2. シェーダーコンパイル (.metal → .metallib → C# コード埋め込み)
3. ネイティブライブラリビルド (.dylib)
4. C# プロジェクトビルド

### 4. 生成されるファイル

- Tsumiki.View.Mac/Resources/ImageResource.g.cs (~7MB)
- Tsumiki.View.Mac/ShaderLibrary.g.cs (~数十KB)
- Tsumiki.View.Mac/Native/libTsumikiMetal.dylib

### 5. 確認事項

- Windows 版と同じ UI が表示されるか
- リサイズが正しく動作するか
- タブ切り替えで画像が切り替わるか
- グラフが正しく描画されるか
- メモリリークがないか

## 重要な注意点

1. NPlug の Mac サポート: NPlug が NSView をサポートしていることが前提です
2. Xcode コマンドラインツール: xcrun コマンドが必要です
3. macOS 11.0 以上: Metal 2.3 が必要です
4. Python 3: リソース生成に必要です

## トラブルシューティング

ビルドエラーが発生した場合は、エラーメッセージを確認してください。よくある問題:
- Python パッケージ未インストール → pip3 install Pillow brotli
- xcrun が見つからない → Xcode コマンドラインツールをインストール
- .dylib が見つからない → Native/Makefile でビルドを確認
