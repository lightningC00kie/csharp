
def generate_file(file):
    htmlFile = open(file, 'r')
    htmlTemplate = open('./htmlTemplate.txt', 'w')
    lines = htmlFile.readlines()
    for line in lines:
        htmlTemplate.write(add_code(escape_quotes(line)))
        htmlTemplate.write("\n")

def add_code(line):
    return 'WriteLine("' + line[:-1] + '");' if '{' in line or '}' in line else 'WriteLine($"' + line[:-1] + '");'

def escape_quotes(line):
    skip = False
    i = 0
    while i < len(line):
        if skip:
            skip = False
            continue
        if line[i] == '"':
            line = line[:i] + "\\" + line[i:]
            skip = True
            i += 2
            continue
        i += 1
    return line
        

generate_file("./Example/05.html")