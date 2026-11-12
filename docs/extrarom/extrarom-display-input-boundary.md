# ExtraROM display/input boundary (Phase A + Phase B)

Static analysis plus a gated, off-by-default host path. No Boots, no writes to D:\UverseFirmware, no invented framebuffer contents or key names, no host CreateProcess(tv2clientce). Work was from `C:\Users\juler\agent-tools\extrarom\` copies and box extracts only.

Updated 2026-11-11 14:05 CST (America/Chicago).

## Copies and hashes

Source: `C:\Users\juler\agent-tools\extrarom\` on DESKTOP-PGN06F7. Box copies under `/workspace/extrarom-dll-extract/`. SHA256 matches the source and the expected prefixes.

| File | Bytes | SHA256 |
|---|---:|---|
| etc.bin | 13,749,339 | 4088F8DEFB4B74BAE646737AF5F8699D09D3BBE6537EBE67C48EF22D8642E74B |
| nk.bin | 3,126,079 | 1B323FD6C9C55EDD916450F75349E24C855261C5671AE9B200B2C08ABDC22B60 |
| boot.hv | 53,248 | E2A2FE0C87779A95AA8E6C3B63032EB67670787F40527CE0A51A5FFB3D31C2A2 |
| default.hv | 266,240 | 9B6E82D2023D032CA36C350A859EAF8C3CCC270EC0DD1B81EE651E816D2BBBE0 |

Hives are CE object-store “MIKE”/EKIM heaps (not NT `regf`). Keys were walked from size+type `0xC000`/`0xD000` records and last-child / prev-sibling links. Paths below are exact key paths relative to the hive root (these hives store HKLM-style trees as roots such as `\Drivers`, `\init`, `\System`).

Extracted modules (LZX CEDecompressROM, 3-byte uncompressed size as block-count prefix, 4 KB blocks, window=16) live in `/workspace/extrarom-dll-extract/modules/`. Originals on PGN were not modified.

### 97405 data-section extract bug (why 0x1E9C2C8 looked like zeros)

iptvhal_97405 data is at realaddr **0x1E9C000** / rva **0x19000**, vsize 0xB80. The on-disk section is raw-compressed (psize **0x206**). An earlier extract left the compressed stream zero-padded in place (first16 `39070006…`, last nonzero at 0x205), so offset 0x2C8 (past 0x206) read as zeros.

Correct CEDecompressROM: 3-byte size **0x739**, n=1849, first16 `6cb601004cbc0100…`, 325 nonzero, last nonzero at 0x738. The bad extract was backed up as `/workspace/extrarom-dll-extract/modules/iptvhal_97405.dll.bad-compressed-data.bak` and the module was rewritten (114688 bytes). BSS flags/handles at 0x1E9C760+ sit past 0x739 and are correctly zero. Text and rdata were already valid (`same_as_old=True`).

Address formation in this module is `lui 0x1EA` + signed `addiu` (not `lui 0x1E9C`). Capstone `disasm()` on a whole section stops at the first invalid word; walks must step 4 bytes at a time.

## Verified TOC / FILE slots

nk.bin: pTOC=0x802808B4, 32 modules, 8 files, usCPU=0x0166, phys 0x80010000–0x8031B3BC.

| Slot | Name | Notes |
|---|---|---|
| TOC[3] | bcmiisr.dll | ISR helper |
| TOC[5] | coredll.dll | vbase 0x3F50000; DeviceIoControl export rva 0x280E4, va **0x3F780E4** |
| TOC[7] | gwes.exe | vbase 0x10000, entry 0x63C8; one compressed data section |
| TOC[20] | ceddk.dll | |
| TOC[25] | bcmatapi.dll | |
| TOC[31] | iptvcryptohal.dll | vbase 0x3D90000; crypto HAL (not display/IR) |

etc.bin: pTOC=0x8134DA84, 80 modules, 48 files, usCPU=0x0166, phys 0x80630000–0x8134EA18.

| Slot | Name | Notes |
|---|---|---|
| TOC[4] | waveapi.dll | audio (`\Drivers\BuiltIn\WAPIMAN`, prefix WAM) |
| TOC[33] | ddi_nop.dll | vbase 0x3980000, entry 0x18014; export `DrvEnableDriver` only |
| TOC[49] | tv2engine.dll | vbase 0x3110000; imports Gfx APIs from iptvplatform |
| TOC[50] | tv2remotekeys.exe | vbase 0x10000, entry 0x4DFC; no exports |
| TOC[63] | bcmuart.dll | stream prefix COM |
| TOC[67] | iptvhal_97405.dll | vbase 0x2EB0000, entry 0x143B4; data 0x1E9C000 |
| TOC[68] | iptvhal_100.dll | sibling HAL, not loaded on 97405 |
| TOC[69] | iptvhal_200.dll | |
| TOC[72] | iptvhal_400.dll | |
| TOC[73] | iptvhal_500.dll | used by OEM\500 RemoteControl (RCU) |
| TOC[75] | iptvhal_510.dll | used by OEM\510 RemoteControl (RCU) |
| TOC[76] | iptvdriver.dll | vbase 0x27B0000; stream exports `BTV_*` |
| TOC[77] | iptvplatform.dll | vbase 0x27A0000; `IPTV_HAL_Gfx_*` / Physmem / Display |
| FILE[25] | tv2clientce.exe | 5120-byte MZ, machine 0x14C, import only `_CorExeMain` (mscoree). Managed stub; depends on TV2ClientCoreCE. |
| FILE[26] | tv2clientcorece.dll | 6,398,464 real / 2,612,926 comp |
| FILE[47] | iptv_hal_common.dll | 7168-byte MZ, machine 0x0166; only `fake_key_from_hal` / `iptv_hal_keyhandler` |

FILE table has no keymap file. Sibling HALs 500/510 contain `KEY_` / `RcKeyMapping` / RCU UTF-16 name lists; those names must not be used for 97405 extended codes.

## Hive values (exact)

### Display (GDI stub, not the Mediaroom pixel path)

`\System\GDI\Drivers`
- Display (SZ) = `ddi_nop.dll`

This matches ExtraROM’s already-loaded stub DDI. GWES can come up with a black screen; real pixels do not go through `ddi_nop`.

### Init launch

`\init`
- Launch20 (SZ) = `device.exe`
- Launch30 (SZ) = `gwes.exe` ; Depend30 (BINARY) = `1400`
- Launch50 (SZ) = `sipcfg.exe` ; Depend50 = `1400`
- Launch53 (SZ) = `RunOnce.exe` ; Depend53 = `14001e00`
- Launch56 (SZ) = `\Windows\tv2clientce.exe` ; Depend56 = `14001e003500`
- Launch95 (SZ) = `dw.exe` ; Depend95 = `1400`

`\init\BootVars` (default.hv and boot.hv)
- Flags (DWORD) = 3
- ProfileDir (SZ) = `\Documents and Settings\user.hv`
- SystemHive (SZ) = `\Documents and Settings\system.hv`

No Launch* entry starts `tv2remotekeys.exe`.

### Built-in / UART (not IR)

`\Drivers` RootKey (SZ) = `Drivers\BuiltIn`
`\Drivers\BuiltIn` Dll=`BusEnum.dll`, BusName=`BuiltIn`

`\Drivers\BuiltIn\Serial1` / `Serial2`: Dll=`bcmuart.dll`, Prefix=`COM`, IoBase=0x10400B40 / 0x10400B80, IoLen=32.

### BCM97405 IPTV / remote (this dump’s chip)

`\Drivers\OEM\97405\IPTVDriver`
- Dll (SZ) = `IPTVDriver.dll`
- Prefix (SZ) = `BTV`
- Sysintr (DWORD) = 24
- Order (DWORD) = 2
- Priority256 (DWORD) = 64
- Iclass (SZ) = `{C782F631-87F9-4be4-A97D-AA36786ADC93}`

`\Drivers\OEM\97405\IPTVHal_97405`
- Tuner0Interrupt=48, Tuner0Slot=2, Tuner1Interrupt=50, Tuner1Slot=3
- MaxTuners (DWORD) = 0
- RemoteProtocolVal (DWORD) = 4
- **No Dll, Prefix, Order, or IRQ/MemBase** on this key. The 97405 HAL is not a stream device in the hive.
- **No binary keymap**, no RemoteControl/RCU/IRD key (those exist on OEM\200/500/510 only).

There is **no** `\Drivers\OEM\97405\RemoteControl` key.

Sibling platforms (not this board, recorded for contrast only):
- `\Drivers\OEM\500\RemoteControl` and `\Drivers\OEM\510\RemoteControl`: Dll=`iptvhal_500/510.dll`, Prefix=`RCU`, Order=22, IstPriority=165, DebugMask=2
- `\Drivers\OEM\200\Remote`: Dll=`iptvhal_200.dll`, Prefix=`IRD`, Index=1, RemoteProtocolVal=22

boot.hv has the OEM\97405\BCMFMD flash driver but not the IPTVDriver/HAL/RemoteControl values (those are in default.hv).

## Display contract (dump-grounded)

e32 units on this image start at +0x24 after a PE timestamp at +0x20 (`e32_subsys=9`). iptvdriver exports (`BTV_IOControl` rva 0x31A7BC, va 0x2ACA7BC) and iptvplatform Gfx wrappers were re-parsed with that layout.

`BTV_IOControl` (va 0x2ACA7BC) is a logging wrapper. The real dispatcher is va 0x2ACC01C (rva 0x31C01C):

- index = `dwCode + 0x7FFCE000` = `dwCode - 0x80032000`
- first jump table at va 0x2ACC0C8 is only a timing filter
- real switch at va 0x2ACC3D8, second jump table at va **0x2ACC434** (signed halfword offsets, not DWORD function pointers), 0x131 entries (default va 0x2AD14EC)

Verified case handlers (each is unique):

| IOCTL | API | Handler va / rva | In checks | Implementation |
|---|---|---|---|---|
| 0x80032024 | GetFrameBufferBackbuffer | 0x2ACCA40 / 0x31CA40 | pBufIn>=4, pBufOut>=4 | va 0x2ADD7F8 → 0x2ADD6F8(out, 1) |
| 0x80032028 | UpdateFrameBuffer | 0x2ACCAB0 / 0x31CAB0 | pBufIn>=8, pBufOut>=4 | va 0x2AE2F6C → 0x2AE2D94(ptr, flag, 0) |
| 0x80032050 | GetVidMem | 0x2ACCFB8 / 0x31CFB8 | in/out >=4 | va 0x2ADCEF8; returns size (`end-start`) from Nexus thunk 0x2B33B1C |
| 0x80032064 | GetDesktopSurface | 0x2ACD258 / 0x31D258 | in/out >=4 | va 0x2AE1EC4 |
| 0x80032068 | Display_Init2 | 0x2ACD2C8 / 0x31D2C8 | in>=0x18, out>=4 | va 0x2AE7100 |
| 0x8003206C | Display_SetMode2 | 0x2ACD364 / 0x31D364 | in>=0x18, out>=4 | va 0x2AE67A8; status written to pBufOut |
| 0x80032070 | Display_GetMode2 | 0x2ACD3F4 / 0x31D3F4 | in>=0x18, out>=4 | va 0x2ADF664 |
| 0x80032084 | Gfx_Init | 0x2ACD6C8 / 0x31D6C8 | in>=0x10, out>=4 | va 0x2AE7278 |
| 0x8003201C | LockBitmapBuffer | 0x2ACC954 / 0x31C954 | in>=0xC, out>=4 | va 0x2ADD610 |
| 0x80032020 | UnlockBitmapBuffer | 0x2ACC9DC / 0x31C9DC | in>=4 | (paired unlock) |

iptvplatform handle is `CreateFileW("BTV1:")` stored at **0x1DF205C**; `DeviceIoControl` IAT at **0x1DF2024**. Platform wrappers put the caller’s first argument into the 4-byte IN buffer (it is an **out-pointer**): the driver does `MapCallerPtr(*(DWORD*)pBufIn, 4)` and writes through that pointer. Status comes back via pBufOut / pdwActualOut.

Kernel Win32CreateFile is at **0x8003D700** (already hooked). coredll `DeviceIoControl` is at **0x3F780E4** (rva 0x280E4, export ord 179). At entry: a0=hDevice, a1=dwIoControlCode, a2=lpInBuffer, a3=nInBufferSize; 5th+ args at sp+0x10 (lpOutBuffer), +0x14 (nOutBufferSize), +0x18 (lpBytesReturned). Kernel neighbors 0x8003D958 and 0x8003DBEC were not fully named; the implemented hook is the coredll export (the IAT path).

### How the backbuffer is allocated (not a static phys/MMIO FB)

There is **no fixed framebuffer address** in the dump. Graphics state lives in a context at **0x1DF7368** (mutex at 0x1DF7890).

GetFrameBufferBackbuffer (va 0x2ADD6F8), with a1=1:

- current surface id at `ctx+0x6C` (0x1DF73D4)
- surface A id `ctx+0x78` (0x1DF73E0) → wrapper pointer `ctx+0x5BC` (**0x1DF7924**)
- surface B id `ctx+0x74` (0x1DF73DC) → wrapper pointer `ctx+0x5C0` (**0x1DF7928**)
- writes the matching **wrapper object pointer** (not a raw pixel VA) through the caller out-pointer

The wrapper is a **0x7C-byte** heap object (alloc va 0x2AFAF3C): word0 is a lock flag, word1 (`+4`) is a Nexus surface handle. `0x2AEC1A8` reads `wrapper+4`; `0x2AEC1C8` writes the handle into `wrapper+4`. Surfaces are created by the thunk at va 0x2B338E4 (source string `nexus_surface_thunks.c`) and torn down at va 0x2B33954. Pointer slots are filled around va 0x2ADE074 / 0x2ADE09C.

UpdateFrameBuffer (va 0x2AE2D94) looks up the handle from the wrapper, compares it to the A/B surfaces, flips `ctx+0x6C`, and issues Nexus present/blit calls (including va 0x2B2FF84 begin/end and 0x2B304A4). This is the “guest finished writing, show the frame” IOCTL.

LockBitmapBuffer (va 0x2ADD610) is how pixels are actually addressed:

- platform in-struct (0xC): `+0` wrapper, `+4` `void**` pixel out, `+8` `uint16*` stride out
- `NEXUS_Surface_GetMemory` thunk va 0x2B33A3C fills `{buffer, pitch}` at sp+0x18 / sp+0x1C
- writes the **CPU pixel pointer** to *out1 and the **uint16 stride/pitch** to *out2
- sets wrapper word0 = 1 (locked)

### Format id 0x13 = BPXL_eA8_R8_G8_B8 (32 bpp)

GetDesktopSurface (va 0x2AE1EC4) writes settings+0 = **0x13**, +8 = **0x358** (856), +0xA = **0x1E0** (480), then jals 0x2B338E4 (create).

iptvdriver function 0x2B0C5D8: if format id < 0x47, `*a1 = *(0x27BB084 + id*12)`. 12-byte records start at va **0x27BB080** / rva 0xB080 (word0 bpp/flags, word1 packed BPXL descriptor, word2 = id). Format **0x13** has word0 = **0x20010001** (32 bpp, flags 0x010001) and word1 = **0x07E48888**.

BPXL `{desc, name}` table starts at rva **0x115EF8** (va 0x28C5EF8). Parsing `{name, desc}` from 0x115EFC is off-by-one and must not be used. Descriptor 0x07E48888 uniquely matches **BPXL_eA8_R8_G8_B8** (no collisions).

Channel-order corroboration only (not the numeric enum value): rdkcmf/westeros `westeros-render-nexus.cpp` treats `eA8_R8_G8_B8` as ARGB in a register (A MSB, B LSB), which is little-endian memory **B,G,R,A**. The value **0x13** is dump-internal, not taken from that file. WinForms `Format32bppArgb` is the same LE memory layout.

### SetMode2 mode ids 8, 9, 0xB, 0xC, 0xD

SetMode2 (va 0x2AE67A8) compares the DWORD at pBufIn+0 to **8, 9, 0xB, 0xC, 0xD**. Lookup 0x2AEC524 walks 21 records of 0x14 bytes at va **0x27B86A8** / rva 0x86A8:

- modes 8 and 0xB: field +8 = **0x1C**
- modes 9 and 0xC: field +8 = **0x1D**
- mode 0xD: field +8 = **0x1E**, field +4 = 9

0x2AEC644 remaps 1 or 4 → 0x18, 0xD → 0x19, else passthrough (so 0x1C/0x1D/0x1E stay).

BFMT_VideoFmt records are 0x50 bytes, name at +0x4C, start rva **0x112230**. Array-index names:

- 0x1C = `BFMT_VideoFmt_e720p_60Hz_3DOU_AS` (1280-class)
- 0x1D = `BFMT_VideoFmt_e720p_50Hz_3DOU_AS`
- 0x1E = `e480p` (720x480, w2=0x2D0); word1 of that record is official id **0x1A**

A 6-entry map also says official id 0x1A = e480p, so **array-index vs official-id for field +8 is not fully proven**. Do not over-claim the 3D 720p names until 0x2ADECA8 is shown to index the BFMT table with those values. SetMode2 also passes width/height as uint16 at pBufIn +4/+6 of the 0x18 struct; the host uses those guest-supplied dimensions (default 856×480 if unset).

A 0x30-stride short-name table at rva 0x116E14 (PowerUp, 640x480p, 1080i, 480p, …) only goes to ~index 0x13 and is not the 0x1C/0x1D/0x1E map.

### Init2 / SetMode2 input structs (0x18 bytes)

From the IOCTL case + iptvplatform wrappers:

Display_Init2 / SetMode2 (pBufIn 0x18):

- `+0x00` DWORD mode/id (SetMode2 compares this to **8, 9, 0xB, 0xC, 0xD**)
- `+0x04` uint16 (width; platform `sh a1`)
- `+0x06` uint16 (height; platform `sh a2`)
- `+0x08` DWORD
- `+0x0C` byte/ptr field (Init2 maps 1 byte; SetMode2 also has a DWORD here)
- `+0x10` DWORD or mapped 4-byte ptr
- `+0x14` Init2: mapped 4-byte ptr; SetMode2: a **flag byte**

Gfx_Init (0x10): DWORD at +0, uint16 +4, uint16 +6, DWORD +8, byte at +0xC.

GetVidMem returns a **size** (range difference), not a framebuffer pointer.

GetMode2 out layout is proven only for the same +0 mode DWORD and +4/+6 uint16 width/height; later fields were not invented in the host.

## Input contract (dump-grounded)

### IPTV_HAL_Global_Init and RemoteProtocolVal

`IPTV_HAL_Global_Init` va **0x2EC41F8** (rva 0x141F8) defaults RemoteProtocolVal to **4** at **0x1E9C084** (now visibly `04 00 00 00` in initialized data), then `RegCreateKeyExW("Drivers\\OEM\\97405\\IPTVDriver")` + `RegQueryValueExW("RemoteProtocolVal")`. Protocol select:

- 0x16 → table **0x1E9C088** (starts ASCII `0123456789` plus extended codes; 40/256 nonzero)
- 0x17 → table **0x1E9C188** (34 nonzero)
- else, including hive default **4** → table **0x1E9C288**

Pointer stored at **0x1E9C774** (`sw $v0, 8($s7)` at 0x2EC430C, s7=0x1E9C76C). Init also starts the MMIO path: 0x2EC6804 with callback 0x2EC3BD4, handle stored at 0x1E9C77C.

There are **no stores** into 0x1E9C088–0x1E9C388 in 97405; the tables are XIP-initialized, not runtime-filled. Hive has no binary keymap.

### Path A — MMIO bit table (implemented host path)

iptvhal_97405 helper **va 0x2EC68F8** (rva 0x168F8) is **not** a message queue. It polls BCM MMIO:

```
lw  [0xB0400140]
lw  [0xB0400144]
halfword = low16( (reg140 << 8) | reg144 )
event[0] = 0
event[2] = halfword
*count = 1
```

The same block is initialized / armed at va 0x2EC6810: writes **0x40** to **0xB0400168**, also uses **0xB0400174**, and later **0xB0400148, 0xB0400150, 0xB0400154, 0xB040016C**, plus a second cluster at **0xB0404128 / 412C / 4130**. There is no InterruptInitialize on this read path (pure MMIO poll). Tuner hive IRQs 48/50 are not this path. 0x1E9C2C8 is only read at va **0x2EC3C38**.

Caller va 0x2EC3BD4 (the path that jals 0x2EC68F8 then iptv_hal_keyhandler):

- special halfwords **0xDFFB** and **0xEFFB** set flags at 0x1E9C760 / 0x1E9C764
- then bit-walks the same halfword, bits **0..15**
- **a 0-bit is treated as pressed** (`bnez` skips keyhandler when the bit is set; idle is all-ones)
- for each 0-bit, `keycode = *(uint8*)(0x1E9C2C8 + bit)` packed into the `iptv_hal_keyhandler` argument (byte2 = VK-style code; byte0 flags: bit7 press/release)
- sink is `iptv_hal_keyhandler` → `GetForegroundWindow` + `PostMessageW(WM_KEYDOWN 0x100 / WM_KEYUP 0x101 / WM_CHAR 0x102)`

Recovered 16-byte table at **0x1E9C2C8** (also default proto table +0x40):

```
E9 28 9E 00 25 27 9D 00 26 AF 99 00 2B 12 AE 00
```

| bit | byte | standard VK (only where the byte is a known Win32 VK) |
|---:|---|---|
| 0 | 0xE9 | unnamed in 97405 (do not invent) |
| 1 | 0x28 | VK_DOWN |
| 2 | 0x9E | unnamed |
| 3 | 0x00 | unused |
| 4 | 0x25 | VK_LEFT |
| 5 | 0x27 | VK_RIGHT |
| 6 | 0x9D | unnamed |
| 7 | 0x00 | unused |
| 8 | 0x26 | VK_UP |
| 9 | 0xAF | unnamed |
| 10 | 0x99 | unnamed |
| 11 | 0x00 | unused |
| 12 | 0x2B | VK_EXECUTE |
| 13 | 0x12 | VK_MENU (Alt) |
| 14 | 0xAE | unnamed |
| 15 | 0x00 | unused |

0x1E9C2E0 is a MaxTuners registry byte (strings at 0x2EB1050 / 0x2EB1064), not a key table slot. 0x1E9C2E4 is a hardware config template used at 0x2EC6AF8. Bytes from ~0x1E9C2D8/0x50 into the default proto table overlap other initialized structs.

### Path B — protocol table (not the implemented host path)

Second path va **0x2EC3CF8**, helper **0x2EC69C4**: `keycode = *(uint8*)(table_at_0x1E9C774 + (event+4 & 0xFF))`, then keyhandler press followed by release (OR 0x80). Debounce 0x97 ms via GetTickCount; address arithmetic with 0x4FBFFA00 / 0x4FBFF9E4. RemoteProtocolVal=4 selects the 0x100-byte table at 0x1E9C288 (starts `0123456789` then E9 9E 9D AF AE AD …). That table does **not** write 0x1E9C2C8; it overlaps the 16-byte map at +0x40 because they share initialized data. 0x2EC6F94 memcpy’s 0x118 bytes of config (not a 16-byte keymap) into 0x1E9C7E0+index*0x130 and programs 0x4FBFF9E0 / 0x4FBFFA00-relative MMIO.

`IPTV_HAL_Input_GetKey` / `RegisterKeyHandler` (rva 0x141E8) and `fake_key_from_hal` remain stubs. `tv2remotekeys.exe` remains a TCP TV2Message helper, not this MMIO path.

## Phase B (implemented, gated, off by default)

Both Goal 1 (16 IR keycodes) and Goal 2 (format 0x13 + mode ids with the BFMT caveat) are dump-grounded enough for a host path that does not invent pixels or key names.

Backup (before edits): `C:\Users\juler\Documents\GitHub\Processor-Emulator\_backup_phaseB_20261111\` (MediaroomHostForm.cs, MediaroomSession.cs, MipsBus.cs, MipsUverseEmulator.cs, ProcessorEmulator.csproj, IBusDevice.cs). The backup folder is excluded from compile in the csproj so it cannot collide with live sources.

Flag: **`EXTRAROM_BTV=1`** (also accepts `true`). Unset / any other value keeps the previous behavior: `CreateFile("BTV1:")` still returns `INVALID_HANDLE` (0xFFFFFFFF), no IR MMIO device, no PictureBox blit, `GuestVideoWrote=false`.

When the flag is on:

- `CreateFile("BTV1:")` at kernel 0x8003D700 / `TryMissMissingDevice` returns distinctive handle **0xB7100001**.
- `DeviceIoControl` is hooked at coredll **0x3F780E4** for that handle and answers the real IOCTL codes above.
- Nexus surface is host-reserved guest RAM at **0x8F000000**, wrapper at **0x8EFFFF80** (0x7C bytes: word0 lock flag, +4 surface VA). Default 856×480, 32 bpp A8R8G8B8, stride = width×4. This VA is inside the existing 256 MB kseg0 window (0x80000000–0x8FFFFFFF), below SP **0x8FFFF000**. It is a reserved host allocation, not a dump-specified phys/MMIO framebuffer.
- PictureBox is updated only on **UpdateFrameBuffer**, from guest-written pixels already in that surface (LE B,G,R,A). `GuestVideoWrote` is set only then. No invented pixels.
- IR window **0xB0400000**, size 0x5000 (covers 0xB0400140/0144 and the init cluster through 0xB0404130). 0140/0144: idle bits are 1, 0-bit = pressed, packed halfword `(reg140<<8)|reg144`. Other offsets in the window are sticky.
- Host KeyDown/KeyUp maps a WinForms `KeyValue` onto a bit only when it equals a table byte (arrows 0x25–0x28, Alt 0x12, Execute 0x2B if the host actually sends that VK). Extended bytes 0xE9/0x9E/0x9D/0xAF/0x99/0xAE have no invented names or F-key aliases.

### Files, build, commit

Repo: `C:\Users\juler\Documents\GitHub\Processor-Emulator`, branch **dev**, local only, **not pushed**.

| Path | Change |
|---|---|
| `ExtraRomBtv.cs` | new: flag, BTV IOCTL handlers, surface, IR MMIO device, key bit table |
| `Core/CeRomTocFiles.cs` | `TryMissMissingDevice` returns 0xB7100001 when the flag is on |
| `MipsCpuEmulator.cs` | hook PC == 0x3F780E4 |
| `MediaroomSession.cs` | add `ExtraRomIrMmio` when flag on; `MarkGuestVideoWrote()` |
| `MediaroomHostForm.cs` | KeyPreview, blit on `FrameReady`, keyboard → `SetKey` |
| `ProcessorEmulator.csproj` | `Compile Remove` for `_backup_phaseB_20261111\**` |

- Build: `dotnet build ProcessorEmulator.csproj` — **succeeded, 0 errors, 92 warnings** (existing nullable noise in CeRomTocFiles/HostHardDisk; none from ExtraRomBtv.cs).
- Commit: **3695dcc** (`3695dcc65a1582f70f100c8807c80e490e45d94d`), message starts `EXTRAROM: Phase B BTV1 display + 97405 IR MMIO (EXTRAROM_BTV, off by default)`. Branch is **ahead 84** of origin/dev.
- Not committed / not touched: `PaceHogSession.cs` (still dirty, pre-existing), untracked `UverseDriveE/raven_fw.bin`, the backup folder.

## What remains

- No Boot this turn: the host path is dump-grounded and compiles, but it has not been runtime-proven against a live guest Gfx/IR sequence.
- Extended 97405 keycodes 0xE9, 0x9E, 0x9D, 0xAF, 0x99, 0xAE still have no dump-internal names. Sibling RCU/IRD lists must not be used to name them.
- Protocol path 0x2EC3CF8 / table 0x1E9C288 is a real alternative 256-byte lookup when RemoteProtocolVal selects it; the host keyboard drives only the MMIO bit path.
- SetMode2 field +8 = 0x1C/0x1D/0x1E vs BFMT official-id (0x1A for e480p) is still ambiguous. Host uses guest width/height from the 0x18 struct.
- GetMode2 fields past +0/+4/+6 are not fully proven (host writes zeros there).
- GetVidMem on the host returns the surface byte count as the only size this allocation can honestly report; the real driver returns Nexus vidmem end-start.
- Kernel DeviceIoControl PC (candidates 0x8003D958 / 0x8003DBEC) was not named. A call that bypasses coredll 0x3F780E4 would miss the hook.
- Surface VA 0x8F000000 is reserved by the host, not recovered from the dump (the dump has no static framebuffer).
