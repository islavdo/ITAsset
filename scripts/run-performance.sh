#!/usr/bin/env bash
set -euo pipefail

k6 run tests/Performance/k6-equipment.js
