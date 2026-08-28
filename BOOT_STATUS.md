# U-verse / Mediaroom boot status

Last probe: `1ca8383` on `dev`. Same bus as the thin host
(`MediaroomSession` → `NkBinLoader.Load` → `HostHardDisk.TryStep` /
`BinBlkMedia.TryStep`). ~49M steps to CreateProcess(device.exe)
return. Later notes live on this PR (and the next open PR into
`dev`), not on merged #25.

This file is the source of truth for what that boot actually did.
Do not treat FEATURE_NOTES splash-screen ideas, Copilot “mount FAT
and the client starts” stories, or a host window as success.

## What success is

A running U-verse / Mediaroom UI from the real `tv2clientce` in the
user dump (`etc.bin` on the Hard Disk volume).

Not success: host chrome, a black pane, OEMIdle `0x80059E98`, a boot
log, or a synthetic Launch.

## What the host is

`MediaroomHostForm` is a thin Win7 frame (dump path, Boot, Stop, a
black `PictureBox`). 900×640 is chrome, not guest video. The CPU
thread calls `MipsCpuEmulator.Step` with no step cap. OEMIdle is
idle, not halt. Do not skip the memset at `0x80014200`–`0x8001420C`.

Hunt / OfferFeed may keep a local dump map. Shipped hunt is
user-pointed. Do not commit dump assets (`etc.bin`, `sec.bin`,
PlayReady, the FAT tree).

## What this boot already did

1. BINBlk media, `BLOCK_DRIVER` notify, `CreateFile` `\StoreMgr\BINBlk`.
2. HDProf / HDProfile, FAT image named **Hard Disk**, `DISK_READ`
   (LBA 0, 63, 64, 80), `mspart` / `fatfsd.dll`.
3. After the first FAT read, hive keys that BINFS blanked are served
   again: `Folder=Hard Disk`, `Dll=fatfsd.dll`.
4. Filters enum: index 0 = `sigcheckfilter`, index 1 =
   `ERROR_NO_MORE_ITEMS`. `integritycheckfilter` is not a second
   Filters child. Do not add BHC / EFS / MyFilter.
5. `CreateFile` `\Windows\sigcheckfilter.dll` (ROM TOC). HookVolume
   `0x03DF22D0` from FSDMGR `0x03E8549C`.
6. `FSDMGR_DiskIoControl` `0x71C20` returned `"Hard Disk"`.
   IsTargetVolume `0x03DF2178` took the success path `0x03DF2338`
   (`[FILTER_HookVolume] This is the target volume??`).
7. HookVolume returned `v0=0x0008AD50` (filter object). It then
   touches HKLM `...\HDProfile\FATFS\Filters\MyFilter`. It does
   **not** `CreateFile` `\ETC.bin`.
8. `0x03DF20C8` is PARTINFO FindFirst/FindNext (cbSize 296), called
   from IsTargetVolume. It is not a file walk of the volume.

## What this boot did not do

After HookVolume, CreateFile names were only:

- `\StoreMgr\BINBlk` (already served)
- `\windows\initdb.ini`

No `\ETC.bin`, no `\Hard Disk\...`, no `BOOT.PRF`, no
`\Hard Disk\NK.bin`. Filter CreateFileW `0x03DF1ADC` = 0 hits.
`\ETC.bin` at `0x03DF124C` never appeared in a0/a1/a2. The
CreateFileW sites that use it (`0x03DF1F9C` / `0x03DF2010` /
`0x03DF2090`) never ran. Those sites are a later signature check
(`Signature check failed for "%s"`), not mount.

`binblk.dll` `.text` never ran. BINBlk IOCTLs were `0x1` / `0x71800`
/ `0x71C24` / `0x71C00` / `0x71FC4`, not `0x80090006`.

`nk.bin` has no post-FSReady Launch of `tv2clientce` / `udevice` /
Launch50–20. `initobj.dat` Launch is `device.exe` only. RunApps:
`HKLM\init` → `ERROR_BADKEY` → skip. ExtraROMReady `0x00014234` is
a filesys `CreateEvent` during FILESentry, not a second XIP map.
Hunt may log `etc.bin`; the host does not map it.

After that, the CPU sits in OEMIdle `0x80059E98`.

## After initdb.ini (same `1ca8383` boot)

9. `initdb.ini` is a ROM FILESentry, 1239 bytes, binary CEDB.
   HANDLE-USE=0. Hive-init helper `\Windows\boot.hv` `a3=3` →
   `v0=0x00080250`. `system.hv` is never attempted. `default.hv`
   FILESentry (65188 bytes at `0x802FA8AC`) never ran; packed jal
   `0x8011ECB0` skipped.
