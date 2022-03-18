# Pulse

Final Fantasy XIII resource editor


# Files with problems

auto_yus_us.bin
resident\system\txtres.ztr

I have a problem with the **auto_yus_us.bin** file.
The game does not accept text resources files larger than 256kb.
It will crash during loading from any part of the city of yusnan.
# Solution
For the **auto_yus_us.bin** files, use Yusnaan tool **Strings to Ztr** using ZtrCompressor.

**resident\system\txtres.ztr** 
For this file, use **Strings to ZTr Pulse fakecompression**.
This file is incompatible with ZtrCompressor.

## ZtrCompressor
Compression algorithm created by lehieugch68.
Decompression is compatible with all ztr files.
Compression is compatible with most files.

It is incompatible with the resident/system;txtres.ztr file and event\ev_yuaa_010


# Pulse
Created by albeoris.
Decompression compatible with all ztr, imgb, wpd files and videos.
Fakecompression compatible with most files, except auto_yus_us.bin.


## Yusnaan
I created this to make it easier to use the two tools.
Extracting and re-inserting ztr file into the wpd archives in white_imga\dbai\npc\pack.
Decompression and compression of the ztr files.

For image files, for example imgb, use Pulse.exe.
