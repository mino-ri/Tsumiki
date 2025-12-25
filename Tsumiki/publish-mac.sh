#!/bin/bash

set -e
dotnet publish -c Release -r osx-arm64
