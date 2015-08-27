#!/usr/bin/python

import os
import os.path

file_names = [file_name for file_name in os.listdir('.') if file_name.endswith('.c')]

for file_name in file_names:
    test_name = file_name[:-2]
    print("Compiling %s" % test_name)
    os.system('gcc %s.c -o %s.gcc > /dev/null 2>&1' % (test_name, test_name))
    os.system('mono ../bin/Debug/C-Compiler.exe %s.c > %s.s' % (test_name, test_name))

    if os.path.isfile('%s.s' % test_name):
        print("    Successfully compiled ^_^")
    else:
        print("    Didn't compile T_T")
        continue

    os.system('gcc %s.s -o %s > /dev/null 2>&1' % (test_name, test_name))

    os.system('./%s.gcc > %s.gcc.out' % (test_name, test_name))
    os.system('./%s > %s.out' % (test_name, test_name))
    if open('%s.gcc.out' % test_name).read() == open('%s.out' % test_name).read():
        print("    Output correct ^_^")
    else:
        print("    Output incorrect T_T")
