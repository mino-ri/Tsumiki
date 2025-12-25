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

# 調査結果

NPlugのソースコードを調査した結果、M4 Macで.vst3ファイルが生成されない原因を特定しました。

## 問題の根本原因

macOS（M4 Mac含む）では、VST3プラグインは単一ファイルではなく、特定のディレクトリ構造を持つバンドルでなければならないのですが、NPlugは現在この構造を自動生成していません。

## プラットフォーム間の違い

Windows環境:
- VST3プラグインは単一の.vst3ファイル（実際はDLL）
- NPlugはdotnet publish後、DLLを.vst3にリネームするだけで動作

macOS環境:
- VST3プラグインは以下のようなバンドル構造が必須:
Tsumiki.vst3/               # ディレクトリ（ファイルではない）
├── Contents/
│   ├── MacOS/
│   │   └── Tsumiki         # 実際のバイナリ
│   └── Info.plist          # 必須のプロパティリストファイル

## NPlugの現在の動作

macOSでdotnet publishを実行すると:
1. NativeAOTが.dylibファイルを生成
2. NPlugは単純にファイル名を変更するだけ（../NPlug-main/src/NPlug/build/NPlug.targets:76-79）
3. 結果: 単一の.dylibファイルができるだけで、必要なバンドル構造が作られない

## 証拠

- NPlugのドキュメント（../NPlug-main/doc/readme.md:379）にも「他のプラットフォームではプラグイン構造を正しく設定する必要がある（issue #1参照）」と明記されています
- この問題はhttps://github.com/xoofx/NPlug/issues/1として認識されています

## 関連ファイル

- ../NPlug-main/src/NPlug/build/NPlug.targets:76-79 - .vst3へのリネーム処理（macOS対応が不足）
- ../NPlug-main/src/NPlug.Validator/build/NPlug.Validator.targets - Validator用には適切なバンドル構造を作成している（参考実装）

この問題を解決するには、macOS向けに適切なバンドル構造を生成するMSBuildターゲットを追加する必要があります。
