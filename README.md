# JAGBE
JAGBE or Just Another GameBoy Emulator for long, is exactly what it sounds like,
the goal of this emulator is to further learn how emulators work (maybe make something that will play more than tetris),
and to try not to stick everything in one file.

Currently passes the following of blargg's instruction tests:

|#|name|success?|
|-|-|-|
|01|special|:white_check_mark:|
|02|interrupts|:x:|
|03|op sp,hl|:x:|
|04|op r,imm|:white_check_mark:|
|05|op rp|:white_check_mark:|
|06|ld r,r|:white_check_mark:|
|07|jr,jp,call,ret,rst|:white_check_mark:|
|08|misc instrs|:white_check_mark:|
|09|op r,r|:white_check_mark:|
|10|bit ops|:white_check_mark:|
|11|op a,(hl)|:white_check_mark:|

|Memory Timing #|success?|
|-|-|
|1|:x:|
|2|:x:|
|3|:x:|

|name|success?|
|-|-|
|instr_timing|:x:|

MoonEye tests:

|test|state|comment|
|-|-|-|
|add_sp_e_timing|:x:|Doesn't complete.|
|boot_regs-dmgABCX|:white_check_mark:|All correct data.|
|call_cc_timing|:x:|Doesn't complete.|
|call_cc_timing2|:x:|B,C,D are wrong.|
|call_timing|:x:|Doesn't complete.|
|call_timing2|:x:|B,C,D are wrong.|
|di_timing-GS|:question:|-|
|div_timing|:question:|-|
|ei_timing|:question:|-|
|halt_ime0_ei|:question:|-|
|halt_ime0_nointr_timing|:question:|-|
|halt_ime1_timing|:question:|-|
|halt_ime1_timing2-GS|:question:|-|
|if_ie_registers|:question:|-|
|intr_timing|:question:|-|
|jp_cc_timing|:question:|-|
|jp_timing|:question:|-|
|ld_hl_sp_e_timing|:question:|-|
|oam_dma_restart|:question:|-|
|oam_dma_start|:question:|-|
|oam_dma_timing|:question:|-|
|pop_timing|:question:|-|
|push_timing|:question:|-|
|rapid_di_ei|:question:|-|
|ret_cc_timing|:question:|-|
|ret_timing|:question:|-|
|reti_intr_timing|:question:|-|
|reti_timing|:question:|-|
|rst_timing|:question:|-|
