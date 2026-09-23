# ExtraROM / MIPS memory refresh

Read-only refresh against `dev` tip. Addresses and symbols below are copied from source. Anything not found is marked UNKNOWN.

## 1. Tip

| | |
|---|---|
| Branch | `dev` (`HEAD` and `origin/dev`) |
| SHA | `98a7cb066c4d7a3acaccb6a2610278c7444d3534` |
| Subject | `coredll list-insert: cover twin FA1C past-RAM leave; STOP stackless 426E0` |
| Parent | `802bf53` — `apply coredll past-ram 4266C refuse framed+FFFF D885; leave 0x800426E0 (26d5ce5)` |

Local `dev` and `origin/dev` were the same SHA after `git fetch origin dev`.

## 2. Helpers

### `TryLeavePostApi52ListInsertPastRam`

- Path: `Core/CeRomTocFiles.cs` (definition ~40931; call ~13903 inside `TryTakeDumpMemListInsertDestMiss`)
- Fetch call: `MipsCpuEmulator.cs` ~513 calls `TryTakeDumpMemListInsertDestMiss` when PC is list-insert.
- From source: runs only after leftover-api52-cont is logged and `$a1` is KSEG0 past `GuestRamSize` (`0x10000000`, `IsKseg0PastGuestRam`, lo `Kseg0PastRamLo` `0x90000000`).
- If `$ra == ListInsert3F9E8JalLink` (`0x8003FA1C`, jal at `0x8003FA14` inside twin `0x8003F9E8`): sets `$ra` and PC to `ListInsert3F9E8NextFn` (`0x8003FA34`), via `twin-3f9e8-jal-link-nextfn`. Comment: dump-true third jal of list-insert; same past-RAM `$a1` class as the `0x8003F694` sites; no 3F694 frame unwind on this arm.
- If `$ra` is `CoredllDllMainExn15C28OuterRa` (`0x8003F784`) or `ListInsert3F694RaEarly` (`0x8003F714`): double-unwind when peeks are sane; on `sp-past-ram` / `peek-ra` two-frame exit to `ListInsert3F854JalLink` (`0x8003F9AC`) via `outer-jal-link-no-stack`; other refuse uses beq-fall. Other `$ra` returns false.
- List-insert site constants (`Core/CeRomTocFiles.cs` ~1603–1620): `CoredllDllMainC000Epc` `0x800151D0`, dump word `0xACA20000` (`sw $v0,0($a1)`), next PC `CoredllDllMainC000NextPc` `0x800151D4`. If the leave returns false, `TryTakeDumpMemListInsertDestMiss` sets PC to `0x800151D4`.

### `TryStopPastRamFn426E0`

- Path: `Core/CeRomTocFiles.cs` ~46150. Called from `Core/HostHardDisk.cs` ~874–876 after the 4266C refuse.
- From source: only after `TryContinuePastRamFn4266C` has logged, PC in `[ListInsert3426E0Fn, ListInsert3426E0End)` = `[0x800426E0, 0x80042774)`, and `$sp` is KSEG0 past guest RAM.
- Comment and constants (~2778–2786, ~46141–46198): stackless leaf. Body starts `move $v0,$a0` (word `0x00801025` named on the 4266C NextFn). `lhu` at `ListInsert3426E0Lhu` `0x800426E4`. `jr` at `0x8004276C`. Refuse leave sets `ra=pc=0x800426E0`. Comment: leaf `addiu $a0,0,42` then `jr $ra` re-enters with `a0=0x2A` and TLBL `bad=0x2A` at `0x800426E4`. First hit logs `past-ram-fn-426e0-stop`, clears EXL, parks PC on `0x800426E0` (does not execute the `lhu`). If `$ra` is inside the leaf, pokes `$ra` back to `0x800426E0`. Comment: do not resume leave-hop past this leaf; sticky skip so the `0x800151D0` dest-miss spin stays unblocked without inventing the next hop. Log text says the `0x800151D0` past-RAM dest-miss already left.

### `StripSideNotes`

- UNKNOWN. No `StripSideNotes` (or `SideNotes`) identifier in the tree. `git log -S StripSideNotes --all` returned no commits.
- Unrelated: `StripRomExt` in `Core/CeRomTocFiles.cs` ~58601 strips a ROM file extension. A `strip=` field exists on the filesys-48d TLBL log (~49423). Neither is this helper.

