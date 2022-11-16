deltaTable = {}
rotations = ["Spawn", "Left", "Right", "Flip"]
currentPiece = 'N'
def getSingleDelta(size, text, index):
    currChar = text[index]
    if currChar == 'X' or currChar == 'x':
        return (0, 0)
    elif currChar == 'v':
        d = getSingleDelta(size, text, index + size)
        return (0 + d[0], 1 + d[1])
    elif currChar == '^':
        d = getSingleDelta(size, text, index - size)
        return (0 + d[0], -1 + d[1])
    elif currChar == '>':
        d = getSingleDelta(size, text, index + 1)
        return (-1 + d[0], 0 + d[1])
    elif currChar == '<':
        d = getSingleDelta(size, text, index - 1)
        return (1 + d[0], 0 + d[1])

# print(getSingleDelta(3, ".v.>X<^..", 2, 0))
with open("rotations.txt", "r") as f:
    while True:
        line = f.readline().strip()
        if not line:
            break
        if line[0] == '#':
            continue
        currentPiece = line[0]
        deltaTable[currentPiece] = []
        size = int(line[1])
        # loop through rotations
        for i in range(4):
            deltaTable[currentPiece].append([])
            # read a single rotation
            currStr = ""
            for _ in range(size):
                currStr += f.readline().strip()
            for j in range(size * size):
                if currStr[j] != '.' and currStr[j] != 'x':
                    deltaTable[currentPiece][i].append(getSingleDelta(size, currStr, j))

fnStr = """private Point[] GetDeltas(Piece p, Rotation r)
{
    Point[] outArr = new Point[4];
    switch (p)
    {
"""

print(deltaTable)
for piece, rots in deltaTable.items():
    fnStr += " "*4*2 + "case Piece." + piece + ":\n"
    fnStr += " "*4*3 + "switch (r)\n"
    fnStr += " "*4*3 + "{\n"
    for i in range(len(rots)):
        fnStr += " "*4*4 + "case Rotation." + rotations[i] + ":\n"
        for j in range(len(rots[i])):
            fnStr += " "*4*5 + "outArr[%d] = new Point(%d, %d);\n" % (j, rots[i][j][0], rots[i][j][1])
        fnStr += " "*4*5 + "break;\n"
    fnStr += " "*4*3 + "}\n"
    fnStr += " "*4*2 + "break;\n"
fnStr += """    }
    return outArr;
}"""

fnStr2 = ""
# add 2 tabs before each line
for line in fnStr.split("\n"):
    fnStr2 += " "*4*2 + line + "\n"
fnStr = fnStr2
# import json
# print(json.dumps(deltaTable, sort_keys=True, indent=2))
print(fnStr)

import re

with open("../TetrisGame.cs", "r+") as f:
    fileStr = f.read()
    fileStr = re.sub("(?<=\/\/ Start GetDeltas Function\n)(.|\n)*(?=\n\s+\/\/ End GetDeltas Function)", fnStr, fileStr)
    f.seek(0)
    f.write(fileStr)
    f.truncate()
