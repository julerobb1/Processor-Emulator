# ExtraROM display/input manifest — verified slots (Phase A)

Read-only. Hashes and hive parse are in `/workspace/extrarom-display-input-boundary.md`. No Boots, no dump writes, no commits.

## Verified inventory (independent TOC parse)

### nk.bin (pTOC 0x802808B4, 32 TOC / 8 FILE, CPU 0x0166)
- TOC[3] bcmiisr.dll
- TOC[5] coredll.dll (ordinal map only)
- TOC[7] gwes.exe — vbase 0x10000, entry 0x63C8
- TOC[20] ceddk.dll
- TOC[25] bcmatapi.dll
- TOC[31] iptvcryptohal.dll — vbase 0x3D90000

### etc.bin (pTOC 0x8134DA84, 80 TOC / 48 FILE, CPU 0x0166)
- TOC[4] waveapi.dll — Tier 1 audio
- TOC[33] ddi_nop.dll — vbase 0x3980000, entry 0x18014; **NULL DISPLAY** (`DrvEnableDriver` only)
- TOC[49] tv2engine.dll — vbase 0x3110000; Mediaroom AV/Gfx API (imports iptvplatform)
- TOC[50] tv2remotekeys.exe — vbase 0x10000, entry 0x4DFC; TCP TV2Message helper, not IR
- TOC[63] bcmuart.dll — COM UART
- TOC[67] iptvhal_97405.dll — this board’s HAL
- TOC[68] iptvhal_100.dll, [69] _200, [72] _400, [73] _500, [75] _510 — keep as sibling variants, not all BCM7405-live
- TOC[76] iptvdriver.dll — stream prefix BTV
- TOC[77] iptvplatform.dll — IPTV_HAL_Gfx_*/Physmem/Display
- FILE[25] tv2clientce.exe — 5120-byte mscoree stub (Launch56)
- FILE[26] tv2clientcorece.dll — compressed 6,398,464
- FILE[47] iptv_hal_common.dll — `iptv_hal_keyhandler` (real), `fake_key_from_hal` (stub)

Loose `dump/` copies of gwes/iptvcryptohal/bcmatapi/bcmiisr remain convenient duplicates; the TOC slots above are authoritative.

## Hive (exact, default.hv unless noted)
- `\System\GDI\Drivers\Display` = `ddi_nop.dll`
- `\init\Launch20` = `device.exe`; `Launch30` = `gwes.exe`; `Launch56` = `\Windows\tv2clientce.exe`
- `\Drivers\OEM\97405\IPTVDriver`: Dll=`IPTVDriver.dll`, Prefix=`BTV`, Sysintr=24, Order=2, Iclass=`{C782F631-87F9-4be4-A97D-AA36786ADC93}`
- `\Drivers\OEM\97405\IPTVHal_97405`: RemoteProtocolVal=4, tuner IRQ 48/50, MaxTuners=0; **no Dll/Prefix**
- No `\Drivers\OEM\97405\RemoteControl`. RCU/IRD keys exist only under OEM 200/500/510.

## Tiers (updated)

### Tier 1 — shared CE / Mediaroom (do not talk to BCM display/IR MMIO directly)
gwes.exe, waveapi.dll, tv2clientce.exe, tv2clientcorece.dll, tv2engine.dll, tv2remotekeys.exe, ddi_nop.dll (explicit **null display**, not a real DDI). tv2engine still *calls* the platform Gfx HAL; it is shared API, not SoC-free at runtime.

### Tier 2 — BCM7405 / platform boundary
iptvdriver.dll (BTV), iptvplatform.dll (Gfx IOCTLs to `BTV1:`), iptvhal_97405.dll, iptv_hal_common.dll, iptvcryptohal.dll, bcmuart/bcmatapi/bcmiisr/bcmscard*, iptvhal_* siblings. Crypto HAL stays in this tier; do not invent keys or decrypted SEC contents.

## Host coverage (unchanged this pass)
MediaroomHostForm still has a PictureBox with no blit. MediaroomSession still sets GuestVideoWrote=false and has no framebuffer/IR device. No new env flag was added.

## Next step
BTV_IOControl and the IR helper are now disassembled (see `/workspace/extrarom-display-input-boundary.md`). Phase B is still blocked: no proven pixel format/channel order, and the 16-byte IR bit→keycode table at 0x1E9C2C8 is zero in the XIP image. No Boot. No host driver until those two facts are recovered.
