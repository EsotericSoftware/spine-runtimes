#!/bin/bash
set -e

python3 ../spine-cpp/spine-cpp-lite/spine-cpp-lite-codegen.py > Sources/Spine/Spine.Generated.swift
