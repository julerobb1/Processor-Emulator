# U-verse / Mediaroom boot status

Last probe: `1ca8383` on `dev`. Same bus as the thin host
(`MediaroomSession` → `NkBinLoader.Load` → `HostHardDisk.TryStep` /
`BinBlkMedia.TryStep`). 85.3M steps, 12M after HookVolume return.

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

The first caller of filter CreateFileW / `\ETC.bin` would be a
client that `nk.bin` never Launchs. The real client lives in dump
`etc.bin` on the Hard Disk volume. Opening that file as volume
content is a firmware path; mapping it as ExtraROM or spawning
`tv2clientce` from the host is not.