### List-insert ~`0x800151D0`

- Constant `CoredllDllMainC000Epc` in `Core/CeRomTocFiles.cs` ~1614. Taken by `TryTakeDumpMemListInsertDestMiss` (~13876) when the dump word is `sw $v0,0($a1)` and the dest cannot be peeked. That function calls `TryNotePostApi52ListInsertSkip` then `TryLeavePostApi52ListInsertPastRam`.

### Refuse ~`0x8004266C` / `0x800426E0`

- `TryContinuePastRamFn4266C` in `Core/CeRomTocFiles.cs` ~46061. Called from `Core/HostHardDisk.cs` ~870–872. Comment there: `REFUSE: 4266C framed past-RAM + FFFF* 0xFFFFD885; leave NextFn 426E0.`
- Constants ~2756–2776: `ListInsert34266CFn` `0x8004266C`, frame `0x28`, epi `0x800426C4`, epi `jr` `0x800426D8`, `ListInsert34266CNextFn` `0x800426E0`. Requires prior 423F0 hop (`_postApi52PastRamFn423F0ContLogged`), PC in `[0x8004266C, 0x800426E0)`, `$sp` past-RAM. Sets `$ra` and PC to `0x800426E0`. One-shot sample: `TryNotePastRamAfter4266CLeave` (~46107), log `past-ram-after-4266c-leave`, text says harvest named next stall past `0x800426E0`.
- `0x800426E0` is also `ListInsert3426E0Fn`. The next call in `HostHardDisk.cs` (~874–875) is the STOP helper above, not another refuse leave. Comment: `STOP: stackless 426E0 leaf (self-ra → TLBL bad=0x2A). No leave-hop.`

## 3. Intended next step

UNKNOWN.

No comment or doc in the tree states a future step of a dump-true leave at list-insert when `$a1` is past-RAM so the refuse chain never wins.

What tip comments do state, as current terminal policy on this chain (`TryStopPastRamFn426E0`, `Core/CeRomTocFiles.cs` ~46141–46149):

- STOP at the stackless leaf `0x800426E0`.
- Do not resume leave-hop past that leaf.
- Sticky-skip the leaf while `$sp` stays past-RAM so the `0x800151D0` dest-miss spin stays unblocked without inventing the next hop.

The FA1C past-RAM leave to `0x8003FA34` is already in `TryLeavePostApi52ListInsertPastRam` at this SHA. The 4266C refuse and the 426E0 stop are still called from `Core/HostHardDisk.cs` on later steps. No in-tree comment says that leave disables the refuse chain.

## 4. TODO / FIXME near those helpers

None. `TODO` and `FIXME` do not appear in `Core/CeRomTocFiles.cs` or `Core/HostHardDisk.cs`.

The only `Live next:` line in `Core/CeRomTocFiles.cs` is ~49430, on `MapDdiNopFilesys48dVa` (filesys page `0x48D01000`). That comment is not next to the list-insert leave, the 4266C refuse, or the 426E0 stop.

## 5. Verifier checklist for QA Engineer

- Confirm `dev` and `origin/dev` are `98a7cb066c4d7a3acaccb6a2610278c7444d3534`.
- At `0x800151D0`, after leftover-api52-cont, `$a1` KSEG0 past `0x10000000` and `$ra==0x8003FA1C`: log `post-api52-list-insert-past-ram-leave` with `via=twin-3f9e8-jal-link-nextfn` and `leave=0x8003FA34`. `$ra` other than `0x8003FA1C` / `0x8003F784` / `0x8003F714` does not take that leave; dest-miss sets PC to `0x800151D4`.
- `0x8004266C` refuse only after the 423F0 continue flag, PC in `[0x8004266C, 0x800426E0)`, `$sp` past-RAM: log `past-ram-fn-4266c-refuse`, leave `0x800426E0`.
- `0x800426E0` stop only after the 4266C continue flag, PC in `[0x800426E0, 0x80042774)`, `$sp` past-RAM: first log `past-ram-fn-426e0-stop`, park PC at `0x800426E0`, do not execute `lhu` at `0x800426E4`, no further leave-hop.
- `StripSideNotes` is absent. No `TODO`/`FIXME` beside these helpers. Do not score a missing symbol as present.
