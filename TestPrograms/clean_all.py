#!/usr/bin/python

import os

file_names = [file_name for file_name in os.listdir('.') if not file_name.endswith('.c') and not file_name.endswith('.py')]

for file_name in file_names:
    os.system('rm %s' % file_name)

