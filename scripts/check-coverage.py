#!/usr/bin/env python3
"""Fail the build when a coverage report is below the required threshold.

Every app in this repository has to reach 70% line coverage, and the number
has to be produced by the same measurement on every machine. This script is
the gate: it reads the report a coverage tool produced and exits non-zero
when the line coverage is under the threshold.

Two report formats are understood:

  * Cobertura XML (.NET / coverlet) - the <coverage> root element carries a
    "line-rate" attribute between 0 and 1.
  * Istanbul JSON summary (JavaScript / vitest) - "total.lines.pct" holds a
    percentage between 0 and 100.

Usage:
    scripts/check-coverage.py <report> <threshold> [label]

    report     path to the coverage report
    threshold required line coverage, as a percentage (70)
    label      name used in the output (defaults to the report's directory)
"""

from __future__ import annotations

import json
import sys
import xml.etree.ElementTree as ET
from pathlib import Path


def read_cobertura(path: Path) -> float:
    """Return the line coverage of a Cobertura report, as a percentage."""
    root = ET.parse(path).getroot()
    rate = root.get("line-rate")
    if rate is None:
        raise SystemExit(f"{path}: no 'line-rate' attribute - not a Cobertura report?")
    return float(rate) * 100


def read_istanbul(path: Path) -> float:
    """Return the line coverage of an Istanbul JSON summary, as a percentage."""
    summary = json.loads(path.read_text())
    try:
        return float(summary["total"]["lines"]["pct"])
    except KeyError as error:
        raise SystemExit(f"{path}: missing 'total.lines.pct' - {error}") from error


def read_report(path: Path) -> float:
    if path.suffix == ".xml":
        return read_cobertura(path)
    if path.suffix == ".json":
        return read_istanbul(path)
    raise SystemExit(f"{path}: unsupported report format (expected .xml or .json)")


def main(argv: list[str]) -> int:
    if len(argv) < 3:
        print(__doc__, file=sys.stderr)
        return 2

    report = Path(argv[1])
    threshold = float(argv[2])
    label = argv[3] if len(argv) > 3 else str(report.parent)

    if not report.is_file():
        print(f"FAIL  {label}: no coverage report at {report}", file=sys.stderr)
        return 1

    coverage = read_report(report)
    status = "PASS" if coverage >= threshold else "FAIL"

    print(f"{status}  {label}: line coverage {coverage:.2f}% (threshold {threshold:.2f}%)")

    return 0 if coverage >= threshold else 1


if __name__ == "__main__":
    sys.exit(main(sys.argv))
