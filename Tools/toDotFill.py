import os
def convert(text):
    bytearr = []
    for i in text:
        bytearr.append(ord(i))
    for i in bytearr:
        print(f".FILL {hex(i)} ; {chr(i)}")
    print(".FILL 0x00 ; Null terminator")

convert(input("> "))
os.system("pause")