  echo "=== xcode-select ==="
  xcode-select -p

  echo ""
  echo "=== xcrun --find metal ==="
  xcrun -find metal 2>&1

  echo ""
  echo "=== Xcode.app 存在確認 ==="
  ls -la /Applications/Xcode.app/Contents/Developer/usr/bin/ | grep metal

  echo ""
  echo "=== SDK パス ==="
  xcrun --show-sdk-path

  echo ""
  echo "=== 環境変数 ==="
  echo "DEVELOPER_DIR: $DEVELOPER_DIR"
  