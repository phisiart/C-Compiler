import os

file_names = [file_name for file_name in os.listdir(".") if file_name.endswith('.dot')]
commands = ["dot -Tsvg " + file_name + " -o " + file_name[:-3] + "svg" for file_name in file_names]

for command in commands:
    print(command)
    os.system(command)