10. Flags=3 is a real boot-hive `init\BootVars` DWORD from the
    internal path query (`0x000218AC` / `0x0001EC8C`), not Win32
    `RegOpenKeyEx`. `Start DevMgr` on that path is
    `ERROR_FILE_NOT_FOUND`.
11. RunApps `0x00017BA4` `RegOpenKeyEx` `HKLM` + `"init"` →
    `ERROR_BADKEY`. That is the missing default-hive `Launch` key,
    not a hive unload. Host did not write `Launch`.
12. CreateProcess IAT `jal 0x0004BCA4` / kernel `0x80034D2C`
    `a0=0x00011970` (`device.exe`) `a1=NULL` → `v0=0` last-error 14.
    Earlier `v0=1` was `filesys.exe` only. `device.exe` is TOC[8]
    (e32 `0x80073DBC`, vbase `0x00010000`, `e32+0x14=0x4000`).
    This CreateProcess does not CreateFile the image.

## CreateProcess(device.exe) OOM

Image map succeeds: `VALLOC 0x00010000` size `0x4000` →
`v0=0x00010000` (ra `0x8001F8F0`). Slot is `0x06000000` (next
32MB after filesys `0x04000000`). PROCESS object `0x86F71A48`
is live.

Fail is inherit-list reserve, not stack `0x8001B9EC` and not
o32/TOC:

1. `0x8001F8FC` `jal 0x8001B644(a0=0x06000000)`.
2. `jal 0x800283FC` at `0x8001B724` (ra `0x8001B72C`)
   `a0=0x39FC0000` `a1=0xCC045800` `a2=0x01002000` → `v0=0`.
3. Inside `0x80027B50` at `0x80027C10`: size ≥ 32MB and addr ≠ 0
   → `0x800283B8` last-error 87. CreateProcess then stores 14.

`0x39FC0000` = slot `0x06000000` + pair start `0x33FC0000`.
Size `0xCC045800` = `0x00005800 - 0x33FC0000` (unsigned wrap).

Who wrote LIST `@86F715A8`: binfs, then kernel memcpy. Not an
unzeroed AllocObj, not o32+8. `0x8001687C` AllocObj type 1,
`memcpy` `0x80058B24` of 0x24 bytes from `0x00083800`. Walker
count is list+8 = 3 (the `0x11` at `0x803429E0` is not that
count). binfs `0x03EA2B6C` does `start=(entry+0x14)<<16`,
`end=entry+0x18`.

| pair | start | end | source |
|---|---|---|---|
| 0 | `0x33FC0000` | `0x00005800` | chain `0x81360000` / `0x50000` type 2; +14 leftover `0x03E833FC`, +18 `0x00005800` |
| 1 | `0x33FC0000` | `0x00005800` | chain `0x80630000` / `0xD30000` type 1; same leftovers |
| 2 | `0x01FB0000` | `0x02000000` | chain `0x80010000` / `0x310000` type 0; ROMHDR `0x01FB01FB` / `0x02000000` |

Chain table is `0x8006B9DC`. Pair 2 is the sane dll/stack page
range. Pairs 0–1 are ExtraROM bases from that table. This
environment’s dump has no B000FF whose `imageStart`/`imageLength`
is `0x80630000`/`0xD30000` or `0x81360000`/`0x50000`. `etc.bin` /
`sec.bin` / `raven_fw.bin` are not present as real B000FF files
here (hunt placeholders only). Do not invent ExtraROM to back
those VAs. Firmware does not LoadRom them for this inherit
publish; it walks the in-memory chain.

## What not to invent

Do not `CreateProcess(tv2clientce)`. Do not `SetEvent` the filesys
work pump (`0x66FACF6A`, `0xE6F888F2`, `0xE6F88752`,
`SYSTEM/FSReady`). Do not fake pixels, ExtraROM/XIP,
`CreateStaticMapping` of `etc.bin`, IOCTL `0x80090006`, a second
filter, `MountLabel`, explorer/gwes/shell, implicit-API stubs
(`0xFFFFFAC2` / `0xFFFFABDE` / `0xFFFFDFCA` / `0xFFFFDBFA` /
`0xFFFFDC02`), or a host “Launch”.

Empty `.github/workflows/ci.yml` 0s/no-jobs red is an empty
workflow. Leave it unless a real compile job fails.

## Next real question

CreateProcess(`device.exe`) returns OOM because binfs published
two unsatisfiable inherit pairs from ROM-chain ExtraROM slots
that this dump’s loaded `nk.bin` does not back. A dump B000FF
that already names those bases/sizes would be a host load of
existing records (same as `nk.bin`), not an invented XIP and
not a host `CreateProcess` of the client. This environment has
no such file. Do not invent ExtraROM. Do not fake VirtualAlloc
or CreateProcess success. Do not write `HKLM\init` Launch.
