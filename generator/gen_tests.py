from __future__ import annotations

import argparse
import json
import re
from pathlib import Path
from typing import Any


def load_data(path: Path) -> dict[str, Any]:
    text = path.read_text(encoding="utf-8")
    try:
        import yaml
    except ModuleNotFoundError:
        return json.loads(text)

    data = yaml.safe_load(text)
    if not isinstance(data, dict):
        raise ValueError(f"{path} must contain a mapping at the top level")
    return data


def sanitize_identifier(value: str) -> str:
    value = re.sub(r"[^0-9A-Za-z_]+", "_", value.strip())
    value = re.sub(r"_+", "_", value).strip("_")
    if not value:
        return "Case"
    if value[0].isdigit():
        value = f"Case_{value}"
    return value


def csharp_string(value: str) -> str:
    escaped = (
        value.replace("\\", "\\\\")
        .replace("\"", "\\\"")
        .replace("\r", "\\r")
        .replace("\n", "\\n")
        .replace("\t", "\\t")
    )
    return f"\"{escaped}\""


def csharp_literal(value: Any) -> str:
    if value is None:
        return "null"
    if isinstance(value, str):
        return csharp_string(value)
    if isinstance(value, bool):
        return "true" if value else "false"
    if isinstance(value, float):
        return repr(value)
    return str(value)


def csharp_type(exception_name: str) -> str:
    if "." in exception_name:
        return exception_name
    return exception_name


def test_name(method_name: str, case: dict[str, Any]) -> str:
    reqs = "_".join(case.get("requirements", [])) or "REQ"
    reqs = sanitize_identifier(reqs.replace("-", "_"))
    case_id = sanitize_identifier(str(case.get("id", case.get("case", "Case"))))
    return f"{sanitize_identifier(method_name)}_{case_id}_{reqs}"


def build_testcase_args(case: dict[str, Any], include_expected: bool) -> str:
    inputs = [csharp_literal(value) for value in case.get("inputs", [])]
    if include_expected:
        inputs.append(csharp_literal(case["expected"]))
    else:
        inputs.append(f"typeof({csharp_type(case['expected_exception'])})")
    return ", ".join(inputs)


def render_return_tests(method: dict[str, Any], cases: list[dict[str, Any]]) -> str:
    if not cases:
        return ""
    method_name = method["name"]
    attrs = []
    for case in cases:
        args = build_testcase_args(case, include_expected=True)
        attrs.append(f"    [TestCase({args}, TestName = \"{test_name(method_name, case)}\")]")
    pre = csharp_string(method.get("pre", ""))
    post = csharp_string(method.get("post", ""))
    return "\n".join(attrs) + f"""
    public void {sanitize_identifier(method_name)}_ReturnCases_ReturnsExpectedResult(string? expression, double expected)
    {{
        TestContext.WriteLine(\"Pre: \" + {pre});
        TestContext.WriteLine(\"Post: \" + {post});

        var result = _sut.{method_name}(expression!);

        Assert.That(result, Is.EqualTo(expected).Within(1e-9));
    }}
"""


def render_exception_tests(method: dict[str, Any], cases: list[dict[str, Any]]) -> str:
    if not cases:
        return ""
    method_name = method["name"]
    attrs = []
    for case in cases:
        args = build_testcase_args(case, include_expected=False)
        attrs.append(f"    [TestCase({args}, TestName = \"{test_name(method_name, case)}\")]")
    pre = csharp_string(method.get("pre", ""))
    post = csharp_string(method.get("post", ""))
    return "\n".join(attrs) + f"""
    public void {sanitize_identifier(method_name)}_ExceptionCases_ThrowsExpectedException(string? expression, Type expectedException)
    {{
        TestContext.WriteLine(\"Pre: \" + {pre});
        TestContext.WriteLine(\"Post: \" + {post});

        Assert.Throws(expectedException, () => _sut.{method_name}(expression!));
    }}
"""


def render_method_tests(method: dict[str, Any]) -> str:
    cases = method.get("equivalence_classes", [])
    return_cases = [case for case in cases if case.get("assertion") == "returns"]
    exception_cases = [case for case in cases if case.get("assertion") == "throws"]
    return "\n".join(
        block
        for block in [
            render_return_tests(method, return_cases),
            render_exception_tests(method, exception_cases),
        ]
        if block
    )


def render_file(spec: dict[str, Any], config: dict[str, Any]) -> str:
    module = spec["module"]
    class_name = config.get("generated_class_name") or f"{module['name']}GeneratedTests"
    namespace = config.get("namespace", "Module.Tests")
    interface_namespace = config["interface_namespace"]
    implementation_namespace = config["implementation_namespace"]
    interface_type = config.get("interface_type", module["interface"])
    implementation_type = config.get("implementation_type", module["implementation"])
    methods = "\n".join(render_method_tests(method) for method in spec.get("methods", []))

    return f"""// =============================================================
// AUTO-GENERATED TESTS. DO NOT EDIT MANUALLY.
// Source: {config.get("spec_path", "N/A")}
// Generator: generator/gen_tests.py
// =============================================================

using System;
using NUnit.Framework;
using {interface_namespace};
using {implementation_namespace};

namespace {namespace};

[TestFixture]
[Description("Generated tests for {module['name']} formal specification")]
public class {class_name}
{{
    private {interface_type} _sut = null!;

    [SetUp]
    public void SetUp()
    {{
        _sut = new {implementation_type}();
    }}

{methods}
}}
"""


def main() -> None:
    parser = argparse.ArgumentParser(description="Generate C# NUnit tests from a YAML specification.")
    parser.add_argument("--config", default="config.yaml", help="Path to generator config.")
    args = parser.parse_args()

    config_path = Path(args.config)
    config = load_data(config_path)
    spec_path = Path(config["spec_path"])
    spec = load_data(spec_path)

    output_dir = Path(config.get("output_dir", "tests/Module.Tests"))
    output_dir.mkdir(parents=True, exist_ok=True)
    module_name = spec["module"]["name"]
    output_file = output_dir / f"{module_name}Tests.Generated.cs"
    output_file.write_text(render_file(spec, config), encoding="utf-8")

    test_count = sum(len(method.get("equivalence_classes", [])) for method in spec.get("methods", []))
    print(f"Generated {output_file}")
    print(f"Methods covered: {len(spec.get('methods', []))}")
    print(f"Test cases generated: {test_count}")


if __name__ == "__main__":
    main()
