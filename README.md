# JAGBE
JAGBE or Just Another GameBoy Emulator for long, is exactly what it sounds like,
the goal of this emulator is to further learn how emulators work (maybe make something that will play more than tetris),
and to try not to stick everything in one file.

Currently passes the following of blargg's instruction tests:

|#|name|state|
|-|-|-|
|01|special|:white_check_mark:|
|02|interrupts|:white_check_mark:|
|03|op sp,hl|:white_check_mark:|
|04|op r,imm|:white_check_mark:|
|05|op rp|:white_check_mark:|
|06|ld r,r|:white_check_mark:|
|07|jr,jp,call,ret,rst|:white_check_mark:|
|08|misc instrs|:white_check_mark:|
|09|op r,r|:white_check_mark:|
|10|bit ops|:white_check_mark:|
|11|op a,(hl)|:white_check_mark:|

Memory Timing:

|#|access type|state|
|-|-|-|
|1|read|:white_check_mark:|
|2|write|:white_check_mark:|
|3|modify|:white_check_mark:|

dmg_audio:

|#|name|state|
|-|-|-|
|01|registers|:x:|
|02|len ctr|:x:|
|03|trigger|:x:|
|04|sweep|:x:|
|05|sweep details|:x:|
|06|overflow on trigger|:x:|
|07|len sweep period sync|:x:|
|08|len ctr during power|:x:|
|09|wave read while on|:x:|
|10|wave trigger while on|:x:|
|11|regs after power|:x:|
|12|wave write while on|:x:|

|name|state|
|-|-|
|instr_timing|:white_check_mark:|
|halt_bug|:white_check_mark:|

MoonEye acceptance tests:

|test|state|comment|
|-|-|-|
|add_sp_e_timing|:white_check_mark:||
|bits/mem_oam|:white_check_mark:||
|bits/reg_f|:white_check_mark:||
|bits/unused_hwio-GS|:white_check_mark:||
|boot_regs-dmgABCX|:white_check_mark:||
|call_cc_timing|:white_check_mark:||
|call_cc_timing2|:white_check_mark:||
|call_timing|:white_check_mark:||
|call_timing2|:white_check_mark:||
|di_timing-GS|:white_check_mark:||
|div_timing|:white_check_mark:||
|ei_sequence|:white_check_mark:||
|ei_timing|:white_check_mark:||
|oam_dma/basic|:white_check_mark:||
|oam_dma/reg_read|:white_check_mark:||
|oam_dma/sources-dmgABCXmgbS|:x:|'Fail: $FE00'|
|ppu/hblank_ly_scx_timing-GS|:x:|'Test Failed'|
|ppu/intr_1_2_timing-GS|:x:|D incorrect.|
|ppu/intr_2_0_timing|:x:|D,E Incorrect|
|ppu/intr_2_mode0_timing|:x:|E Incorrect.|
|ppu/intr_2_mode0_timing_sprites|:x:|Test #00 failed|
|ppu/intr_2_mode3_timing|:x:|E Incorrect.|
|ppu/intr_2_oam_ok_timing|:x:|E Incorrect.|
|ppu/lcdon_timing-dmgABCXmgbS|:x:|Expected $01 got $00.|
|ppu/lcdon_write_timing-GS|:x:|Expected $81 got $00.|
|ppu/stat_irq_blocking|:white_check_mark:||
|ppu/vblank_stat_intr-GS|:x:|D Incorrect.|
|halt_ime0_ei|:white_check_mark:||
|halt_ime0_nointr_timing|:x:|D Incorrect.|
|halt_ime1_timing|:white_check_mark:||
|halt_ime1_timing2-GS|:x:|E Incorrect.|
|if_ie_registers|:white_check_mark:||
|intr_timing|:x:|E Incorrect.|
|interrupts/ie_push|:white_check_mark:|
|jp_cc_timing|:white_check_mark:||
|jp_timing|:white_check_mark:||
|ld_hl_sp_e_timing|:white_check_mark:||
|oam_dma_restart|:white_check_mark:||
|oam_dma_start|:white_check_mark:||
|oam_dma_timing|:white_check_mark:||
|pop_timing|:white_check_mark:||
|push_timing|:white_check_mark:||
|rapid_di_ei|:white_check_mark:||
|ret_cc_timing|:white_check_mark:||
|ret_timing|:white_check_mark:||
|reti_intr_timing|:white_check_mark:||
|reti_timing|:white_check_mark:||
|rst_timing|:white_check_mark:||
|timer/div_write|:white_check_mark:||
|timer/rapid_toggle|:x:|C Incorrect.|
|timer/tim00|:white_check_mark:||
|timer/tim00_div_trigger|:white_check_mark:||
|timer/tim01|:white_check_mark:||
|timer/tim01_div_trigger|:white_check_mark:||
|timer/tim10|:white_check_mark:||
|timer/tim10_div_trigger|:white_check_mark:||
|timer/tim11|:white_check_mark:||
|timer/tim11_div_trigger|:white_check_mark:||
|timer/tima_reload|:x:|B,C Incorrect.|
|timer/tima_write_reloading|:white_check_mark:||
|timer/tma_write_reloading|:white_check_mark:||

mooneye mbc1 tests:

|Name|Status|
|-|-|
|multicart_rom_8Mb|:x:|
|ram_64Kb|:white_check_mark:|
|ram_256Kb|:white_check_mark:|
|rom_1Mb|:white_check_mark:|
|rom_2Mb|:white_check_mark:|
|rom_4Mb|:white_check_mark:|
|rom_8Mb|:white_check_mark:|
|rom_16Mb|:white_check_mark:|
|rom_512Kb|:white_check_mark:|
