# JAGBE
JAGBE or Just Another GameBoy Emulator for long, is exactly what it sounds like,
the goal of this emulator is to further learn how emulators work (maybe make something that will play more than tetris),
and to try not to stick everything in one file.

Currently passes the following of blargg's tests:

|#|name|success?|
|-|-|-|
|01|special|:white_check_mark:|
|02|interrupts|:x:|
|03|op sp,hl|:x:|
|04|op r,imm|:x:|
|05|op rp|:white_check_mark:|
|06|ld r,r|:white_check_mark:|
|07|jr,jp,call,ret,rst|:x: (doesn't complete)|
|08|misc instrs|:white_check_mark:|
|09|op r,r|:x:|
|10|bit ops|:white_check_mark:|
|11|op a,(hl)|:x:|
