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

|name|state|
|-|-|
|instr_timing|:white_check_mark:|
|halt_bug|:white_check_mark:|

MoonEye tests:

|test|state|comment|
|-|-|-|
|add_sp_e_timing|:x:|Doesn't complete.|
|boot_regs-dmgABCX|:white_check_mark:|-|
|call_cc_timing|:x:|Doesn't complete.|
|call_cc_timing2|:x:|B,C,D Incorrect.|
|call_timing|:x:|Incorrect.|
|call_timing2|:x:|B,C,D Incorrect.|
|di_timing-GS|:white_check_mark:|-|
|div_timing|:white_check_mark:|-|
|ei_timing|:white_check_mark:|-|
|halt_ime0_ei|:white_check_mark:|-|
|halt_ime0_nointr_timing|:x:|D Incorrect.|
|halt_ime1_timing|:white_check_mark:|-|
|halt_ime1_timing2-GS|:x:|E Incorrect.|
|if_ie_registers|:white_check_mark:|-|
|intr_timing|:x:|E Incorrect.|
|jp_cc_timing|:x:|Doesn't complete.|
|jp_timing|:x:|Doesn't complete.|
|ld_hl_sp_e_timing|:x:|Doesn't complete.|
|oam_dma_restart|:white_check_mark:|-|
|oam_dma_start|:x:|B,C,D,E Incorrect.|
|oam_dma_timing|:white_check_mark:|-|
|pop_timing|:white_check_mark:|-|
|push_timing|:x:|D Incorrect.|
|rapid_di_ei|:white_check_mark:|-|
|ret_cc_timing|:x:|Doesn't complete.|
|ret_timing|:x:|Doesn't complete.|
|reti_intr_timing|:white_check_mark:|-|
|reti_timing|:x:|Doesn't complete.|
|rst_timing|:x:|B Incorrect.|
|timer/div_write|:white_check_mark:|-|
|timer/rapid_toggle|:x:| |
|timer/tim00|:x:|E Incorrect.|
|timer/tim00_div_trigger|:x:|E Incorrect.|
|timer/tim01|:white_check_mark:|-|
|timer/tim01_div_trigger|:white_check_mark:|-|
|timer/tim10|:white_check_mark:|-|
|timer/tim10_div_trigger|:white_check_mark:|-|
|timer/tim11|:white_check_mark:|-|
|timer/tim11_div_trigger|:white_check_mark:|-|
|timer/tima_reload|:x:|B,C,H,L Incorrect.|
|timer/tima_write_reloading|:x:|E,L Incorrect.|
|timer/tma_write_reloading|:x:|C,L Incorrect.|
