# Exchange maintenance

## Educational copy: pi-exchange

For laboratory work, use `lab-2` with PRs into `main`; retain mirrored `master` as the source baseline. All lab changes and GitHub operations belong exclusively to `pi-exchange/exchange`. Do not modify, synchronize, deploy, or push to the original Novcic repositories. The historical instructions below describe the upstream layout and do not authorize changes outside this copy. In particular, do not run the inherited setup/reset or sibling-doc synchronization commands against the original environment.

Lab 2 is a documentation change: keep runtime code and the API contract intact. Validate with `python scripts/check-lab2.py` and `python scripts/check-contract.py`; regenerate and visually inspect PNGs after diagram edits. Keep `LAB2-HANDOFF.md` untracked via the local Git exclude file. Actual teammate reviews/approvals must come from their own accounts; merge and tag `lab-2` only after both reviews are complete.

Use master. The exchange repository is NovcicExchange; ../docs is a separate public repository. Preserve Docker project names, database volumes and local chain state when changing workspace layout.

Read BRANDING.md before interface changes. Keep the shared Meadow light design and synchronize tokens/docs with scripts/sync-docs.py. Do not commit task.txt or operational secrets.

Financial amounts use decimal arithmetic and exact JSON strings. Financial mutations and idempotency receipts must commit atomically. Public books expose aggregated levels only. Never fabricate liquidity or multiply quoting budgets by quote-level count.

API changes require an audited export-api.py catalog update, regenerated public contract, updated sibling documentation and focused validation. Run python scripts/check-contract.py to detect source/contract drift. Record implementation and verification in IMPLEMENTATION.md.
