import random

MAX = 99

randNum = random.randint(1, MAX)

playerGuess = int(input("Enter an integer from 1 to " + str(MAX) + ": "))
numGuesses = 1

while randNum != playerGuess:
    print
    if playerGuess < randNum:
        print("You guessed low")
        playerGuess = int(input("Enter an integer from 1 to " + str(MAX) + ": "))
    elif playerGuess > randNum:
        print("You guessed high")
        playerGuess = int(input("Enter an integer from 1 to " + str(MAX) + ": "))
    numGuesses += 1
print("You guessed it in " + str(numGuesses) + " tries!")
input("Press any key to exit")
