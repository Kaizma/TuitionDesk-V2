# Agent guidance

## Sources of truth

- `docs/product.md` defines user-visible behavior, business rules, and scope.
- `docs/architecture.md` defines the system boundaries, dependency direction, and simplicity guardrails.
- `docs/roadmap.md` records delivery status and release checks.
- When implementation and docs differ, inspect the code and tests, then call out the discrepancy before changing product behavior.

## Project boundaries

- Keep business invariants in `TuitionDesk.Domain`; use cases and validation in `TuitionDesk.Application`; persistence and integrations in `TuitionDesk.Infrastructure`; HTTP and middleware in `TuitionDesk.Server`; window lifecycle in `TuitionDesk.Desktop`; UI behavior in `web/src`.
- Keep dependencies pointing inward. Follow nearby patterns and existing libraries before introducing abstractions or dependencies.
- Preserve local-first behavior and the declared stack. Avoid adding infrastructure or patterns that the architecture docs explicitly exclude unless the requested feature requires them.

## Task workflow

1. Read the relevant product rules, architecture notes, implementation, and tests before editing.
2. Identify the expected behavior and affected layers. If product intent or a consequential tradeoff is unclear, ask before choosing.
3. Make the smallest complete change, including validation, error/loading states, and focused tests where behavior changes.
4. Run the relevant checks from the README and CI workflow. Do not claim checks passed unless they were run successfully.
5. Summarize behavior changed, files or areas affected, checks run and their results, and any remaining limitations.
