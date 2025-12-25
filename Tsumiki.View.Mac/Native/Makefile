# Tsumiki Metal レンダラービルド設定

TARGET = libTsumikiMetal.dylib
SOURCES = TsumikiMetal.m
FRAMEWORKS = -framework Metal -framework MetalKit -framework Cocoa -framework QuartzCore
CFLAGS = -O3 -fPIC -mmacosx-version-min=11.0 -fobjc-arc
LDFLAGS = -dynamiclib -install_name @rpath/$(TARGET)

all: $(TARGET)

$(TARGET): $(SOURCES)
	@echo "Building $(TARGET)..."
	clang $(CFLAGS) $(FRAMEWORKS) $(LDFLAGS) -o $@ $<
	@echo "Build complete: $(TARGET)"

clean:
	@echo "Cleaning..."
	rm -f $(TARGET)
	@echo "Clean complete"

.PHONY: all clean
