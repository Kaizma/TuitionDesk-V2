# Agent guidance

## Sources of truth

- `docs/product.md` defines user-visible behavior, business rules, and scope.
- `docs/architecture.md` defines the system boundaries, dependency direction, and simplicity guardrails.
- `docs/roadmap.md` records delivery status and release checks.
- `docs/engineering-guide.md` is a concise repo map and audit handoff for agent orientation; it is not authoritative over the product, architecture, or roadmap docs.
- When implementation and docs differ, inspect the code and tests, then call out the discrepancy before changing product behavior.

## Project boundaries

- Keep business invariants in `TuitionDesk.Domain`; use cases and validation in `TuitionDesk.Application`; persistence and integrations in `TuitionDesk.Infrastructure`; HTTP and middleware in `TuitionDesk.Server`; window lifecycle in `TuitionDesk.Desktop`; UI behavior in `web/src`.
- Keep dependencies pointing inward. Follow nearby patterns and existing libraries before introducing abstractions or dependencies.
- Preserve local-first behavior and the declared stack. Avoid adding infrastructure or patterns that the architecture docs explicitly exclude unless the requested feature requires them.

## Task workflow

1. Read the relevant product rule, one nearby implementation, and its tests; use `docs/engineering-guide.md` to find a representative flow and checks.
2. Before editing, pin down expected behavior, affected layers, failure cases, and acceptance checks. Ask only when a product rule or consequential tradeoff is genuinely unclear; otherwise follow the nearest working pattern.
3. Make the smallest complete change. Keep server-side rules authoritative and add focused tests for changed behavior.
4. Update Markdown on every coding task: update the authoritative product/architecture/roadmap doc when its content changes; otherwise replace the `Latest agent task` entry in `docs/engineering-guide.md` with a one-line handoff. Do not add a new history log or duplicate product rules.
5. Run checks for the changed layer using the matrix in `docs/engineering-guide.md`. Report only checks actually run, plus changes, docs updated, and known limitations.

For simple tasks, keep the task contract and final report short. Do not restate whole docs, invent requirements, or claim behavior that code/tests do not support.
