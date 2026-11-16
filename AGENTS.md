# Exchange maintenance

## Educational copy: pi-exchange

For laboratory work, use the corresponding `lab-N` branch with PRs into `main`; retain mirrored `master` as the source baseline. All lab changes and GitHub operations belong exclusively to `pi-exchange/exchange`. Do not modify, synchronize, deploy, or push to the original Novcic repositories. The historical instructions below describe the upstream layout and do not authorize changes outside this copy. In particular, do not run the inherited setup/reset or sibling-doc synchronization commands against the original environment.

Lab 2 is a documentation change: keep runtime code and the API contract intact. Validate with `python scripts/check-lab2.py` and `python scripts/check-contract.py`; regenerate and visually inspect PNGs after diagram edits. Keep `LAB2-HANDOFF.md` untracked via the local Git exclude file. Actual teammate reviews/approvals must come from their own accounts; merge and tag `lab-2` only after both reviews are complete.

Lab 3 documents the existing `POST /api/transfer` operation in `docs/interface-contract.md`, linked to US-4 and FR-4. Use `lab-3`; do not change runtime code, database schema or generated `api-contract` files. Check the examples against the implementation and run both existing Python checks above. Record the actual validation scope in IMPLEMENTATION.md. Keep `LAB3-HANDOFF.md` untracked via the local Git exclude file. Both other participants must independently describe a call and its expected response (or request clarification) in the PR and approve it themselves. Respond to feedback before merging; place tag `lab-3` on the merged commit only after both reviews are complete.

Lab 4 uses `lab-4` and tests the real transfer module with a dedicated PostgreSQL database named exactly `pi_exchange_lab4_tests`. Run only the `FullyQualifiedName~TransferContractTests` filter for this lab; never point its reset fixture at an existing application database. Preserve runtime code and generated API files. Demonstrate one deliberate assertion failure and its correction in separate commits on the same PR, with links to both Actions runs. Keep `LAB4-HANDOFF.md` untracked through the local Git exclude file. Obtain both teammates' real reviews/approvals, merge lab 3 first, integrate main, and merge/tag `lab-4` only with successful current checks.

Use master. The exchange repository is NovcicExchange; ../docs is a separate public repository. Preserve Docker project names, database volumes and local chain state when changing workspace layout.

Read BRANDING.md before interface changes. Keep the shared Meadow light design and synchronize tokens/docs with scripts/sync-docs.py. Do not commit task.txt or operational secrets.

Financial amounts use decimal arithmetic and exact JSON strings. Financial mutations and idempotency receipts must commit atomically. Public books expose aggregated levels only. Never fabricate liquidity or multiply quoting budgets by quote-level count.

API changes require an audited export-api.py catalog update, regenerated public contract, updated sibling documentation and focused validation. Run python scripts/check-contract.py to detect source/contract drift. Record implementation and verification in IMPLEMENTATION.md.
