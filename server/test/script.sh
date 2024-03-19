# Path to the file you want to reset
FILE_PATH="./results.json"

# Reset the file (e.g., clear its contents)
echo "{}" > $FILE_PATH

artillery run test.yaml

node calculate.js