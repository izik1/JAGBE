# JAGBE
JAGBE or Just Another GameBoy Emulator for long, is exactly what it sounds like,
the goal of this emulator is to further learn how emulators work (maybe make something that will play more than tetris),
and to try not to stick everything in one file.

Currently passes the following of blargg's instruction tests:

|#|name|success?|
|-|-|-|
|01|special|:white_check_mark:|
|02|interrupts|:x:|
|03|op sp,hl|:white_check_mark:|
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
|boot_regs-dmgABCX|:white_check_mark:|-|
|call_cc_timing|:x:|Doesn't complete.|
|call_cc_timing2|:x:|B,C,D Incorrect.|
|call_timing|:x:|Incorrect.|
|call_timing2|:x:|B,C,D Incorrect.|
|di_timing-GS|:x:|Crashes, unimplemented opcode 0x76 (HALT)|
|div_timing|:white_check_mark:|-|
|ei_timing|:white_check_mark:|-|
|halt_ime0_ei|:x:|Crashes, unimplemented opcode 0x76 (HALT)|
|halt_ime0_nointr_timing|:x:|Crashes, unimplemented opcode 0x76 (HALT)|
|halt_ime1_timing|:x:|Crashes, unimplemented opcode 0x76 (HALT)|
|halt_ime1_timing2-GS|:x:|Crashes, unimplemented opcode 0x76 (HALT)|
|if_ie_registers|:x:|C,E Incorrect.|
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
