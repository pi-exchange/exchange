"""Validate the lab deliverables without touching application or runtime state."""
from pathlib import Path
import re
import struct

ROOT = Path(__file__).resolve().parents[1]
DOC = ROOT / "docs/01-requirements.md"
text = DOC.read_text(encoding="utf-8")
readme = (ROOT / "README.md").read_text(encoding="utf-8")
expected = {f"FR-{i}" for i in range(1, 11)}
requirements = {}
for line in text.splitlines():
    cells = [c.strip() for c in line.strip("|").split("|")]
    if len(cells) == 5 and cells[0] in expected:
        assert cells[0] not in requirements, f"Duplicate {cells[0]}"
        requirements[cells[0]] = cells
assert set(requirements) == expected, "Expected FR-1 through FR-10"
for key, cells in requirements.items():
    assert "может " in cells[1] and cells[4], f"Missing user capability/acceptance: {key}"
    assert cells[2] in {"обязательно в этом семестре", "желательно", "потом"}
    assert cells[3] in {"уже есть", "нужно доработать", "нужно сделать"}
    assert "| " + " | ".join(cells[:4]) + " |" in readme, f"README drift: {key}"
assert len(re.findall(r"^\| FR-\d+ \|", readme, re.M)) == 10
assert re.findall(r"^\| (NFR-\d+) \|", text, re.M) == [f"NFR-{i}" for i in range(1, 6)]
scenarios = re.findall(r"^### (US-\d+)\. .*? — (FR-\d+)", text, re.M)
assert len(scenarios) == 10 and len(set(scenarios)) == 10
assert {fr for _, fr in scenarios} == expected
for us, fr in scenarios:
    assert f"| {fr} | {us} |" in text, f"Missing traceability: {us}"
    block = re.search(rf"^### {us}\. .*?(?=^### |\Z)", text, re.M | re.S).group()
    for marker in ("**Я как", "- Вход:", "- Шаги:", "- Выход:"):
        assert marker in block, f"Missing {marker} in {us}"
assert text.count("**Конкретный пример (успех).**") == 3
assert text.count("**Конкретный пример (ошибка).**") == 3
assert "Версия документа: 1.0" in text
assert "## 1.0 — 2026-11-05" in (ROOT / "CHANGELOG.md").read_text(encoding="utf-8")
names = {"context", "order-activity", "use-cases", "transfer-sequence", "liquidity-sequence", "data-model", "order-state"}
folder = ROOT / "docs/diagrams"
assert {p.stem for p in folder.glob("*.puml")} == names
assert {p.stem for p in folder.glob("*.png")} == names
for name in names:
    source = (folder / f"{name}.puml").read_text(encoding="utf-8")
    assert source.startswith("@startuml") and source.rstrip().endswith("@enduml")
    assert "FR-" in source
    data = (folder / f"{name}.png").read_bytes()
    assert data[:8] == b"\x89PNG\r\n\x1a\n", f"Invalid PNG: {name}"
    w, h = struct.unpack(">II", data[16:24])
    assert w >= 200 and h >= 100, f"Unexpected PNG size: {name}"
    for ext in ("puml", "png"):
        assert f"(diagrams/{name}.{ext})" in text
usecases = (folder / "use-cases.puml").read_text(encoding="utf-8")
assert expected <= set(re.findall(r"\bFR-\d+\b", usecases))
for path in [DOC, ROOT / "README.md", folder / "README.md"]:
    for target in re.findall(r"\]\(([^)]+)\)", path.read_text(encoding="utf-8")):
        if "://" in target or target.startswith("#"):
            continue
        assert (path.parent / target.split("#")[0]).exists(), f"Broken link in {path.name}: {target}"
print("Lab 2: 10 FR, 5 NFR, 10 US, 3 example pairs, 7 diagram pairs and local links verified.")
