#!/usr/bin/env bash
set -o xtrace
target="${1}cleanupcode.sh"
$target ignis.sln -s="ignis.sln.DotSettings" --exclude="tools/**/*" --profile="Built-in: Reformat & Apply Syntax Style" $2
