import random

MAX_GUESS = 100

randomNumber = random.randint(1, MAX_GUESS )

print("Welcome to the Number Guessing Game, hosted by the Python Host App Sample.")
print("Try to guess the secret number between 1 and", MAX_GUESS)

playerGuess = -1
numGuesses = 0

while randomNumber != playerGuess:
    playerGuess = int(input("Enter an integer from 1 to " + str(MAX_GUESS) + ": "))
    numGuesses += 1

    if playerGuess < randomNumber:
        print("You guessed too low; try again!")
    elif playerGuess > randomNumber:
        print("You guessed too high; try again!")

print("Congratulations! You guessed the number in", numGuesses, "tries.")
input("Press any key to exit...")

