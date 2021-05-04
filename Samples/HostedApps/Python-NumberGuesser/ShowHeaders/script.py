import sys
import urllib.request

if (len(sys.argv) < 2):
    print("This app expects to be called from a 'Share' operation.")
    input("Press enter to exit...")
    exit()

url = urllib.request.urlopen(sys.argv[1])
print("Final URL was", url.url)
print("Status code was", url.status)
print("Headers follow...")
print(url.headers)
input("Press enter to exit...")
