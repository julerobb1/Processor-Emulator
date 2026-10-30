# ARMv7-A (Cortex-A15) core for AX014AN (Arris XG1v4, BCM7439B0)

Goal: real AX014 kernel to earlyprintk, then initramfs `/init`. Not RDK services.
Nothing here is wired into App yet (flag `ARMV7_A15`, off).

## What the firmware gives us (box analysis, 2026-10-30)
- Image: PKCS#7 (0x574) + Arris PACK. KERN at file 0x5DEDF; zImage at KERN+0x1BC
  (magic at +0x24, PIC, end 0x516E90). Payload xz -> 21.7 MB Image,
  "Linux version 4.9.232-1.19 ... gcc 9.3.0 ... #1 SMP".
- DT compatible list includes `brcm,bcm7439b0`. No DTB inside KERN: BOLT passes it.
  So a DTB must be built (HOST-ASSISTED) from the 7439 nodes (memory, cpus, GIC,
  arch timer, UART, chosen/bootargs). Label it HOST-ASSISTED.
- Bootargs seen: KERN header `vmalloc=532m bmem=448m@484m bmem=501m@2568m
  usb-storage.delay_use=7`; BOLT adds root=/TOKEN=. First target:
  `earlycon console=ttyS0,115200 mem=512M` (HOST-ASSISTED, documented).
- initramfs /init (ARM C): mount proc/devtmpfs, parse root= TOKEN=, mount ext4
  /rootdisk, loop0 squashfs /new_root, chroot, exec /lib/systemd/systemd.

## Boot contract (Linux ARM, zImage)
r0=0, r1=~0 (DT machine), r2=DTB phys; MMU off, D-cache off, SVC mode, IRQ/FIQ masked.
Load zImage at phys 0x8000 (RAM base 0). The zImage decompressor runs with the MMU
off first, then turns on a flat map with caches. The kernel then sets up its own tables.

## Minimum CPU for earlyprintk
1. ARM (A32) mode: data-processing incl. shifts and rotate-immediate, MUL/MLA/UMULL/SMULL,
   LDR/STR/LDRB/LDRH/LDRSB/LDRSH/LDRD/STRD, LDM/STM (incl. ^ user-bank), B/BL/BX/BLX,
   MRS/MSR, MOVW/MOVT, CLZ, REV, UBFX/SBFX/BFI/BFC, UXT*/SXT*, LDREX/STREX, DMB/DSB/ISB
   (no-ops), WFI/WFE (idle), SVC, CPS.
2. Thumb-2: needed. A 4.9 kernel built with CONFIG_THUMB2_KERNEL boots in Thumb. The
   initramfs `/init` uses Thumb (blx stubs seen). Do A32 first, then T32.
3. Modes and banking: USR/FIQ/IRQ/SVC/ABT/UND/SYS (+MON/HYP: ignore, enter SVC).
   Banked r13/r14/SPSR per mode; FIQ r8-r12.
4. Exceptions: reset/und/svc/pabt/dabt/irq/fiq with VBAR (cp15 c12) and HIVECS.
5. CP15: MIDR (0x412FC0F? Cortex-A15 r2/r3; read from firmware expectations),
   MPIDR (cpu0, set the U bit for UP), SCTLR (M,C,I,Z,V,TE,AFE), TTBR0/1, TTBCR, DACR,
   DFSR/IFSR/DFAR/IFAR, CONTEXTIDR, TPIDR*, cache/TLB maintenance ops (accept, no-op),
   CCSIDR/CLIDR/CSSELR (the kernel's cache-flush loop reads these), ID_* feature regs.
6. MMU: short-descriptor (sections 1MB, supersections, 4KB/64KB pages, domains, AP,
   XN). LPAE (long-descriptor) needed only if CONFIG_ARM_LPAE; check
   the kernel config strings first (bmem@2568m means memory above 2GB, so it probably
   needs LPAE). Plan: implement short-descriptor first. LPAE comes second if the kernel needs it.
7. VFP/NEON: kernel probes FPSID/MVFR; can report "no VFP" for early boot.
   Userland (glibc armhf) needs VFPv3/NEON. That is a later phase.

## Minimum devices for earlyprintk
- RAM at phys 0 (start 512 MB; add the high bmem windows later).
- UART: brcmstb ns16550, mmio32, reg-shift 2. Take the address from the 7439 DT node
  that we build. Do not guess: confirm against strings/BOLT or the kernel's earlycon.
  THR write goes to log, LSR=0x60.
- GIC-400 (distributor and CPU interface): enough for the kernel to probe it, with no
  interrupt delivery at first.
- ARM generic timer (CNTFRQ/CNTPCT/CNTV_*). The kernel needs it for delay calibration.
  Brcmstb uses 27 MHz.
- Sticky catch-all MMIO for the rest of the 0xF0000000 register space, logging every
  first access (the same scheme as PACE_HOG).

## Phases
1. Skeleton (this commit): register file, modes, banking, condition codes, fetch loop,
   bus interface, unimplemented-insn log. Not wired.
2. A32 integer set + CP15 basics, then run the zImage decompressor
   ("Uncompressing Linux... done" goes to its own UART only if DEBUG_LL; otherwise watch PC
   reach the kernel's stext).
3. MMU short-descriptor + exceptions + timer + GIC stub, then earlycon output.
4. Thumb-2 + VFP for initramfs /init.
